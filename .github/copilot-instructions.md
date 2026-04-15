# AnimStudio — GitHub Copilot Instructions

> **Source of truth for Copilot and all AI agent workflows.**
> Generated and maintained by the AnimStudio Dev Crew (CrewAI pipeline).
> See `C:\Users\Vaibhav\cartoon_automation\src\cartoon_automation\dev_crew.py` for the
> authoritative phase catalogue and agent definitions.

---

## Project Architecture

AnimStudio is a SaaS cartoon-production platform built as a **modular monolith** on:

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 8 / .NET 8, Clean Architecture |
| ORM | EF Core 8, SQL Server (schema-per-module) |
| CQRS | MediatR 12 + FluentValidation pipeline behaviour |
| Real-time | Azure SignalR (`Microsoft.AspNetCore.SignalR`) |
| Background jobs | Hangfire (queues: `critical`, `default`, `low`) |
| Messaging | Azure Service Bus (queues: `jobs-queue`, `completions-queue`, `character-training`) |
| Frontend | Next.js 14 App Router (TypeScript strict) |
| State | TanStack Query v5, Zustand |
| UI | shadcn/ui + Tailwind CSS + Framer Motion |
| Forms | react-hook-form + zod |
| Real-time (frontend) | `@microsoft/signalr` |
| Auth | Microsoft Entra External ID (JWT); `DevAuthHandler` bypass in Development |
| Infra | Azure Bicep, GitHub Actions CI/CD |

### Solution projects

```
backend/src/
  AnimStudio.SharedKernel/      ← AggregateRoot<T>, IDomainEvent, Result<T>, ICacheService
  AnimStudio.IdentityModule/    ← Users, Teams, Subscriptions (schema: identity.*)
  AnimStudio.ContentModule/     ← Projects, Episodes, Characters, Templates (schema: content.*)
  AnimStudio.DeliveryModule/    ← Renders, CDN delivery (schema: delivery.*)
  AnimStudio.AnalyticsModule/   ← Usage metering (schema: analytics.*)
  AnimStudio.API/               ← Program.cs, Controllers, Hubs, Middleware, SignalR notifiers
backend/tests/
  AnimStudio.UnitTests/
  AnimStudio.IntegrationTests/
frontend/src/
  app/(dashboard)/              ← Route group for authenticated pages
  components/                   ← UI components by domain
  hooks/                        ← TanStack Query + SignalR hooks
  lib/                          ← api-client, auth, stripe-client
  stores/                       ← Zustand stores
  types/                        ← Shared TypeScript interfaces (index.ts)
infra/                          ← Bicep modules + parameters
e2e/                            ← Playwright tests
security/                       ← Per-phase OWASP security reviews
specs/                          ← OpenAPI specs + architecture docs per phase
```

---

## Phase Catalogue

| # | Name | Key Feature |
|---|------|-------------|
| 1 | Foundation | .NET skeleton, Bicep infra, Entra auth, Stripe billing, Next.js shell |
| 2 | Project & Episode Management | DB models, state machine, dashboard, SignalR job tracking |
| 3 | Template & Style Library | Genre templates, style presets, starter packs |
| **4** | **Character Studio** | LoRA training pipeline, character gallery, team-scoped library ← **CURRENT** |
| 5 | Script Workshop | Screenplay generation, inline editor, regenerate-with-notes |
| 6 | Storyboard Studio | Grid, per-shot regenerate, style override |
| 7 | Voice Studio | Voice assignment, TTS preview, multi-language, voice cloning |
| 8 | Animation Studio | Cost estimate, approval flow, per-shot clip preview |
| 9 | Post-Production & Delivery | Final render, CDN delivery, aspect ratio exports, SRT captions |
| 10 | Timeline Editor | Drag-and-drop shot reorder, clip trimming, transitions, music |
| 11 | Sharing & Publishing | Review links, comment threads, YouTube publish, brand watermark |
| 12 | Analytics, Admin & Usage Controls | Usage metering, admin dashboard, notifications |

---

## The Dev Crew: Agent Roles & DNA

The following agents form the CrewAI Dev Crew that built this codebase. Their roles define
the **cognitive contract** Copilot should follow when generating code in this repository.

### Architect Agent
**Role**: System Architect & Tech Lead  
**Mindset**: Design first. No file is created without a spec. Owns OpenAPI contracts, Bicep blueprints, ADRs (Architecture Decision Records), and cross-cutting concerns.

**Responsibilities in this codebase**:
- Author `specs/openapi/phase{N}-*.yaml` before any code is generated
- Author `specs/phase{N}-*-architecture.md` with entity schemas, SB message contracts, OWASP notes
- Define module boundaries — no circular dependencies across projects
- Decide on patterns: port/adapter for cross-module communication (e.g. `ICharacterProgressNotifier`)

