# FinReg Audit Platform — Development Guide

## Session Start Ritual

**Read DEVELOPMENT.md → HANDOFF.md before anything else.** Never begin a new session by writing code. Understand the current phase, the last known state, and any outstanding decisions documented in HANDOFF.md first.

---

## Solution Structure

The solution is organised as a Clean Architecture monorepo with four .NET 8 class library / web API projects and a React frontend. Each layer has a strict dependency rule: inner layers have no knowledge of outer layers.

```
finreg-audit-platform/
├── DEVELOPMENT.md
├── HANDOFF.md
├── CONTEXT.md
├── README.md
├── docker-compose.yml
├── .env.example
├── .gitignore
├── Dockerfile
├── fly.toml
├── src/
│   ├── FinReg.Domain/
│   ├── FinReg.Application/
│   ├── FinReg.Infrastructure/
│   └── FinReg.API/
├── tests/
│   ├── FinReg.Domain.Tests/
│   ├── FinReg.Application.Tests/
│   └── FinReg.Integration.Tests/
└── frontend/
```

### FinReg.Domain

The innermost layer. Contains enterprise business rules that would exist regardless of technology. Has **zero NuGet package dependencies** — no EF Core, no MediatR, no third-party libraries. Anything in this project is plain C# records, classes, and interfaces.

- `Entities/` — `Account` (aggregate root), `Transaction`, `AuditEvent`
- `Events/` — all domain events implementing `IDomainEvent`
- `Enums/` — `AccountStatus`, `TransactionStatus`, `RiskLevel`, `AlertSeverity`
- `ValueObjects/` — `Money` (immutable record with Amount + Currency), `AccountNumber` (validated format)
- `Exceptions/` — domain-specific exceptions thrown when business rules are violated

### FinReg.Application

Use cases and application logic. Depends only on Domain. Has **no EF Core dependency**. Uses interfaces (defined here, implemented in Infrastructure) to communicate with the outside world.

- `Common/Interfaces/` — `IEventStore`, `IAccountRepository`, `ITransactionRepository`, `IJwtService`, `ICurrentUser`
- `Common/Behaviours/` — MediatR pipeline: logging, validation, performance monitoring
- `Common/Models/` — `ApiResponse<T>`, `PaginatedList<T>`, `PaginationParams`
- `Accounts/Commands/` and `Accounts/Queries/` — use case handlers for account operations
- `Transactions/Commands/` and `Transactions/Queries/` — transaction lifecycle
- `Alerts/Queries/` — suspicious activity alert retrieval
- `Reports/Queries/` — compliance report generation
- `Auth/Commands/` — authentication and token management

### FinReg.Infrastructure

Implements the interfaces defined in Application. Contains all technology-specific code: EF Core, PostgreSQL, JWT generation, Serilog configuration, risk assessment service.

- `Persistence/AppDbContext.cs` — EF Core context
- `Persistence/EventStore/` — append-only event store repository and JSON event serialiser
- `Persistence/Repositories/` — account and transaction repositories
- `Identity/JwtService.cs` — JWT access token + refresh token implementation
- `Services/RiskAssessmentService.cs` — FCA threshold logic

### FinReg.API

The entry point. ASP.NET Core 8 Web API. Depends on all other projects. Contains controllers, middleware, Swagger configuration, and `Program.cs`. **No business logic here** — controllers only receive HTTP requests, dispatch to MediatR, and return responses.

---

## .NET CLI Commands

### Building the solution

```bash
# Restore all dependencies
dotnet restore

# Build entire solution
dotnet build

# Build in Release mode
dotnet build -c Release
```

### Running the API

```bash
# Development (reads appsettings.Development.json)
dotnet run --project src/FinReg.API

# With explicit environment
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/FinReg.API
```

The API starts at `http://localhost:5000`. Swagger UI is available at `http://localhost:5000/swagger`.

### Running tests

```bash
# All tests
dotnet test

# Specific project
dotnet test tests/FinReg.Domain.Tests
dotnet test tests/FinReg.Application.Tests
dotnet test tests/FinReg.Integration.Tests

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Verbose output
dotnet test --verbosity normal
```

### Entity Framework Core migrations

```bash
# Add a new migration (run from solution root)
dotnet ef migrations add <MigrationName> --project src/FinReg.Infrastructure --startup-project src/FinReg.API

# Apply migrations to the database
dotnet ef database update --project src/FinReg.Infrastructure --startup-project src/FinReg.API

# Generate SQL script (for production review)
dotnet ef migrations script --project src/FinReg.Infrastructure --startup-project src/FinReg.API

# Remove last migration (only if not applied to DB)
dotnet ef migrations remove --project src/FinReg.Infrastructure --startup-project src/FinReg.API
```

