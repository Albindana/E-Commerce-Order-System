# E-Commerce Order System

A production-structured N-tier monolith built with ASP.NET Core 8. Covers Auth, Products, Cart, and Orders with JWT authentication, EF Core 8, and a clean separation between layers.

---

## Architecture

```
┌──────────────────────────────────────────────────────────┐
│                    HTTP Clients / Swagger                │
└───────────────────────────┬──────────────────────────────┘
                            │
┌───────────────────────────▼──────────────────────────────┐
│                     ECommerce.API                        │
│  Controllers · JWT Bearer · FluentValidation             │
│  GlobalExceptionMiddleware · Swagger/OpenAPI             │
└───────────────────────────┬──────────────────────────────┘
                            │  interfaces only
┌───────────────────────────▼──────────────────────────────┐
│                 ECommerce.Application                    │
│  Services · DTOs · Mapperly mapper · Validators          │
│  IUnitOfWork · IRepository interfaces · Exceptions       │
└───────────────────────────┬──────────────────────────────┘
                            │  implements
┌───────────────────────────▼──────────────────────────────┐
│                ECommerce.Infrastructure                  │
│  AppDbContext · Repositories · UnitOfWork                │
│  ASP.NET Core Identity · DbSeeder · EF Migrations        │
│  SQLite (dev, auto-detected) · SQL Server (prod)         │
└───────────────────────────┬──────────────────────────────┘
                            │  entities & enums
┌───────────────────────────▼──────────────────────────────┐
│                   ECommerce.Domain                       │
│  Entities · Enums · BaseEntity                           │
│  ── zero external NuGet dependencies ──                  │
└──────────────────────────────────────────────────────────┘
```

**Dependency rule:** arrows flow downward only. Application never imports Infrastructure. Domain imports nothing outside the BCL.

---

## Project Structure

| Project | Responsibility |
|---|---|
| `ECommerce.Domain` | Entities, enums, `BaseEntity`. No NuGet dependencies. |
| `ECommerce.Application` | Service interfaces & implementations, DTOs, Mapperly mapper, FluentValidation validators, `IUnitOfWork` / repository interfaces, domain exceptions. |
| `ECommerce.Infrastructure` | EF Core `AppDbContext`, entity configurations, repositories, `UnitOfWork`, Identity, `DbSeeder`, migrations. |
| `ECommerce.API` | Controllers, JWT Bearer config, FluentValidation pipeline, global exception middleware, Swagger, `Program.cs`. |
| `ECommerce.Tests` | xUnit unit tests (Moq) for all services + `WebApplicationFactory` integration tests with isolated SQLite. |

---

## Quick Start

**Prerequisites:** .NET 8 SDK and the `dotnet-ef` global tool.

```bash
dotnet tool install -g dotnet-ef
```

### 1. Clone

```bash
git clone https://github.com/Albindana/E-Commerce-Order-System.git
cd E-Commerce-Order-System
```

### 2. Connection string

Development uses SQLite by default — no changes needed. The provider is auto-detected from the connection string prefix.

```json
// ECommerce.API/appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=ecommerce.db"
  }
}
```

To use SQL Server, replace the value with a standard `Server=...` connection string.

### 3. Apply migrations

```bash
dotnet ef database update \
  --project ECommerce.Infrastructure \
  --startup-project ECommerce.API
```

### 4. Run

```bash
dotnet run --project ECommerce.API
# Swagger UI: https://localhost:{port}/swagger
```

Seed data is applied automatically on first startup:

| Account | Password | Role |
|---|---|---|
| admin@shop.com | Admin123! | Admin |
| customer@shop.com | Customer123! | Customer |

Plus 5 categories and 20 products.

---

## Auth Flow

```
POST /api/auth/register
POST /api/auth/login         →  { accessToken, refreshToken }
Authorization: Bearer <token>
POST /api/auth/refresh        →  rotate refresh token
```

Access tokens expire in 60 minutes, refresh tokens in 7 days. The `Admin` role gates product write endpoints and the admin order list.

---

## Adding a Migration

```bash
dotnet ef migrations add <DescriptiveName> \
  --project ECommerce.Infrastructure \
  --startup-project ECommerce.API \
  --output-dir Data/Migrations
```

Use descriptive names (`AddOrderStatusColumn`, not `Migration1`).

---

## Design Decisions

**N-tier architecture.** Separating Domain, Application, Infrastructure, and API into distinct projects enforces boundaries at compile time, not just convention. The Application layer holds no knowledge of HTTP or EF Core — it works against interfaces only, making it unit-testable without spinning up a web server or database. The trade-off is more ceremony for simple CRUD; the payoff is that the seams are load-bearing: swapping SQLite for SQL Server, or replacing the token service, requires changes in exactly one layer.

**Repository + Unit of Work.** Both patterns are sometimes dismissed as unnecessary when using EF Core, since `DbContext` already implements them internally. The case for keeping them here is testability: mocking `IUnitOfWork` and individual repository interfaces is trivial with Moq, while mocking `DbContext` is not. Unit tests for all five services run in milliseconds with no database. The Unit of Work also gives a single `SaveChangesAsync()` call site, making cross-aggregate consistency explicit.

**Mapperly instead of AutoMapper.** AutoMapper versions 12 and 13 carry CVE GHSA-rvv3-g6hj-g44x, a medium-severity vulnerability with no patched release available at time of writing. The project uses Riok.Mapperly 3.6.0, a Roslyn source generator that emits plain C# mapping methods at compile time. There is no runtime reflection, no configuration object to bootstrap, and no known CVEs. The only cost was rewriting the mapping profile as a `[Mapper]` partial class.

---

## Tests

33 integration tests using `WebApplicationFactory` with isolated SQLite — each test class gets its own fresh database, seeded via `CreateHost` override.

```bash
dotnet test                          # all tests
dotnet test --filter "Services"      # unit tests only
dotnet test --filter "Integration"   # integration tests only
```

Coverage spans the full HTTP stack: auth (register, login, token validation), products (list, get, create with admin role), cart (add item, clear), and orders (checkout, empty cart rejection, order listing).
