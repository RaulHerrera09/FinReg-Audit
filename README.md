# FinReg Audit 

A production-grade financial compliance system implementing Event Sourcing, CQRS, and Clean Architecture — the same patterns used at Monzo, Revolut, and Standard Life to satisfy FCA audit trail requirements.

---

**Live demo:**
| Service | URL |
|---|---|
| Swagger UI | https://finreg-audit.fly.dev/swagger |
| Frontend | https://fin-reg-audit.vercel.app |
| Health check | https://finreg-audit.fly.dev/health |

**Demo credentials:** `compliance@demo.finreg.dev` / `Compliance@123`

---

## What this demonstrates

Every account state change — an account opening, a transaction, a fraud flag — is stored as an immutable event. Nothing is ever updated or deleted. Any account's complete history can be reproduced by replaying its events, exactly as required under FCA audit rules.

The system automatically detects four simplified but realistic AML patterns:

| Trigger | Rule |
|---|---|
| Large transaction | Single transaction ≥ £10,000 |
| Structuring | Cumulative transactions > £25,000 within 24 hours |
| Fraud attempt | 5+ failed transactions within 1 hour |
| Dormant account | 12+ months inactive, then sudden large activity |

Recruiter flow in under 2 minutes: log in with demo credentials → open an account → initiate a £15,000 credit → watch the `TransactionFlaggedEvent` and `SuspiciousActivityDetectedEvent` appear in the audit trail automatically.

---

## Architecture

```
┌───────────────────────────────────────────────────────────────┐
│                        React 18 Frontend                       │
│          (Vite · TanStack Query · Zustand · Tailwind 4)       │
└─────────────────────────────┬─────────────────────────────────┘
                              │ REST / JSON
┌─────────────────────────────▼─────────────────────────────────┐
│                      FinReg.API (ASP.NET Core 8)               │
│          Controllers · JWT Auth · Swagger · MediatR Dispatch   │
└──────┬──────────────────────────────────────────┬─────────────┘
       │ MediatR Commands/Queries                  │
┌──────▼──────────────────────┐    ┌──────────────▼─────────────┐
│  FinReg.Application          │    │   FinReg.Infrastructure     │
│  Use cases · Validators      │    │   EF Core · Npgsql          │
│  Pipeline behaviours         │    │   EventStoreRepository      │
│  (Logging · Validation       │◄───│   Repositories · JwtService │
│   · Performance)             │    │   RiskAssessmentService     │
└──────┬──────────────────────┘    └──────────────┬─────────────┘
       │ Entities · Events                         │
┌──────▼──────────────────────┐                   │
│  FinReg.Domain               │    ┌──────────────▼─────────────┐
│  Account aggregate root      │    │  PostgreSQL 16              │
│  Domain events · Value       │    │  audit_events (append-only) │
│  objects · FCA rules         │    │  account_snapshots (perf)   │
└─────────────────────────────┘    │  accounts · transactions     │
                                   │  alerts · users              │
                                   └─────────────────────────────┘
```

### Key architectural decisions

**Event Sourcing over CRUD** — The `audit_events` table is strictly append-only. The aggregate root's current state is always derived by replaying events. Snapshots kick in after 100 events to avoid full replay on every read.

**CQRS** — Commands mutate state through the domain model and append events. Queries read from denormalised read models (populated synchronously during event processing) and never touch the event store.

**Clean Architecture** — Zero EF Core references in Domain or Application. Infrastructure implements Application interfaces. Controllers contain no business logic.

---

## Tech stack

| Layer | Technology |
|---|---|
| Frontend | React 18, TypeScript, Vite, Tailwind CSS 4, TanStack Query, TanStack Table, Zustand, React Router 7, Lucide React |
| API | ASP.NET Core 8, MediatR, FluentValidation, Swagger/Swashbuckle |
| Application | CQRS handlers, MediatR pipeline behaviours (logging, validation, performance) |
| Infrastructure | Entity Framework Core 8, Npgsql, BCrypt.Net, System.IdentityModel.Tokens.Jwt |
| Database | PostgreSQL 16 |
| Testing | xUnit, NSubstitute, FluentAssertions, TestContainers (PostgreSQL) |
| Deployment | Fly.io (API), Supabase (DB), Vercel (frontend) |

---

## Local development

### Prerequisites

- .NET 8 SDK
- Node.js 20+ and pnpm
- Docker (for PostgreSQL)

### Backend

```bash
# Start PostgreSQL
docker compose up -d

# Run API (http://localhost:5000 · Swagger at /swagger)
dotnet run --project src/FinReg.API
```

Demo users are seeded automatically on first startup in Development.

### Frontend

```bash
cd frontend
pnpm install
pnpm dev    # http://localhost:5173
```

---

## Tests

```bash
# Unit tests (Domain + Application) — no external dependencies
dotnet test tests/FinReg.Domain.Tests
dotnet test tests/FinReg.Application.Tests

# Integration tests — requires Docker (TestContainers spins up PostgreSQL automatically)
dotnet test tests/FinReg.Integration.Tests

# All tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

Integration tests spin up a real PostgreSQL container per test class via TestContainers, apply the EF Core schema, seed a test user, and exercise the full HTTP stack from controller to database.

---

## Deployment

### 1 — Supabase (PostgreSQL)

Create a project at [supabase.com](https://supabase.com). Use the **transaction pooler URL** at port **6543** (required for EF Core with serverless connections):

```
Host=aws-0-eu-west-2.pooler.supabase.com;Port=6543;Database=postgres;
Username=postgres.yourref;Password=yourpassword;
Pooling=true;Minimum Pool Size=1;Maximum Pool Size=20
```

### 2 — Fly.io (API)

```bash
fly launch                              # first time only
fly secrets set DATABASE_URL="<supabase-pooler-url>"
fly secrets set JWT_SECRET="<min-32-char-secret>"
fly secrets set JWT_ISSUER="finreg-audit-platform"
fly secrets set JWT_AUDIENCE="finreg-audit-api"
fly secrets set ASPNETCORE_ENVIRONMENT="Production"
fly deploy
```

The API runs at `https://finreg-audit.fly.dev`. Swagger UI is disabled in Production — add `ASPNETCORE_ENVIRONMENT=Staging` to keep it enabled without demo user seeding.

### 3 — EF Core migrations

```bash
DATABASE_URL="<supabase-pooler-url>" \
  dotnet ef database update \
  --project src/FinReg.Infrastructure \
  --startup-project src/FinReg.API
```

### 4 — Vercel (frontend)

- Set Root Directory to `frontend` in Vercel project settings
- Set environment variable `VITE_API_BASE_URL=https://finreg-audit.fly.dev`
- Vercel auto-detects Vite

---

## Project structure

```
finreg-audit-platform/
├── src/
│   ├── FinReg.Domain/          # Entities, events, value objects, enums (zero NuGet deps)
│   ├── FinReg.Application/     # CQRS handlers, validators, MediatR pipeline
│   ├── FinReg.Infrastructure/  # EF Core, Npgsql, JWT, BCrypt, risk service
│   └── FinReg.API/             # Controllers, middleware, Swagger, Program.cs
├── tests/
│   ├── FinReg.Domain.Tests/
│   ├── FinReg.Application.Tests/
│   └── FinReg.Integration.Tests/   # TestContainers + WebApplicationFactory
└── frontend/                   # React 18 SPA
```
