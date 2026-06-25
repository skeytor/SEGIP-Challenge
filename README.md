# SGIP — Loan Management System

SGIP is a fullstack web application for managing financial loans. It allows users to simulate loans, view the full payment schedule before committing, apply for loans that are automatically approved based on business rules, and record transactions with idempotency guarantees to prevent duplicate charges.

---

## Deployment Links

| Service | URL |
|---|---|
| Frontend | https://segip-challenge.vercel.app/ |
| Backend (Swagger) | https://segip-challenge-production.up.railway.app/swagger/index.html |

**Test credentials:** No authentication required. All operations use a fixed `userId` (`user-hardcoded-002`) representing the active user.

The database comes with two pre-loaded loans:
- **$5,000** over 12 months — status `Active`
- **$15,000** over 24 months — status `Pending`

---

## Technologies

### Backend
- **.NET 8** — main API framework
- **PostgreSQL** — relational database
- **Entity Framework Core** — ORM with versioned migrations
- **Swashbuckle (Swagger/OpenAPI)** — API documentation

### Frontend
- **Next.js 14** (App Router) — React framework with server components support
- **TypeScript** — static typing
- **Tailwind CSS** — utility-first styling
- **React Hook Form + Zod** — form handling and validation

### Testing
- **xUnit** — unit testing framework
- **Moq** — dependency mocking

### Key Technical Decisions

**Clean Architecture with explicit CQRS:** The Command/Query pattern is implemented directly via interfaces (`ICommandHandler<TCommand, TResponse>`) without MediatR. Each use case is a single file with its command and handler, and DI registration is explicit and easy to follow. The trade-off is more manual wiring, but at this scale that's preferable to adding an extra dependency.

**Result pattern instead of exceptions:** Handlers never throw exceptions for business rule violations. Instead they return `Result<T>` with a typed `Error`. Controllers use `.Match(onSuccess, onFailure)` to convert that into an `ActionResult`. This makes the error flow explicit, predictable, and easy to test without `try/catch`. Throwing exceptions for validations can also hurt performance.

**Repository projections:** Query methods in `ILoanRepository` and `ITransactionRepository` accept an `Expression<Func<TEntity, TResult>> selector`. This avoids loading full entity graphs when only a few fields are needed, without duplicating filtering logic outside the repository.

---

## Local Setup

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Running the project

A single command starts all three services together — frontend, API, and database:

```bash
docker-compose up --build
```

| Service | URL |
|---|---|
| Frontend (Next.js) | `http://localhost:3000` |
| Backend (API) | `http://localhost:8080` |
| Swagger | `http://localhost:8080/swagger` |
| PostgreSQL | `localhost:5432` |

Docker handles building the images, starting PostgreSQL, applying migrations, and loading the initial seed data. No need to have .NET or Node.js installed locally.

To stop:

```bash
docker-compose down
```

---

## Environment Variables

### Backend

| Variable | Description | Example |
|---|---|---|
| `DATABASE_URL` | PostgreSQL URL connection string (used in production/Railway) | `postgresql://user:pass@host:5432/db` |
| `ConnectionStrings__DefaultConnection` | Npgsql format alternative (used in local development without Docker) | `Host=localhost;Database=sgip;...` |

When running with Docker locally, no configuration is needed — variables are already defined in `docker-compose.yml`.

### Frontend

Create a `.env.local` file inside the `frontend/` folder:

```env
NEXT_PUBLIC_API_URL=http://localhost:8080
```

---

## Testing

```bash
# From the repository root
dotnet test FinTech.slnx

# Run a single test class
dotnet test --filter "FullyQualifiedName~ApplyForLoanCommandHandlerTests"
```

Tests are unit tests and require no database. Each handler is tested directly by instantiating it with mocked repositories. Covered cases include:

- Correct fixed installment calculation (French system)
- Auto-approval when amount is < $10,000 and fewer than 2 active loans exist
- Rejection when the active loan limit is exceeded
- Rejection when the installment exceeds 40% of declared monthly income
- Minimum and maximum amount validations

---

## Architecture

The project follows **Clean Architecture** split into five layers with unidirectional dependencies:

```
FinTech.API  →  FinTech.Application  →  FinTech.Domain  ←  FinTech.Persistence
                                              ↑
                                        SharedKernel
```

- **FinTech.Domain** (`src/Core/FinTech.Domain`): entities (`Loan`, `Transaction`, `PaymentSchedule`), enums, and repository contracts.
- **FinTech.Application** (`src/Core/FinTech.Application`): use cases implemented as CQRS handlers, DTOs, and mappers.
- **FinTech.Persistence** (`src/FinTech.Persistence`): EF Core repository implementations, entity configurations, and seed data.
- **FinTech.API** (`src/FinTech.API`): REST controllers, startup configuration, and extensions.
- **SharedKernel** (`src/SharedKernel`): shared types across layers — `Result<T>`, `Error`, `IUnitOfWork`, `FinancialCalculator`, and `FinancialConstants`.

### Implemented Patterns

**Repository Pattern** — each entity has its own repository interface in the domain (`ILoanRepository`, `ITransactionRepository`) with its implementation in Persistence. This allows testing handlers without a real database.

**Command/Query Pattern** — write operations are `ICommand` and read operations are `IQuery`, each with its corresponding handler.

**Result Pattern** — replaces exceptions for business errors, making the error flow explicit and predictable.

---

## Design Decisions

**Why React Hook Form + Zod instead of Formik?**
React Hook Form performs better because it doesn't re-render the entire form on every keystroke — it only updates the changed field. Zod allows defining the validation schema once and reusing it on both client and server.

**Why Next.js App Router with `force-dynamic`?**
Listing pages (`/loans`, `/transactions`) use `force-dynamic` to prevent Next.js from caching data at build time. Since content changes frequently, static caching would cause users to see stale data.

**What was simplified?**
- The annual interest rate (TEA) is fixed at 24% for all loans. In production this would vary based on the client's risk profile.
- The `userId` is hardcoded. Authentication was left out because it wasn't required and implementing it would have taken time away from the business logic.
- Only the fixed installment system (French) was implemented. The decreasing system (German) remains as a future improvement.

---

## Assumptions and Limitations

### Not implemented
- User authentication and authorization
- Decreasing installment system (German system)
- CI/CD pipeline

### Simplifications made
- Single hardcoded user (`user-hardcoded-002`) instead of a session system
- Fixed TEA at 24% with no variation by risk profile
- The `Active` status is not automatically assigned on approval — the loan stays in `Approved` until a full disbursement flow is implemented

### Future improvements
- JWT authentication
- Installment due date notifications
- Portfolio metrics dashboard (total amount lent, delinquency rate, etc.)
- Decreasing installment system (German system)
- Integration tests with a real database using Testcontainers
