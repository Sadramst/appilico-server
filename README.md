# AppilicoShopServer

Full-featured e-commerce backend API built with ASP.NET Core 8, Entity Framework Core 8, and PostgreSQL.

## Architecture

Clean layered architecture with one-way project dependencies:

```
AppilicoShopServer.sln
├── src/
│   ├── AppilicoShopServer.Domain       # Entities, Enums, Interfaces
│   ├── AppilicoShopServer.DataAccess   # EF Core DbContext, Repositories, Configs
│   ├── AppilicoShopServer.Business     # DTOs, Services, Validators, Mappings
│   └── AppilicoShopServer.API          # Controllers, Middleware, Seed Data
└── tests/
    ├── AppilicoShopServer.UnitTests
    ├── AppilicoShopServer.IntegrationTests
    └── AppilicoShopServer.DataAccess.Tests
```

`AppilicoShopServer.Business` depends on Domain abstractions only; it does not
reference DataAccess or `AppDbContext`. The API project is the composition root
and wires DataAccess implementations at startup. See [ARCHITECTURE.md](ARCHITECTURE.md)
for the dependency map, repository pattern, and modernization notes.

## Tech Stack

- **Runtime**: .NET 8 / ASP.NET Core 8
- **ORM**: Entity Framework Core 8 (PostgreSQL via Npgsql)
- **Auth**: ASP.NET Identity + JWT Bearer + Refresh Tokens
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Logging**: Serilog (Console + File)
- **API Docs**: Swagger / Swashbuckle
- **Rate Limiting**: AspNetCoreRateLimit
- **Payments**: Stripe PaymentIntents, refunds, subscriptions, and webhooks
- **Storage**: Local development storage or private Azure Blob Storage with SAS downloads
- **Testing**: xUnit, Moq, FluentAssertions
- **Containerization**: Docker + Docker Compose

## Features

- **Products**: CRUD, search, filtering, variants, SKU management
- **Categories**: Hierarchical tree, CRUD
- **Brands**: CRUD
- **Customers**: Profile, addresses, loyalty points, membership tiers
- **Auth**: Register, login, JWT + refresh tokens, forgot/reset password, role-based (Admin/Manager/Customer)
- **Orders**: Cart-to-order, status tracking, history, cancellation
- **Cart**: Add/update/remove items, auto-create
- **Discounts**: Code-based, percentage/fixed, validation
- **Vouchers**: Code-based, validation, redemption tracking
- **Special Offers**: Time-based offers with product associations
- **Payments**: Stripe-backed card/debit PaymentIntents, offline pending payments, provider-confirmed refunds
- **Reviews**: Product reviews with approval workflow
- **Wishlist**: Add/remove products
- **Inventory**: Stock adjustments, low-stock alerts, transaction history
- **Dashboard**: Sales summary, top products, revenue charts, customer stats
- **Settings**: Key-value app settings with grouping
- **Operations**: `/health/live`, `/health/ready`, correlation IDs, CI build/test/package scan

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/) (or Docker)

### Local Development

```bash
# Clone and navigate
cd server

# Restore packages
dotnet restore

# Update connection string via user secrets, environment variables, or appsettings.Development.json
# Example: Host=localhost;Database=appilicodb_dev;Username=appilico;Password=development-only-password

# Run database migrations
dotnet ef database update --project src/AppilicoShopServer.DataAccess --startup-project src/AppilicoShopServer.API

# Run the API
dotnet run --project src/AppilicoShopServer.API
```

The API will be available at the configured ASP.NET Core URL. Swagger is enabled in Development and disabled by default in Production unless `Swagger:Enabled=true`.

Health endpoints:

- `GET /health/live` for container/process liveness.
- `GET /health/ready` for database and configured integration readiness.
- `GET /health` remains available for backward-compatible health checks.

### Docker

```bash
# Build and run with Docker Compose
docker-compose up -d

# API available at http://localhost:5000/swagger
```

### Run Tests

```bash
dotnet test AppilicoShopServer.sln --no-restore

# Optional live-server tests; requires a running API and is disabled by default
$env:APPILICO_API_BASE_URL="https://api.appilico.com"
dotnet test tests/AppilicoShopServer.IntegrationTests/AppilicoShopServer.IntegrationTests.csproj -p:RunLiveApiTests=true
```