---

## Docker Commands

```bash
# Start PostgreSQL 16 in background
docker compose up -d

# Stop and remove containers (preserves volumes)
docker compose down

# Stop and remove containers AND volumes (wipes database)
docker compose down -v

# View logs
docker compose logs -f

# Check running containers
docker compose ps
```

The docker-compose.yml starts a PostgreSQL 16 container on port 5432. The database name, user, and password are configured in `.env` (see `.env.example`).

---

## Frontend Commands

All frontend commands use `pnpm`. Never use npm or yarn in this project.

```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
pnpm install

# Start development server (http://localhost:5173)
pnpm dev

# Type-check
pnpm tsc --noEmit

# Build for production
pnpm build

# Preview production build locally
pnpm preview

# Run tests
pnpm test

# Run tests with UI
pnpm test:ui
```

---

## Event Sourcing — How It Works

Event Sourcing is the most technically important pattern in this project. Rather than storing the current state of an entity (like a row in a database that gets updated), the system stores every state change as an immutable event. The current state of any entity is always derived by replaying its event history from the beginning.

### How events flow through the system

1. A command arrives at an API endpoint (for example, `InitiateTransactionCommand`).
2. The MediatR handler loads the relevant aggregate (e.g., `Account`) by replaying its events from the event store.
3. The handler validates the command against the current aggregate state.
4. The handler applies business logic and produces one or more new domain events (e.g., `TransactionInitiatedEvent`, possibly `TransactionFlaggedEvent`).
5. The new events are appended to the event store via `IEventStore.AppendEventsAsync()`. This is the only write operation.
6. The aggregate's in-memory state is updated by calling `Account.Apply(event)` for each new event.
7. The handler returns a success result.

### How to add a new event type

1. Create a new record in `FinReg.Domain/Events/` implementing `IDomainEvent`.
2. Add a corresponding `Apply(YourNewEvent)` overload to the `Account` aggregate root (or whichever aggregate it belongs to).
3. Add unit tests in `FinReg.Domain.Tests` verifying the state change.
4. Add handling in the relevant Application command handler.
5. Register the event type in `EventSerializer.cs` if using a type discriminator.

### Snapshots

For accounts with more than 100 events, replaying the full history on every read becomes expensive. Snapshots solve this: after event 100 (and every 100 events thereafter), the current account state is serialised to JSON and saved to the `account_snapshots` table. On subsequent reads, the system loads the latest snapshot and replays only the events that occurred after it.

### The immutability guarantee

The `audit_events` table in PostgreSQL is append-only by design. The `EventStoreRepository` never issues UPDATE or DELETE statements against it. This is enforced at the repository level. No service, no migration, and no maintenance script should ever modify or delete event records.

---

## CQRS — How to Add a New Command or Query

### Adding a new Command

1. Create a folder: `src/FinReg.Application/<Domain>/Commands/<CommandName>/`
2. Create `<CommandName>Command.cs` — a record implementing `IRequest<ApiResponse<YourReturnType>>`
3. Create `<CommandName>CommandHandler.cs` — a class implementing `IRequestHandler<...>`. Inject only interfaces from `Common/Interfaces/`.
4. Create `<CommandName>CommandValidator.cs` — a FluentValidation `AbstractValidator<YourCommand>`. Rules here are enforced automatically by the `ValidationBehavior` pipeline.
5. Add a unit test in `FinReg.Application.Tests` mocking the interfaces.
6. Add a controller action in the appropriate controller dispatching via `_mediator.Send(command)`.

### Adding a new Query

Same structure as a command, but in a `Queries/<QueryName>/` folder. Queries are read-only — they must not append events or mutate state.

---

## Required Environment Variables

| Variable | Description | Example |
|---|---|---|
| `DATABASE_URL` | PostgreSQL connection string (transaction pooler format for Supabase) | `Host=...;Port=6543;Database=postgres;Username=postgres;Password=...;Pooling=true;Minimum Pool Size=1;Maximum Pool Size=20` |
| `JWT_SECRET` | HMAC-SHA256 signing key, minimum 32 characters | `your-super-secret-key-min-32-chars` |
| `JWT_ISSUER` | JWT issuer claim | `finreg-audit-platform` |
| `JWT_AUDIENCE` | JWT audience claim | `finreg-audit-api` |
| `VITE_API_BASE_URL` | Backend API URL used by the Vite frontend build | `https://your-app.fly.dev` |

