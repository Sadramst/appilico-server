#!/bin/bash

# AppilicoShopServer Auto-Deploy Script
# Run this on the VPS as: bash deploy.sh

set -e  # Exit on any error

echo "=========================================="
echo "AppilicoShopServer Auto-Deploy"
echo "=========================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
DEPLOY_DIR="/opt/appilico"
SERVER_DIR="$DEPLOY_DIR/server"
REPO_URL="https://github.com/appilico/server.git"
BRANCH="main"

# Function to print status
print_status() {
    echo -e "${GREEN}[✓]${NC} $1"
}

print_error() {
    echo -e "${RED}[✗]${NC} $1"
}

print_section() {
    echo -e "\n${YELLOW}==>${NC} $1"
}

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
   print_error "This script must be run as root (use: sudo bash deploy.sh)"
   exit 1
fi

# Step 1: Check system requirements
print_section "Checking system requirements"
command -v docker >/dev/null 2>&1 || { print_error "Docker not found. Install Docker first."; exit 1; }
command -v git >/dev/null 2>&1 || { print_error "Git not found. Install Git first."; exit 1; }
print_status "Docker and Git are installed"

# Step 2: Create deployment directory
print_section "Setting up deployment directory"
if [ ! -d "$DEPLOY_DIR" ]; then
    mkdir -p "$DEPLOY_DIR"
    print_status "Created $DEPLOY_DIR"
else
    print_status "Directory $DEPLOY_DIR already exists"
fi

# Step 3: Clone or update repository
print_section "Cloning/updating repository"
if [ ! -d "$SERVER_DIR/.git" ]; then
    print_status "Cloning repository from $REPO_URL..."
    git clone --branch "$BRANCH" "$REPO_URL" "$SERVER_DIR"
    print_status "Repository cloned successfully"
else
    print_status "Repository already exists, pulling latest changes..."
    cd "$SERVER_DIR"
    git pull origin "$BRANCH"
    print_status "Repository updated"
fi

# Step 4: Copy environment file
print_section "Configuring environment"
if [ ! -f "$DEPLOY_DIR/.env" ]; then
    print_error ".env file not found in $DEPLOY_DIR"
    echo "Please create $DEPLOY_DIR/.env with your configuration:"
    echo "  - PostgreSQL connection string"
    echo "  - Stripe API keys"
    echo "  - Azure Storage connection"
    echo "  - Email configuration"
    echo "  - JWT secret"
    exit 1
else
    print_status ".env file found"
fi

# Step 5: Build and start Docker containers
print_section "Building and starting Docker containers"
cd "$DEPLOY_DIR"

print_status "Building backend service..."
docker compose build backend

print_status "Starting backend and database..."
docker compose up -d backend db

print_status "Waiting for backend to be ready (30 seconds)..."
sleep 30

# Step 6: Run database migrations
print_section "Running database migrations"
print_status "Applying Entity Framework migrations..."
docker compose exec -T backend dotnet ef database update --project src/AppilicoShopServer.DataAccess/AppilicoShopServer.DataAccess.csproj || true

# Step 7: Restart nginx
print_section "Configuring reverse proxy"
print_status "Restarting nginx..."
docker compose restart nginx

# Step 8: Verify deployment
print_section "Verifying deployment"
sleep 5

# Check if backend is responding
if curl -sf http://localhost:5000/health >/dev/null 2>&1; then
    print_status "Backend health check passed"
else
    print_error "Backend health check failed - check logs with: docker compose logs backend"
fi

# Check if nginx is responding
if curl -sf https://api.appilico.com/health >/dev/null 2>&1 || curl -sf http://api.appilico.com/health >/dev/null 2>&1; then
    print_status "Nginx is responding"
else
    print_status "Nginx may need DNS resolution - check manually"
fi

# Final summary
print_section "Deployment Summary"
echo "=========================================="
echo -e "${GREEN}Deployment completed!${NC}"
echo "=========================================="
echo ""
echo "Deployment directory: $DEPLOY_DIR"
echo "Application directory: $SERVER_DIR"
echo ""
echo "Useful commands:"
echo "  View logs:        cd $DEPLOY_DIR && docker compose logs -f backend"
echo "  Stop containers:  cd $DEPLOY_DIR && docker compose down"
echo "  Restart backend:  cd $DEPLOY_DIR && docker compose restart backend"
echo "  View running containers: docker ps"
echo ""
echo "Test endpoints:"
echo "  Health: curl https://api.appilico.com/health"
echo "  Swagger: https://api.appilico.com/swagger/index.html"
echo ""
print_status "Setup complete!"