## Default Seed Accounts

| Role     | Email                  | Password       |
|----------|------------------------|----------------|
| Admin    | admin@appilico.com     | Admin@123!     |
| Manager  | manager@appilico.com   | Manager@123!   |
| Customer | customer@appilico.com  | Customer@123!  |

Some seed data uses numbered customer accounts such as `customer1@appilico.com` for integration scenarios.

## Production Safety

- Production startup validates the database connection string and JWT settings.
- Stripe, Azure Blob Storage, SMTP email, and production Swagger are disabled unless explicitly enabled and configured.
- Stripe card/debit payments create pending local payments and require client confirmation plus webhook settlement before an order is marked paid.
- Stripe refunds are requested from the provider before local refund/payment/order state is changed.
- Paid subscriptions are created as provider-backed subscriptions and do not grant paid access until Stripe reports an active/trialing subscription.
- Azure Blob visual downloads use private blobs with short-lived SAS URLs.
- Contact and waitlist emails are queued in-process after persistence succeeds; use a durable queue later if email delivery must survive process restarts.
- See [SECURITY.md](SECURITY.md) before deploying if this repository history may contain exposed secrets.

## API Endpoints

### Auth
- `POST /api/auth/register` — Register
- `POST /api/auth/login` — Login
- `POST /api/auth/refresh` — Refresh token
- `POST /api/auth/revoke` — Revoke token
- `GET /api/auth/profile` — Get profile
- `PUT /api/auth/profile` — Update profile
- `POST /api/auth/forgot-password` — Forgot password
- `POST /api/auth/reset-password` — Reset password

### Products
- `GET /api/products` — Search/list products
- `GET /api/products/{id}` — Get by ID
- `GET /api/products/sku/{sku}` — Get by SKU
- `GET /api/products/featured` — Featured products
- `POST /api/products` — Create (Admin/Manager)
- `PUT /api/products/{id}` — Update (Admin/Manager)
- `DELETE /api/products/{id}` — Delete (Admin)
- `POST /api/products/{id}/variants` — Add variant (Admin/Manager)

### Categories
- `GET /api/categories` — List all
- `GET /api/categories/tree` — Category tree
- `GET /api/categories/{id}` — Get by ID
- `POST /api/categories` — Create (Admin/Manager)
- `PUT /api/categories/{id}` — Update (Admin/Manager)
- `DELETE /api/categories/{id}` — Delete (Admin)

### Orders
- `GET /api/orders` — List all (Admin/Manager)
- `GET /api/orders/{id}` — Get by ID
- `GET /api/orders/my` — My orders
- `POST /api/orders` — Create from cart
- `PUT /api/orders/{id}/status` — Update status (Admin/Manager)
- `GET /api/orders/{id}/history` — Status history
- `POST /api/orders/{id}/cancel` — Cancel

### Cart
- `GET /api/cart` — Get cart
- `POST /api/cart/items` — Add item
- `PUT /api/cart/items/{id}` — Update item
- `DELETE /api/cart/items/{id}` — Remove item
- `DELETE /api/cart` — Clear cart

### Dashboard (Admin/Manager)
- `GET /api/dashboard/sales-summary`
- `GET /api/dashboard/top-products`
- `GET /api/dashboard/revenue-chart`
- `GET /api/dashboard/customer-stats`

> See Swagger UI for complete API documentation with request/response schemas.

## Project Patterns

- **Repository + Unit of Work**: Business services use Domain repository abstractions through `IUnitOfWork`
- **Soft Delete**: `IsDeleted` flag on all entities
- **Audit Trail**: `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`
- **API Response Wrapper**: `ApiResponse<T>` with `Success`, `Message`, `Data`, `Pagination`
- **Global Exception Handling**: Middleware catches all unhandled exceptions
- **Role-Based Authorization**: Admin, Manager, Customer roles

## Dependency Health

The normal package health check is:

```bash
dotnet list AppilicoShopServer.sln package --vulnerable --include-transitive
dotnet list AppilicoShopServer.sln package --outdated --highest-minor
```

As of the Phase 2/3 modernization pass, there are no vulnerable packages and no
available patch/minor updates. Deferred legacy migrations are documented in
[ARCHITECTURE.md](ARCHITECTURE.md).

## License

MIT
