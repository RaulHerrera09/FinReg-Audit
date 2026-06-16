# FinReg Audit Platform — Handoff Document

> This file is updated at the end of every phase. Read it immediately after DEVELOPMENT.md at the start of every session.

---

## Current Status

**Phase 6 — Testing, README & Deployment** is complete. All phases done. Live at https://finreg-audit.fly.dev/swagger (API) and https://fin-reg-audit.vercel.app (frontend).

---

## Phase 0 — Documentation & Setup

### Completed

- `DEVELOPMENT.md` created with full developer guide: solution structure, CLI commands, Event Sourcing explanation, CQRS guide, environment variables, deployment instructions, code conventions, and active skills summary.
- `HANDOFF.md` created (this file).
- `CONTEXT.md` created with project purpose, target audience, FCA compliance context, and Event Sourcing rationale.

### Completed (continued)

- `.gitignore` created (standard .NET + Node patterns)
- `.env.example` created with all required environment variable placeholders and comments
- `docker-compose.yml` created (PostgreSQL 16-alpine, port 5432, healthcheck)

### Completed (continued)

- `FinRegAuditPlatform.sln` created at repo root
- `src/FinReg.Domain` — classlib, zero NuGet deps, folders: Entities, Events, Enums, ValueObjects, Exceptions
- `src/FinReg.Application` — classlib, references Domain, folders: Common/Interfaces, Common/Behaviours, Common/Models, Accounts, Transactions, Alerts, Reports, Auth
- `src/FinReg.Infrastructure` — classlib, references Application, folders: Persistence, Persistence/EventStore, Persistence/Repositories, Identity, Services
- `src/FinReg.API` — webapi with controllers, references Application + Infrastructure, folders: Controllers, Middleware, Extensions
- `tests/FinReg.Domain.Tests` — xunit, references Domain
- `tests/FinReg.Application.Tests` — xunit, references Application
- `tests/FinReg.Integration.Tests` — xunit, references API
- `frontend/` — empty directory stub (populated in Phase 5)
- `dotnet build` — 7/7 projects, 0 warnings, 0 errors

### Architectural Decisions Made

None of significance yet — Phase 0 is documentation and scaffolding only.

### Next Steps

1. Await explicit approval for Phase 1 (Domain Layer & Event Sourcing Core).

### Known Issues / Tech Debt

None.

### .NET / EF Core Quirks Discovered

None at this stage.

---

## Phase History

| Phase | Completed | Notes |
|---|---|---|
| 0 | Complete | Docs + solution scaffold, 7 projects, 0 build errors |
| 1 | Complete | Domain layer — 5 enums, 2 value objects, 5 exceptions, 8 events, 3 entities, 39/39 tests |
| 2 | Complete | Application layer — 9 interfaces, 3 behaviours, 10 commands, 7 queries, 3 auth, DI extension, 8/8 tests |
| 3 | Complete | Infrastructure layer — 7 EF entities, 7 EF configs, AppDbContext, EventSerializer, EventStoreRepository (sync projections), 5 repositories, JwtService, BCryptPasswordHasher, RiskAssessmentService, DI extension — 47/47 tests pass |
| 4 | Complete | API layer — ExceptionHandlingMiddleware, CurrentUserService, JwtAuthentication, SwaggerWithJwt, 5 controllers (16 endpoints), Program.cs, Dockerfile (multi-stage), fly.toml (lhr region) — 47/47 tests pass |
| 5 | Complete | React 18 frontend — Vite + Tailwind CSS 4 + TanStack Query/Table + Zustand + Lucide React + React Router 7. Login page, Dashboard (stat cards + alerts), Accounts (table + open-account modal), Account Detail (transactions/alerts/compliance tabs + initiate/complete/flag modals), Alerts. TypeScript build clean. |
| 6 | Complete | Integration tests (TestContainers.PostgreSql + WebApplicationFactory) — 8 tests across Auth + AccountLifecycle. DatabaseSeeder (dev demo users). Root README.md (portfolio-grade). Live: https://finreg-audit.fly.dev/swagger + https://fin-reg-audit.vercel.app. Fixed POST /api/accounts/{id}/transactions 400 by adding JsonStringEnumConverter to JSON options. |

---

## Demo Credentials

| Role | Email | Password |
|---|---|---|
| ComplianceOfficer | compliance@demo.finreg.dev | Compliance@123 |
| Auditor | auditor@demo.finreg.dev | Audit@123 |

Seeded automatically by `DatabaseSeeder` on first startup in Development. For production, run `dotnet ef database update` then deploy — the seeder runs on app startup.

---

## Live URLs (populate after deployment)

| Service | URL |
|---|---|
| Swagger UI | https://finreg-audit.fly.dev/swagger |
| Frontend | https://fin-reg-audit.vercel.app |
| Health Check | https://finreg-audit.fly.dev/health |