**Copilot extension**: When designing a new feature, always write the OpenAPI spec and entity schema first. Never generate a handler without a corresponding DTO defined.

---

### Backend Dev Agent
**Role**: Senior .NET 8 Backend Engineer  
**Mindset**: Clean Architecture, DRY, SOLID. No business logic in controllers. MediatR for every user action.

**Coding standards**:
```csharp
// Domain entity: ALWAYS use AggregateRoot<TId> from SharedKernel
public sealed class MyEntity : AggregateRoot<Guid> { }

// Factory method: never use public setters; expose intent-revealing methods
public static MyEntity Create(Guid teamId, string name) => new() { ... };

// Commands: IRequest<Result<TDto>>, raise domain events, NOT infrastructure calls
public sealed record CreateX(string Name) : IRequest<Result<XDto>>;

// BOLA: always check ownership before returning data
if (entity.TeamId != _currentUser.GetCurrentTeamId())
    return Result.Failure<XDto>(Error.Forbidden("Access denied."));

// Port interface to break circular deps (Application → API)
// Define in ContentModule.Application.Interfaces, implement in AnimStudio.API
public interface IXProgressNotifier { Task NotifyAsync(...); }
```

**Naming conventions**:
- Handlers: `CreateXCommandHandler`, `GetXQueryHandler`
- Commands: past-tense nouns: `CreateCharacterCommand`, `DeleteCharacterCommand`
- DTOs: `XDto` (sealed records, immutable)
- Repositories: `IXRepository` (interface in Application/Interfaces, impl in Infrastructure/Repositories)

**Must-follow rules**:
- MediatR v12: `services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(...))` — never the v11 overload
- `ICacheService`: use `InvalidateAsync(key)` not `RemoveAsync(key)`
- EF migrations: always run with `--startup-project src\AnimStudio.API`
- Schema prefix: `content.*`, `identity.*`, `shared.*` — never use `dbo.*`
- Soft-delete: never hard-delete Domain entities; use `DeletedAt IS NULL` global filter in DbContext

---

### Frontend Dev Agent
**Role**: Senior Next.js 14 / TypeScript Engineer  
**Mindset**: Server Components for data, Client Components for interaction. TanStack Query for server state, Zustand for client state.

**Coding standards**:
```typescript
// All data fetching: TanStack Query with typed apiFetch
const { data } = useQuery<XDto>({
  queryKey: ["x", id],
  queryFn: () => apiFetch<XDto>(`/api/v1/x/${id}`),
  staleTime: 30_000,
});

// Mutations: invalidate related queries on success
const mutation = useMutation({
  mutationFn: (payload: CreateXPayload) =>
    apiFetch<XJobAcceptedDto>("/api/v1/x", { method: "POST", body: JSON.stringify(payload) }),
  onSuccess: () => qc.invalidateQueries({ queryKey: ["x"] }),
});

// SignalR: always use useCharacterTrainingUpdates() pattern
// — connect on mount, disconnect on unmount, update query cache on message

// Forms: react-hook-form + zod
const schema = z.object({ name: z.string().min(1).max(200) });
const { register, handleSubmit, formState: { errors } } = useForm({ resolver: zodResolver(schema) });
```

**Naming conventions**:
- Hooks: `use-kebab-case.ts` (e.g. `use-characters.ts`)
- Components: `PascalCase.tsx` (e.g. `CharacterCard.tsx`)
- Pages: `page.tsx` — never `Page.tsx` (Next.js convention)
- Route group: `(dashboard)` — never `(studio)` (existing project uses `(dashboard)`)

**Must-follow rules**:
- All types in `src/types/index.ts` — never define DTOs inline in component files
- Use `cn()` from `@/lib/utils` for conditional class merging — never string template literals
- `Image` from `next/image` for all images — never `<img>` without `alt`
- Accessible by default: `aria-label` on icon-only buttons, `role="progressbar"` with `aria-valuenow`

---

### DevOps Agent
**Role**: Azure Cloud Infrastructure Engineer  
**Mindset**: Infrastructure as Code, least-privilege, idempotent deployments.

**Bicep conventions**:
```bicep
// Always use API version from 2022+ for Service Bus
resource myQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  name: 'queue-name'
  parent: serviceBusNamespace
  properties: {
    lockDuration: 'PT5M'   // ISO 8601
    maxDeliveryCount: 3
  }
}
```

