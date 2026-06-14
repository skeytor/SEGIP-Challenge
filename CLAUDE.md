# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**SGIP** — Loan Management System (Sistema de Gestión de Inversiones y Préstamos). A .NET 8 Web API backed by PostgreSQL, implementing loan simulation, application, and transaction management with idempotency guarantees.

## Commands

```bash
# Run the API
cd src/FinTech.API && dotnet run

# Build entire solution
dotnet build FinTech.slnx

# Run tests
dotnet test FinTech.slnx

# Add a new EF Core migration (run from solution root)
dotnet ef migrations add <MigrationName> --project src/FinTech.Persistence --startup-project src/FinTech.API

# Apply migrations manually
dotnet ef database update --project src/FinTech.Persistence --startup-project src/FinTech.API
```

### Connection string

Set in `src/FinTech.API/appsettings.Development.json` (git-ignored) under `ConnectionStrings:DefaultConnection`. Migrations are applied automatically on startup via `MigrationExtensions.ApplyMigrations()`.

## Architecture

The solution uses **Clean Architecture** split into four projects:

| Project | Role |
|---|---|
| `FinTech.Domain` | Entities, enums, repository interfaces, `FinancialCalculator` |
| `FinTech.Application` | Use-case services (`LoanService`), DTOs, mappers, DI registration |
| `FinTech.Persistence` | EF Core `AppDbContext`, entity configurations, repository implementations |
| `FinTech.API` | Controllers, `Program.cs`, startup extensions |
| `SharedKernel` | `Result<T>`/`Error` types, `IUnitOfWork`, `FinancialCalculator` is also here |

**Dependency direction:** API → Application → Domain ← Persistence (Persistence depends on Domain interfaces).

### Result pattern

All service methods return `Result<T>` (from `SharedKernel.Results`). Controllers call `.Match(onSuccess, onFailure)` to map to `ActionResult`. Never throw exceptions for business rule violations — return `Result.Failure(Error.Validation(...))`.

### Repository pattern

`IRepository<TEntity, TId>` is the generic base. `ILoanRepository` extends it with projection-based query methods that accept `Expression<Func<Loan, TResult>> selector` to avoid loading full entity graphs unnecessarily. Use `AsNoTracking()` for read-only queries.

`IUnitOfWork` is implemented by `AppDbContext`. Services inject both `ILoanRepository` and `IUnitOfWork`, then call `uow.SaveChangesAsync()` after mutations.

### Financial calculations

`FinancialCalculator` (in `FinTech.Domain/Utils`) is the single source of truth for:
- `GetMonthlyRate(TEA)` → TEM using `(1 + TEA)^(1/12) - 1`
- `CalculateFixedMonthlyPayment(amount, TEM, n)` → French system (cuota fija)
- `GenerateFixedSchedule(...)` → full amortization table as `List<PaymentInstallment>`

### Business rules hardcoded in `LoanService`

- `CurrentUserId = "user-hardcoded-001"` — no authentication is implemented
- TEA fixed at **24%**
- Max **3 active loans** per user
- Monthly payments must not exceed **40% of declared monthly income**
- Auto-approval: amount < $10 000 AND fewer than 2 active loans → status set to `Approved` immediately on creation

### Enums

`LoanStatus`: `Pending → Approved | Rejected → Active`  
`LoanType`: `Fixed | Decreasing` (only Fixed is currently implemented)  
`TransactionType`: `Disbursement | Payment | Transfer`  
`TransactionStatus`: `Pending | Completed | Failed`

### EF Core configuration

Entity configurations live in `FinTech.Persistence/Configurations/` and are picked up via `ApplyConfigurationsFromAssembly`. Table names are centralized in `Helpers/TableNames.cs`. `TransactionConfiguration` sets a **unique index on `IdempotencyKey`** — enforce this at the DB level, not only in application code.

## Frontend (Next.js)

The `frontend/` directory contains a Next.js 14 app (App Router) built with TypeScript, Tailwind CSS, React Hook Form, and Zod.

### Commands

```bash
cd frontend

# Dev server (http://localhost:3000)
npm run dev

# Build
npm run build

# Lint
npm run lint
```

### API connection

`frontend/lib/api.ts` exports a thin `api` helper (`get`, `post`, `patch`) that wraps `fetch`. The base URL defaults to `http://localhost:8080` and is overridden by `NEXT_PUBLIC_API_URL`. Errors are surfaced as `ApiError(status, message)` using the `detail` field from ProblemDetails responses.

### Structure

| Path | Role |
|---|---|
| `app/` | Next.js App Router pages (`/`, `/loans`, `/loans/[id]`, `/loans/simulate`, `/transactions`) |
| `components/` | Client components: `LoanList`, `LoanSimulator`, `PaymentScheduleTable`, `TransactionList`, `TransactionFilters` |
| `services/` | `loan-service.ts`, `transaction-service.ts` — thin wrappers over `api` that map to backend endpoints |
| `types/` | `loan.ts`, `transaction.ts` — TypeScript interfaces matching backend DTOs |
| `lib/api.ts` | Shared fetch wrapper |

Pages that fetch on the server use `export const dynamic = "force-dynamic"` to avoid caching stale loan/transaction data. All mutation forms live in client components under `components/`.

## Pending / Not yet implemented

- `Transaction` endpoints (POST /api/transactions, GET /api/transactions, GET /api/transactions/{id})
- Loan schedule endpoint (GET /api/loans/{id}/schedule)
- Approve/reject endpoints (PATCH /api/loans/{id}/approve, PATCH /api/loans/{id}/reject)
- Idempotency check logic in transaction processing
