# Appilico Server — VPS Deployment Guide

## Prerequisites
- VPS with Ubuntu 22.04+ (or similar)
- Docker & Docker Compose installed
- Domain `api.appilico.com` DNS A record pointing to your VPS IP
- Ports 80 and 443 open in firewall

---

## Step 1: Clone the repo on VPS

```bash
ssh root@YOUR_VPS_IP
mkdir -p /opt/appilico
cd /opt/appilico
git clone https://github.com/Sadramst/appilico-server.git server
cd server
```

## Step 2: Create `.env` file

```bash
cp .env.example .env
nano .env
```

Fill in your real values:
```
POSTGRES_USER=appilico
POSTGRES_PASSWORD=YourStr0ngP@ssw0rd!Here
DB_CONNECTION_STRING=Host=postgres;Database=appilicodb;Username=appilico;Password=YourStr0ngP@ssw0rd!Here;
JWT_SECRET=YourSuperSecretKeyThatIsAtLeast32CharactersLong!ChangeMe
CLOUDINARY_CLOUD_NAME=your-actual-cloud-name
CLOUDINARY_API_KEY=your-actual-api-key
CLOUDINARY_API_SECRET=your-actual-api-secret
```

**Important:** The `POSTGRES_PASSWORD` in `DB_CONNECTION_STRING` must match the `POSTGRES_PASSWORD` value.

## Step 3: Open firewall ports

```bash
ufw allow 80/tcp
ufw allow 443/tcp
ufw allow 22/tcp
ufw enable
```

## Step 4: Start with HTTP only (for certbot challenge)

The initial `nginx/conf.d/api.conf` only serves HTTP — this is needed for certbot to verify domain ownership.

```bash
docker compose up -d
```

Verify it's running:
```bash
docker compose ps
docker compose logs backend --tail 50
```

Test HTTP is working (from your local machine):
```bash
curl http://api.appilico.com/.well-known/acme-challenge/test
# Should return 404 (file not found) — NOT connection refused
# If connection refused → DNS hasn't propagated or firewall is blocking port 80
```

## Step 5: Get SSL certificate

```bash
docker compose run --rm certbot certonly \
  --webroot \
  --webroot-path=/var/www/certbot \
  -d api.appilico.com \
  --email m.sadra.mst@gmail.com \
  --agree-tos \
  --no-eff-email
```

If successful, you'll see: `Congratulations! Your certificate and chain have been saved`

## Step 6: Switch to SSL Nginx config

```bash
# Replace HTTP-only config with SSL config
cp nginx/conf.d/api.ssl.conf nginx/conf.d/api.conf

# Restart nginx to pick up SSL
docker compose restart nginx
```

## Step 7: Verify everything

```bash
# Check all containers are running
docker compose ps

# Test HTTPS
curl https://api.appilico.com/api/products?pageSize=1

# Test Swagger
curl -I https://api.appilico.com/swagger/index.html
```

Open in browser:
- ✅ https://api.appilico.com/swagger/index.html → Swagger UI
- ✅ https://api.appilico.com/api/products → Products JSON

## Step 8: Set up auto-renewal for SSL

```bash
# Create a cron job to renew certificates
crontab -e
```

Add this line:
```
0 3 * * * cd /opt/appilico/server && docker compose run --rm certbot renew --quiet && docker compose restart nginx
```

---

## Troubleshooting

### "no such service: certbot"
Your docker-compose.yml was outdated. Pull the latest code: `git pull`

### certbot fails with "connection refused" or "unauthorized"
- DNS hasn't propagated. Check: `dig api.appilico.com` — should show your VPS IP
- Port 80 is blocked: `ufw status` — make sure 80/tcp is ALLOW
- Nginx isn't running: `docker compose logs nginx`

### Swagger not loading
- Check backend is running: `docker compose logs backend --tail 20`
- Check nginx is proxying: `docker compose logs nginx --tail 20`

### Database connection fails
- Check postgres is healthy: `docker compose ps` — postgres should show "healthy"
- Verify password matches between `POSTGRES_PASSWORD` and `DB_CONNECTION_STRING`

### View logs
```bash
docker compose logs backend --tail 100 -f    # API logs (live)
docker compose logs nginx --tail 50           # Nginx logs
docker compose logs postgres --tail 50        # Database logs
```

### Rebuild after code changes
```bash
git pull
docker compose build backend
docker compose up -d
```