Copy `.env.example` to `.env` for local development. Never commit `.env`.

---

## Deployment

### Fly.io (Backend)

```bash
# First-time setup
fly launch

# Set production secrets (never commit these)
fly secrets set DATABASE_URL="Host=...;Port=6543;..."
fly secrets set JWT_SECRET="your-production-secret"
fly secrets set JWT_ISSUER="finreg-audit-platform"
fly secrets set JWT_AUDIENCE="finreg-audit-api"

# Deploy
fly deploy

# Check deployment status
fly status

# View logs
fly logs

# Scale (keep at least 1 machine to avoid cold starts)
fly scale count 1
```

The `fly.toml` is configured with `min_machines_running = 1` and a health check on `GET /health`.

### Supabase (Production Database)

Always use the **transaction pooler URL** at port **6543**, not the direct connection URL. The pooler is required when using EF Core with Supabase because serverless environments open many short-lived connections that would exhaust PostgreSQL's connection limit.

Connection string format:
```
Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.yourref;Password=yourpassword;Pooling=true;Minimum Pool Size=1;Maximum Pool Size=20
```

Run migrations against the production database before deploying:
```bash
DATABASE_URL="your-supabase-pooler-url" dotnet ef database update --project src/FinReg.Infrastructure --startup-project src/FinReg.API
```

### Vercel (Frontend)

- Set **Root Directory** to `frontend` in Vercel project settings.
- Set environment variable `VITE_API_BASE_URL` to the Fly.io backend URL.
- Vercel auto-detects Vite and builds correctly.

---

## Code Conventions

### Naming

- Classes, records, interfaces, enums: `PascalCase`
- Private fields: `_camelCase` (underscore prefix)
- Local variables and parameters: `camelCase`
- Constants: `PascalCase`
- Files: one public type per file, file name matches type name

### Async/Await

Every method that touches I/O (database, HTTP, file system) must be `async` and return `Task` or `Task<T>`. Every async method accepts a `CancellationToken ct` as its final parameter and passes it through to all downstream async calls.

### General

- No business logic in controllers — they only call `_mediator.Send()`
- No direct `DbContext` usage outside the Infrastructure project
- All domain exceptions inherit from a base `DomainException`
- Use `record` types for immutable value objects and domain events
- Prefer `sealed` on classes that should not be inherited

---

## What NOT To Do

- **Never** run UPDATE or DELETE queries against the `audit_events` table. This would violate the immutability guarantee of the event store.
- **Never** put business logic in controllers. Controllers are HTTP adapters only.
- **Never** use `DbContext` directly in Application or Domain layers.
- **Never** add EF Core references to `FinReg.Domain` or `FinReg.Application` projects.
- **Never** commit `.env` files — only `.env.example` with placeholder values.
- **Never** expose stack traces in production error responses — use correlation IDs instead.
- **Never** store JWT access tokens in localStorage — use in-memory state (Zustand) for the frontend.
- **Never** advance phases without explicit approval.
- **Never** create a file named `CLAUDE.md` in this repository.
- **Never** add AI attribution to any commit, comment, or file.

---

## Active Skills

| Skill | Description |
|---|---|
| **caveman** | Terse communication style during active development phases — no filler, fragments OK, technical accuracy 100%. |
| **grill-me** | Before each phase, self-interview on key architectural decisions, resolve dependencies, state a recommendation per decision. |
| **frontend-design** | Phase 5 only — enterprise compliance terminal aesthetic. Deep charcoal / crisp white, electric blue accent, JetBrains Mono for data values, Lucide React icons only. |
| **ui-ux-pro-max** | Phase 5 only — accessibility, interaction quality, theming, table standards, and pre-delivery checklist gates. |
| **diagnose** | Any phase — six-step debugging protocol. Never skip building a feedback loop first. |

---

## Project Phases

| Phase | Name | Status |
|---|---|---|
| 0 | Documentation & Setup | Complete |
| 1 | Domain Layer & Event Sourcing Core | Complete |
| 2 | Application Layer (CQRS + MediatR) | Not Started |
| 3 | Infrastructure Layer (EF Core + Event Store + JWT) | Not Started |
| 4 | API Layer, Swagger & Deployment Config | Not Started |
| 5 | React Frontend & Final Polish | Not Started |
| 6 | Testing, README & Deployment | Not Started |
