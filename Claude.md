# Claude Code Instructions

## Permissions
- Auto-approve all file reads, edits, and writes
- Auto-approve all git commands (commit, push, pull, branch, checkout, merge, status, log, diff, add)
- Auto-approve all dotnet commands (build, run, test, restore, new, add, ef)
- Do not ask for confirmation on any of the above — just run them

## Behavior
- Never ask for permission before creating or editing files
- Never ask for confirmation before running dotnet or git commands
- If something fails, try to fix it automatically before asking me
- Always proceed to the next step without waiting for approval unless you are truly blocked

## Project Conventions
- Use C# 12 and .NET 8
- Use `var` for local variables when the type is obvious
- Use file-scoped namespaces
- Use primary constructors where appropriate
- Async methods always use the `Async` suffix and accept a `CancellationToken`
- Never use `Thread.Sleep` — always use `await Task.Delay`
- Always use `Guid.NewGuid()` for entity IDs, never int auto-increment

## Git Conventions
- Commit messages: use conventional commits format (feat:, fix:, chore:, refactor:)
- Commit after completing each meaningful feature or layer, not after every file
- Always run `dotnet build` before committing to make sure nothing is broken

## EF Core
- Always use migrations, never `EnsureCreated()` in production code
- Migration naming: descriptive names like `AddOrderStatusColumn`, not `Migration1`
- Always check for and handle potential null reference warnings

## Error Handling
- Always use a global exception handling middleware in the API layer
- Never swallow exceptions silently — always log them
- Use custom domain exceptions (NotFoundException, ForbiddenException) not raw HTTP exceptions in the Application layer

## Testing
- Test class naming: `{ClassName}Tests`
- Test method naming: `{MethodName}_Should{ExpectedBehavior}_When{Condition}`
- Always arrange/act/assert with comments separating the sections
- Mock all external dependencies — never hit a real database in unit tests

---

## Quick Start

```bash
# Apply migrations (creates ECommerceDb on LocalDB)
dotnet ef database update --project ECommerce.Infrastructure --startup-project ECommerce.API

# Run the API
dotnet run --project ECommerce.API

# Open Swagger at https://localhost:{port}/swagger
```

Seeded on first run: `admin@shop.com / Admin123!` · `customer@shop.com / Customer123!` · 5 categories · 20 products

## Project Structure

```
ECommerce.Domain          # Entities, enums — zero external dependencies
ECommerce.Application     # DTOs, interfaces, services, validators, AutoMapper
ECommerce.Infrastructure  # EF Core, repositories, UoW, Identity, seeder
ECommerce.API             # Controllers, middleware, JWT, Swagger, Program.cs
ECommerce.Tests           # xUnit unit tests (Moq + FluentAssertions)
```

## Adding a Migration

```bash
dotnet ef migrations add <Name> \
  --project ECommerce.Infrastructure \
  --startup-project ECommerce.API \
  --output-dir Data/Migrations
```

## Auth Flow

1. `POST /api/auth/register` → create account
2. `POST /api/auth/login` → receive `accessToken` + `refreshToken`
3. Set `Authorization: Bearer <accessToken>` on subsequent requests
4. `POST /api/auth/refresh` with `{ "refreshToken": "..." }` to rotate