**Must-follow rules**:
- Never hardcode secrets — all secrets in Azure Key Vault; reference via `@Microsoft.KeyVault()`
- New Service Bus queues go in `infra/modules/servicebus.bicep`
- New queues with long-running operations (>5 min): set `lockDuration: 'PT30M'` and `requiresDuplicateDetection: true`
- CI/CD: `infra.yml` deploys Bicep on `push to main/infra/**`; never deploy infra from `deploy-api.yml`

---

### QA Agent
**Role**: Quality Assurance Engineer (unit, integration, E2E, security)  
**Mindset**: Test behaviour, not implementation. Arrange-Act-Assert. One assertion concept per test.

**Test conventions**:
```csharp
// Unit test naming: Method_Scenario_ExpectedOutcome
[Fact]
public async Task Handle_ValidCommand_CreatesCharacterAndQueuesTraining() { ... }

// Use Moq for dependencies: Mock<IXRepository>, verify meaningful interactions
_repoMock.Verify(r => r.AddAsync(It.Is<Character>(c => c.Name == "Test"), ...), Times.Once);
```

**Playwright conventions**:
```typescript
// Use role-based selectors — never CSS classes or test IDs unless unavoidable
await page.getByRole("button", { name: /create character/i }).click();
await expect(page.getByRole("heading", { name: "Team Characters" })).toBeVisible();
```

**Test coverage targets**:
- Unit: ≥80% line coverage on Application layer handlers
- E2E: happy path + 1 auth/BOLA path per new feature page
- Security: per-phase OWASP review in `security/phase{N}-*-security-review.md`

---

### Validator Agent
**Role**: Code Review & Integration Validator  
**Mindset**: Nothing ships without a Review. Catch CS0738/CS0535 duplicate type issues. Validate OpenAPI matches controllers. Validate migration matches DbContext.

**Pre-merge checklist**:
- [ ] `dotnet build` passes with 0 errors, 0 warnings
- [ ] `dotnet test` passes
- [ ] EF migration `Up()` matches new `DbSet<>` properties in `ContentDbContext`
- [ ] OpenAPI spec endpoints match controller routes exactly (verb, path, status codes)
- [ ] Security review doc exists for new phase
- [ ] No circular project references (`dotnet list reference` check)
- [ ] BOLA check on all new GET/DELETE endpoints
- [ ] New types added to `frontend/src/types/index.ts`

---

## Critical Pattern Reference

### Module Registration (one per module)
```csharp
// XxxModuleRegistration.cs — root-level ONLY, no sub-files
public static class ContentModuleRegistration
{
    public static IServiceCollection AddContentModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ContentModuleRegistration).Assembly));
        services.AddScoped<IEpisodeRepository, EpisodeRepository>();
        services.AddScoped<ICharacterRepository, CharacterRepository>();
        // etc.
        return services;
    }
}
```

### Result Pattern
```csharp
// Success
return Result.Success(dto);
// Failure
return Result.Failure<XDto>(Error.NotFound("X.NotFound", $"X {id} not found."));
return Result.Failure<XDto>(Error.Forbidden("X.Forbidden", "Access denied."));
return Result.Failure<XDto>(Error.Conflict("X.InUse", "Cannot delete: in use."));
```

### SignalR Hub Group Naming
```csharp
// Character training progress: group per team
await Groups.AddToGroupAsync(Context.ConnectionId, $"team:{teamId}");
await _hubContext.Clients.Group($"team:{teamId}").SendAsync("CharacterTrainingUpdate", payload);

// Episode progress: group per episode
await Groups.AddToGroupAsync(Context.ConnectionId, $"episode:{episodeId}");
```

### EF Core Schema Conventions
```csharp
// In DbContext OnModelCreating:
entity.ToTable("TableName", "content");       // schema-per-module
entity.HasQueryFilter(e => e.DeletedAt == null); // global soft-delete filter
entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
```

---

## Common Mistakes to Avoid

| Mistake | Correct Approach |
|---------|-----------------|
| `services.AddMediatR(typeof(X).Assembly)` | Use `cfg.RegisterServicesFromAssembly(typeof(X).Assembly)` (MediatR v12) |
| `_cache.RemoveAsync(key)` | Use `_cache.InvalidateAsync(key)` |
| `teamId` from request body/query string | Always from `ICurrentUserService.GetCurrentTeamId()` (JWT) |
| Route group `(studio)` | Use `(dashboard)` — existing project convention |
| New registration file in `Application/` or `Infrastructure/` subfolder | Registration only in root `XxxModuleRegistration.cs` |
| Raw SQL string in migration `Up()` | Use `migrationBuilder.CreateTable(...)` API |
| `<img>` tag in frontend | Use `next/image` `<Image>` with `alt` and `sizes` |
| `IExceptionHandler` from custom namespace | Implement `Microsoft.AspNetCore.Diagnostics.IExceptionHandler` |
