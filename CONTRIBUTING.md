# Contributing

## Development Baseline

Use .NET 8 and keep the existing layered dependency direction:

- API may reference Business, DataAccess, and Domain.
- Business may reference Domain only for repository/entity contracts.
- DataAccess may reference Domain.
- Domain must stay framework-light and must not reference infrastructure projects.

## Local Checks

Run these before opening a PR or deploying:

```bash
dotnet restore AppilicoShopServer.sln
dotnet build AppilicoShopServer.sln --no-restore
dotnet test AppilicoShopServer.sln --no-build
dotnet list AppilicoShopServer.sln package --vulnerable --include-transitive
```

Live API tests are opt-in only:

```powershell
$env:APPILICO_API_BASE_URL="https://api.appilico.com"
dotnet test tests/AppilicoShopServer.IntegrationTests/AppilicoShopServer.IntegrationTests.csproj -p:RunLiveApiTests=true
```

## Database Changes

When changing EF entities or configurations, add a migration from the solution root:

```bash
dotnet ef migrations add DescriptiveMigrationName --project src/AppilicoShopServer.DataAccess --startup-project src/AppilicoShopServer.API
```

Review the generated migration and model snapshot before committing.

## Integration Rules

- Stripe-backed flows must fail safely and must not mark local payments, orders, or subscriptions successful until provider confirmation is available.
- Webhooks must be idempotent and signature-verified.
- Azure Blob files must remain private; expose downloads through short-lived SAS URLs.
- Non-critical email work should stay off the request path. The current queue is in-process and intentionally simple.

## API Contract Rules

Prefer additive DTO changes. Avoid route-breaking changes unless a versioning plan is included. Swagger should continue to describe JWT bearer auth, common error responses, and `X-Correlation-Id` support.

## Production Caution

Before deploying to `api.appilico.com`, verify the live VPS checkout and container image are built from this repository. Historical investigation suggested the public API may have been served from a different source tree.
