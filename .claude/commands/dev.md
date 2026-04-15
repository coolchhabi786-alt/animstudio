# AnimStudio Development Assistant

You are an expert developer for the **AnimStudio** SaaS cartoon-production platform — a .NET 8 modular monolith backed by a Next.js 14 frontend and Azure infrastructure.

## Project Layout

```
animstudio/
├── backend/src/
│   ├── AnimStudio.API/           # ASP.NET Core host — Controllers, Hubs, Middleware, Program.cs
│   ├── AnimStudio.SharedKernel/  # AggregateRoot<Guid>, Result<T>, ICacheService, Jobs
│   ├── AnimStudio.IdentityModule/  # Users, Teams, Subscriptions, Stripe billing
│   ├── AnimStudio.ContentModule/   # Projects, Episodes, Characters, Scripts, Templates
│   ├── AnimStudio.DeliveryModule/  # Renders, CDN, file upload/download
│   └── AnimStudio.AnalyticsModule/ # Usage metering, telemetry
├── frontend/src/
│   ├── app/(auth)/               # Public auth pages
│   ├── app/(dashboard)/          # Protected pages (route groups)
│   ├── components/               # shadcn/ui + domain components
│   ├── hooks/                    # TanStack Query + SignalR hooks
│   ├── stores/                   # Zustand client state
│   ├── lib/api-client.ts         # Centralised fetch wrapper (auth + error toasting)
│   └── types/index.ts            # ALL TypeScript interfaces (never inline)
└── infra/                        # Azure Bicep IaC
```

## Backend Rules (enforce strictly)

- **Entities**: Always extend `AggregateRoot<Guid>` from `AnimStudio.SharedKernel`.
- **Commands/Queries**: `IRequest<Result<TDto>>` via MediatR; raise domain events — never call infra directly in handlers.
- **BOLA check**: Every handler that returns owned data must verify `entity.TeamId == _currentUser.GetCurrentTeamId()`.
- **Cache**: `_cacheService.InvalidateAsync(key)` — never `RemoveAsync`.
- **DB schema prefixes**: `content.*`, `identity.*`, `shared.*` — never `dbo.*`.
- **Soft-delete**: Global filter `DeletedAt IS NULL` already applied in DbContext; don't add manual `.Where(x => x.DeletedAt == null)`.
- **Migrations**: Always pass `--startup-project src\AnimStudio.API`.
- **Jobs**: Enqueue via Hangfire queues (`critical`, `default`, `low`); long tasks go to `low`.
- **Secrets**: Azure Key Vault only — never hardcode connection strings or keys.

## Frontend Rules (enforce strictly)

- **API calls**: Always use `apiFetch()` from `@/lib/api-client` — handles auth headers, error toasting, and 401 redirect.
- **Server state**: TanStack Query (`useQuery` / `useMutation`) + `queryClient.invalidateQueries()` after mutations.
- **UI components**: shadcn/ui only — never raw `<button>`, `<input>`, `<select>`.
- **Styling**: `cn()` from `@/lib/utils` for conditional Tailwind classes.
- **Types**: All interfaces live in `/src/types/index.ts`.
- **Forms**: `react-hook-form` + `zod` schema + `@hookform/resolvers/zod`.
- **Dev auth**: `DevAuthHandler` auto-authenticates in development — no auth header needed locally.

## Key Commands

```bash
# Start local services
docker compose up -d

# Run EF migrations (backend/)
dotnet ef database update \
  --project src\AnimStudio.IdentityModule \
  --startup-project src\AnimStudio.API \
  --context IdentityDbContext

dotnet ef database update \
  --project src\AnimStudio.SharedKernel \
  --startup-project src\AnimStudio.API \
  --context SharedDbContext

# Run API (http://localhost:5001, Swagger /swagger, Hangfire /hangfire)
cd backend/src/AnimStudio.API && dotnet run

# Run frontend (http://localhost:3000)
cd frontend && pnpm install && pnpm dev

# Tests
dotnet test               # backend
pnpm test                 # frontend (jest)

# Linting
pnpm lint

# Production build check
pnpm build
```

## Module Architecture (per domain module)

Each module follows Clean Architecture:
```
AnimStudio.<Module>/
  Domain/          # Entities, ValueObjects, DomainEvents (no infra deps)
  Application/     # Commands, Queries, Handlers, DTOs, Interfaces
  Infrastructure/  # DbContext, Repositories, EF-specific services
  Migrations/      # EF Core migrations (run with --startup-project API)
```

## Real-time (SignalR)

- Hub classes live in `AnimStudio.API/Hubs/`
- Client uses `@microsoft/signalr` via custom hooks in `frontend/src/hooks/`
- Azure SignalR in production; local SignalR in dev

## Service Bus Queues

| Queue | Purpose |
|-------|---------|
| `jobs-queue` | Inbound AI/render job requests |
| `completions-queue` | Job completion notifications |
| `character-training` | LoRA training requests to Python worker |

## Infrastructure

- **IaC**: Azure Bicep in `infra/`; modules for Container Apps, SQL, Redis, Service Bus, Key Vault.
- **CI/CD**: GitHub Actions — `deploy-api.yml`, `deploy-web.yml`, `infra.yml`.
- **Local**: `docker-compose.yml` runs SQL Server 2022 + Redis 7.

## What to do now

The user's message (above `$ARGUMENTS`) describes the task. Apply the rules and patterns above. Read relevant files before suggesting changes. When adding a new feature:

1. Define the domain entity/value object in `Domain/`.
2. Add command + handler in `Application/` with MediatR.
3. Add DbContext configuration and migration in `Infrastructure/`.
4. Expose via controller in `AnimStudio.API/Controllers/`.
5. Add TanStack Query hook + shadcn/ui component in the frontend.

Always check for BOLA, cache invalidation, and schema prefix correctness before finishing.

$ARGUMENTS
