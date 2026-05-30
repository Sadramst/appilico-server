# AppilicoShopServer Architecture

This repository uses a layered .NET 8 architecture. Phase 2/3 tightened the
layer boundaries while keeping public controller contracts stable. Phase 4/5
added provider-backed integrations and operational readiness without changing
the project dependency direction.

## Dependency Direction

```text
AppilicoShopServer.API
  -> AppilicoShopServer.Business
  -> AppilicoShopServer.Domain

AppilicoShopServer.DataAccess
  -> AppilicoShopServer.Domain
```

The API project is the composition root. It references DataAccess to register EF
Core, Identity stores, repositories, and infrastructure implementations. The
Business project no longer references DataAccess and must not import
`AppDbContext` or `Microsoft.EntityFrameworkCore`.

## Layer Responsibilities

- Domain: entities, enums, constants, and repository/service abstractions.
- DataAccess: `AppDbContext`, EF configurations, repository implementations,
  Unit of Work implementation, and EF-backed access checks.
- Business: DTOs, validators, mappings, orchestration, authorization-aware
  business rules, integration policy decisions, and service contracts.
- API: controllers, middleware, startup composition, HTTP pipeline, Swagger,
  CORS, rate limiting, and deployment-time configuration validation.

## Integration Boundaries

- Payments and subscriptions flow through `IStripeService`; Business owns local
  state transitions while the Stripe adapter owns SDK calls, signature checks,
  idempotency keys, and provider exception mapping.
- Stripe webhooks are idempotent through `ExternalWebhookEvents`, uniquely keyed
  by provider and event id.
- File operations flow through `IFileStorageService`; Development uses local
  storage and Production uses Azure Blob Storage with private containers and SAS
  download URLs.
- Email sending flows through `IEmailService`; contact and waitlist emails are
  queued through `IEmailWorkQueue` so non-critical SMTP failures do not block
  request completion.

## Persistence Pattern

Business services depend on `IUnitOfWork` and Domain repository interfaces. New
features should add query/command methods to Domain interfaces and implement
them in DataAccess instead of querying EF directly from Business.

Repository abstractions introduced or expanded in Phase 2/3 include:

- `IRefreshTokenRepository`
- `IBlogPostRepository`
- `IVisualRepository`
- `ISubscriptionRepository`
- `INewsletterSubscriberRepository`
- `IWaitlistRepository`
- `IContactMessageRepository`
- `IWishlistRepository.GetSoftDeletedByCustomerAndProductAsync`
- `IAccessControlService` moved to Domain and implemented in DataAccess

## API Composition

`Program.cs` is intentionally small. Startup wiring now lives in
`src/AppilicoShopServer.API/Extensions/AppilicoApiExtensions.cs`:

- logging
- persistence and Identity
- validated options
- JWT authentication
- application services
- external provider selection
- Swagger/CORS/rate limiting/controllers
- middleware and health endpoint mapping

## Dependency Modernization

The project remains on .NET 8. Packages were updated to current safe patch/minor
versions on that line where available, and AutoMapper was moved to the current
non-vulnerable package line after the deprecated DI package was removed.

Current package scan result:

- No vulnerable packages found, including transitive dependencies.
- No available patch/minor updates found.
- Known deferred legacy packages:
  - `FluentValidation.AspNetCore`: retained because replacing MVC auto-validation
    is a behavior change and should be handled in a dedicated validation pass.
  - `xunit`: v3 migration is a test-platform migration and should be planned
    separately from production architecture cleanup.

## Transitional Notes

- `Npgsql.EnableLegacyTimestampBehavior` remains enabled in `Program.cs`. Remove
  it only after auditing persisted timestamp values and confirming every write
  path uses UTC-compatible values.
- Stripe and Azure integrations are implemented but remain disabled unless their
  feature flags and provider settings are configured.
- The email queue is in-process. Replace it with a durable queue if delivery
  guarantees must survive restarts, deployments, or process crashes.
- Live API tests remain opt-in via `RunLiveApiTests=true` and
  `APPILICO_API_BASE_URL`.

## Adding a Feature Safely

1. Put domain entities/enums/contracts in Domain.
2. Add persistence query/command methods to Domain repository interfaces.
3. Implement EF-specific behavior in DataAccess repositories.
4. Orchestrate business rules in Business services.
5. Register implementation details in the API composition root.
6. Cover service behavior with unit tests and HTTP behavior with integration
   tests when authorization, routing, or DI wiring is involved.