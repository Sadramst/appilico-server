# Appilico Server Deployment

This is the production deployment and operations checklist for the current repository. The older VPS walkthrough remains in [DEPLOY.md](DEPLOY.md); this file is the higher-level release checklist used with the CI/deploy workflow.

## Source-Of-Truth Gate

Before deploying to `api.appilico.com`, verify the VPS checkout, branch, container image, and public Swagger/API surface match this repository. A prior production check suggested the live API may not have been built from this repo, so do not treat a successful deploy command as proof of live parity.

## Required Verification

Run locally or in CI before deploy:

```bash
dotnet restore Appilico.Server.sln
dotnet build Appilico.Server.sln --configuration Release --no-restore
dotnet test Appilico.Server.sln --configuration Release --no-build
dotnet list Appilico.Server.sln package --vulnerable --include-transitive
```

The GitHub Actions deploy workflow now runs restore, build, test, and vulnerable-package scan before the SSH deployment job.

## Database Migration

Apply EF migrations before or during rollout:

```bash
dotnet ef database update --project src/Appilico.Server.DataAccess --startup-project src/Appilico.Server.API
```

The current final-phase migration is `AddProviderWebhookEvents`. It adds:

- `ExternalWebhookEvents` for Stripe webhook idempotency.
- `Refunds.ProviderRefundId` for provider refund traceability.

## Environment Variables

Start from [.env.example](.env.example). Key production groups:

- `DB_CONNECTION_STRING`, `POSTGRES_PASSWORD`, `JWT_SECRET`
- `CLOUDINARY_CLOUD_NAME`, `CLOUDINARY_API_KEY`, `CLOUDINARY_API_SECRET`
- `STRIPE_ENABLED`, `STRIPE_SECRET_KEY`, `STRIPE_PUBLISHABLE_KEY`, `STRIPE_WEBHOOK_SECRET`, `STRIPE_CURRENCY`
- `STRIPE_STARTER_PRICE_ID`, `STRIPE_PROFESSIONAL_PRICE_ID`, `STRIPE_ENTERPRISE_PRICE_ID` for paid subscriptions
- `AZURE_STORAGE_ENABLED`, `AZURE_STORAGE_CONNECTION_STRING`, `AZURE_STORAGE_CONTAINER_NAME`
- `EMAIL_ENABLED`, SMTP settings, `EMAIL_NOTIFY_EMAIL`, `EMAIL_QUEUE_CAPACITY`

Provider integrations stay disabled unless their feature flag is true and required settings are present.

## Health Checks

- `GET /health/live`: process/container liveness.
- `GET /health/ready`: database and configured provider readiness.
- `GET /health`: legacy-compatible health endpoint.

Docker images include a curl-based health check against `/health/live`, and `docker-compose.yml` exposes the same check for the backend service.

## Deployment

Manual VPS deployment:

```bash
cd /opt/appilico/server
git pull origin main
cd /opt/appilico
docker compose up -d --build backend
backend_container=$(docker compose ps -q backend)
docker inspect --format='{{.State.Health.Status}}' "$backend_container"
docker compose restart nginx
```

The GitHub Actions deploy workflow performs the same backend rebuild and waits for the backend container to become healthy before restarting nginx.

## Rollback

1. Identify the last known-good commit or image.
2. Revert the VPS checkout to that commit.
3. Run `docker compose up -d --build backend`.
4. Verify `/health/live`, `/health/ready`, and one representative authenticated API flow.
5. Only roll database migrations backward if the rollback requires schema removal and the data impact is understood.

## Post-Deploy Smoke Test

```bash
curl -fsS https://api.appilico.com/health/live
curl -fsS https://api.appilico.com/health/ready
curl -I https://api.appilico.com/swagger/index.html
```

Swagger is disabled in Production unless `SWAGGER_ENABLED=true`.
