# Appilico E-Commerce Backend

Full-featured e-commerce backend API built with ASP.NET Core 8, Entity Framework Core 8, and SQL Server.

## Architecture

Clean layered architecture:

```
Appilico.Server.sln
├── src/
│   ├── Appilico.Server.Domain       # Entities, Enums, Interfaces
│   ├── Appilico.Server.DataAccess   # EF Core DbContext, Repositories, Configs
│   ├── Appilico.Server.Business     # DTOs, Services, Validators, Mappings
│   └── Appilico.Server.API          # Controllers, Middleware, Seed Data
└── tests/
    ├── Appilico.Server.UnitTests
    ├── Appilico.Server.IntegrationTests
    └── Appilico.Server.DataAccess.Tests
```

## Tech Stack

- **Runtime**: .NET 8 / ASP.NET Core 8
- **ORM**: Entity Framework Core 8 (SQL Server)
- **Auth**: ASP.NET Identity + JWT Bearer + Refresh Tokens
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Logging**: Serilog (Console + File)
- **API Docs**: Swagger / Swashbuckle
- **Rate Limiting**: AspNetCoreRateLimit
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
- **Payments**: Process payments, refunds
- **Reviews**: Product reviews with approval workflow
- **Wishlist**: Add/remove products
- **Inventory**: Stock adjustments, low-stock alerts, transaction history
- **Dashboard**: Sales summary, top products, revenue charts, customer stats
- **Settings**: Key-value app settings with grouping

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (or Docker)

### Local Development

```bash
# Clone and navigate
cd server

# Restore packages
dotnet restore

# Update connection string in appsettings.json
# Default: Server=localhost;Database=AppilicoDB;Trusted_Connection=True;TrustServerCertificate=True;

# Run database migrations
dotnet ef database update --project src/Appilico.Server.DataAccess --startup-project src/Appilico.Server.API

# Run the API
dotnet run --project src/Appilico.Server.API
```

The API will be available at `https://localhost:5001` with Swagger UI at `/swagger`.

### Docker

```bash
# Build and run with Docker Compose
docker-compose up -d

# API available at http://localhost:5000/swagger
```

### Run Tests

```bash
dotnet test
```

## Default Seed Accounts

| Role     | Email                  | Password       |
|----------|------------------------|----------------|
| Admin    | admin@appilico.com     | Admin@123!     |
| Manager  | manager@appilico.com   | Manager@123!   |
| Customer | customer@appilico.com  | Customer@123!  |

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

- **Repository + Unit of Work**: All data access through `IUnitOfWork`
- **Soft Delete**: `IsDeleted` flag on all entities
- **Audit Trail**: `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`
- **API Response Wrapper**: `ApiResponse<T>` with `Success`, `Message`, `Data`, `Pagination`
- **Global Exception Handling**: Middleware catches all unhandled exceptions
- **Role-Based Authorization**: Admin, Manager, Customer roles

## License

MIT
