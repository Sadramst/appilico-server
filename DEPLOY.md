# AppilicoShopServer — VPS Deployment Guide

For the current CI/deployment checklist, migration notes, provider environment
variables, health checks, and rollback process, also see [DEPLOYMENT.md](DEPLOYMENT.md).

> **Production source check required:** During the Phase 0 audit, the public
> `https://api.appilico.com` Swagger document appeared to describe a different
> API than this repository. Before deploying, verify the VPS checkout remote,
> branch, running container image, and public API surface match this repo.

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
git clone https://github.com/Sadramst/appilico-server.git backend
cd backend
```

## Step 2: Create `.env` file

```bash
cd /opt/appilico
nano .env
```

Fill in your real values:
```
POSTGRES_USER=appilico
POSTGRES_PASSWORD=YourStr0ngP@ssw0rd!Here
DB_CONNECTION_STRING=Host=postgres;Database=appilicodb;Username=appilico;Password=YourStr0ngP@ssw0rd!Here;
JWT_SECRET=CHANGE_ME_AT_LEAST_32_CHARS_RANDOM_STRING
SWAGGER_ENABLED=false
STRIPE_ENABLED=false
AZURE_STORAGE_ENABLED=false
EMAIL_ENABLED=false
CLOUDINARY_CLOUD_NAME=your-actual-cloud-name
CLOUDINARY_API_KEY=your-actual-api-key
CLOUDINARY_API_SECRET=your-actual-api-secret
STRIPE_CURRENCY=aud
STRIPE_STARTER_PRICE_ID=price_...
STRIPE_PROFESSIONAL_PRICE_ID=price_...
STRIPE_ENTERPRISE_PRICE_ID=price_...
AZURE_STORAGE_CONTAINER_NAME=visuals
EMAIL_NOTIFY_EMAIL=info@appilico.com.au
```

**Important:** The `POSTGRES_PASSWORD` in `DB_CONNECTION_STRING` must match the `POSTGRES_PASSWORD` value.
Do not commit the filled `.env` file or paste it into logs/chats.

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
cd /opt/appilico
docker compose up -d --build api
```

Verify it's running:
```bash
docker compose ps
docker compose logs api --tail 50
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
docker restart appilico-nginx
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
- https://api.appilico.com/api/products -> Products JSON
- Swagger is disabled by default in Production. Temporarily set `SWAGGER_ENABLED=true` only when you intentionally need public Swagger.

## Step 8: Set up auto-renewal for SSL

```bash
# Create a cron job to renew certificates
crontab -e
```

Add this line:
```
0 3 * * * cd /opt/appilico && docker compose run --rm certbot renew --quiet && docker restart appilico-nginx
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
- Production Swagger is disabled unless `SWAGGER_ENABLED=true`.
- Check API is running: `docker compose logs api --tail 20`
- Check nginx is proxying: `docker logs appilico-nginx --tail 20`

### 502 from nginx after deploy
- The API listens on port 8080 inside the container; production nginx must proxy to `api:8080`.
- `appilico-nginx` is a standalone container on the VPS, not always a compose service. Attach it to the API network if DNS lookup for `api` fails.
- The API must run with `ASPNETCORE_ENVIRONMENT=Production` and a valid `JWT_SECRET`; missing JWT configuration will prevent startup.

### Database connection fails
- Check postgres is healthy: `docker compose ps` — postgres should show "healthy"
- Verify password matches between `POSTGRES_PASSWORD` and `DB_CONNECTION_STRING`

### View logs
```bash
docker compose logs api --tail 100 -f        # API logs (live)
docker logs appilico-nginx --tail 50         # Nginx logs
docker compose logs postgres --tail 50        # Database logs
```

### Rebuild after code changes
```bash
cd /opt/appilico
docker compose up -d --build api
docker network connect appilico_default appilico-nginx || true
docker restart appilico-nginx
```

Prefer the GitHub Actions archive deploy for normal releases; avoid VPS `git pull` unless Git credentials are known to be configured.

### Run verification before deploy
```bash
dotnet test AppilicoShopServer.sln --no-restore
dotnet list AppilicoShopServer.sln package --vulnerable --include-transitive
```

The dependency scan should report no vulnerable packages before deploying.

Optional live API tests are excluded from normal runs. To run them deliberately:
```powershell
$env:APPILICO_API_BASE_URL="https://api.appilico.com"
dotnet test tests/AppilicoShopServer.IntegrationTests/AppilicoShopServer.IntegrationTests.csproj -p:RunLiveApiTests=true
```

### Secret rotation
If this repo history or deployment logs ever contained real secrets, rotate the
database password, JWT secret, Cloudinary credentials, VPS deploy secret, and any
development database credentials before deploying. See [SECURITY.md](SECURITY.md).
