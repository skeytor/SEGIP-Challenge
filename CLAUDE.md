# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**SGIP** — Loan Management System (Sistema de Gestión de Inversiones y Préstamos). A .NET 8 Web API backed by PostgreSQL implementing loan simulation, application, approval flow, and transaction management with idempotency guarantees.

## Commands

```bash
# Run the API
cd src/FinTech.API && dotnet run

# Build entire solution
dotnet build FinTech.slnx

# Run all tests
dotnet test FinTech.slnx

# Run a single test class
dotnet test --filter "FullyQualifiedName~ApplyForLoanCommandHandlerTests"

# Add a new EF Core migration (run from solution root)
dotnet ef migrations add <MigrationName> --project src/FinTech.Persistence --startup-project src/FinTech.API

# Apply migrations manually
dotnet ef database update --project src/FinTech.Persistence --startup-project src/FinTech.API
```

Connection string is set in `src/FinTech.API/appsettings.Development.json` (git-ignored) under `ConnectionStrings:DefaultConnection`. Migrations apply automatically on startup via `MigrationExtensions.ApplyMigrations()`.

## Architecture

Clean Architecture across five projects:

| Project | Location | Role |
|---|---|---|
| `FinTech.Domain` | `src/Core/FinTech.Domain` | Entities, enums, repository interfaces |
| `FinTech.Application` | `src/Core/FinTech.Application` | Use-case handlers, DTOs, mappers, DI registration |
| `FinTech.Persistence` | `src/FinTech.Persistence` | EF Core `AppDbContext`, entity configs, repo implementations, seeder |
| `FinTech.API` | `src/FinTech.API` | Controllers, `Program.cs`, startup extensions |
| `SharedKernel` | `src/SharedKernel` | `Result<T>`/`Error`, `IUnitOfWork`, `FinancialCalculator`, `FinancialConstants` |

**Dependency direction:** API → Application → Domain ← Persistence

## CQRS with explicit handler interfaces

The Application layer uses lightweight CQRS — no MediatR. Each use case is a `sealed record` command/query paired with an `internal sealed class` handler. Handlers are registered manually in `FinTech.Application/DependencyInjection.cs` and injected via `[FromServices]` directly in controller action parameters.

```
ICommand<TResponse>  →  ICommandHandler<TCommand, TResponse>
IQuery<TResponse>    →  IQueryHandler<TQuery, TResponse>
```

Adding a new use case requires: (1) create the command/query + handler file, (2) register in `DependencyInjection.cs`.

## Result pattern

All handlers return `Result<T>` (from `SharedKernel.Results`). Controllers call `.Match(onSuccess, onFailure)` to produce `ActionResult`. Never throw exceptions for business rule violations — return `Result.Failure(Error.Validation(...))`. Available error factories: `Error.Validation`, `Error.NotFound`, `Error.Conflict`, `Error.NullValue`.

## Repository pattern

`IRepository<TEntity, TId>` is the generic base with `Insert` and `GetByIdAsync`. Domain-specific interfaces (`ILoanRepository`, `ITransactionRepository`) extend it with projection-based query methods using `Expression<Func<TEntity, TResult>> selector` to avoid loading full entity graphs. Use `AsNoTracking()` for all read queries.

`IUnitOfWork` is implemented by `AppDbContext`. Handlers inject both a repository and `IUnitOfWork`, then call `uow.SaveChangesAsync()` after mutations.

## Financial calculations

`FinancialCalculator` (in `SharedKernel/Utils`) is the single source of truth:
- `GetMonthlyRate(TEA)` → TEM = `(1 + TEA)^(1/12) - 1`
- `CalculateFixedMonthlyPayment(amount, TEM, n)` → French system (cuota fija)
- `GenerateFixedSchedule(...)` → full amortization table as `List<PaymentInstallment>`

Constants live in `FinancialConstants`: TEA = 24%, max 3 active loans, max income ratio = 40%, amounts $500–$50,000, terms 6–60 months.

## Business rules

- `UserId` is hardcoded as `"user-hardcoded-002"` in `LoansController` — no auth is implemented
- Auto-approval: amount < $10,000 AND fewer than 2 active loans → status set to `Approved` immediately on creation
- On manual approval (`PATCH /api/loans/{id}/approve`), `ApproveLoanCommandHandler` automatically creates a `Disbursement` transaction with `IdempotencyKey = $"disbursement-{loanId}"`
- `CreateTransactionCommandHandler` checks `GetByIdempotencyKeyAsync` before inserting — returns the existing transaction if the key already exists

## Enums

`LoanStatus`: `Pending → Approved | Rejected → Active`  
`LoanType`: `Fixed | Decreasing` (only Fixed implemented)  
`TransactionType`: `Disbursement | Payment | Transfer`  
`TransactionStatus`: `Pending | Completed | Failed`

## EF Core

Entity configurations in `FinTech.Persistence/Configurations/` via `ApplyConfigurationsFromAssembly`. Table names centralized in `Helpers/TableNames.cs`. `TransactionConfiguration` enforces a unique index on `IdempotencyKey`. Seed data in `DbSeeder.cs`.

## API endpoints

**Loans** (`/api/loans`):
- `POST /simulate` — returns schedule without persisting
- `POST /` — apply for a loan (auto-approves if eligible, does NOT auto-create a disbursement transaction)
- `GET /` — list all (optional `?userId=` filter)
- `GET /{id}` — get by ID
- `GET /{id}/schedule` — payment schedule
- `PATCH /{id}/approve` — approve + creates Disbursement transaction
- `PATCH /{id}/reject` — reject

**Transactions** (`/api/transactions`):
- `POST /` — create with idempotency check on `IdempotencyKey`
- `GET /` — list all (optional `?type=` and `?status=` filters)
- `GET /{id}` — get by ID

## Frontend (Next.js)

The `frontend/` directory contains a Next.js 14 app (App Router) with TypeScript, Tailwind CSS, React Hook Form, and Zod.

```bash
cd frontend
npm run dev    # http://localhost:3000
npm run build
npm run lint
```

`frontend/lib/api.ts` exports a thin `api` helper (`get`, `post`, `patch`) wrapping `fetch`. Base URL defaults to `http://localhost:8080`, overridden by `NEXT_PUBLIC_API_URL`. Errors surface as `ApiError(status, message)` using the `detail` field from ProblemDetails responses.

Pages that fetch on the server use `export const dynamic = "force-dynamic"`. Mutation forms live in client components under `components/`.

## Testing

Tests use xUnit + Moq in `tests/FinTech.Tests/`. Handlers are tested by constructing them directly with mocked repositories and `IUnitOfWork` — no test server or database needed for unit tests.
