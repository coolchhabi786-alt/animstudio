---
description: "Use when writing, editing, or debugging C# code in AnimStudio. Covers architecture conventions, diagnosing cryptic compiler errors (CS0738/CS0535), MediatR v12 registration, EF Core migrations, ICacheService API, and module registration patterns."
applyTo: "backend/**/*.cs"
---

# AnimStudio — .NET / C# Conventions

## Architecture: Clean Module Structure

Each module (`IdentityModule`, `ContentModule`, etc.) follows this layout:
```
Domain/
  Entities/       ← AggregateRoot<TId> subclasses
  Interfaces/     ← Repository interfaces (IXxxRepository)
  Events/         ← Domain events
Application/
  Commands/       ← IRequest<Result<T>> + handler pairs
  Queries/        ← IRequest<Result<T>> + handler pairs
  DTOs/           ← sealed records, not classes
  Interfaces/     ← ICurrentUserService, ICacheService, etc.
Infrastructure/
  Persistence/    ← DbContext and entity configurations
  Repositories/   ← IXxxRepository implementations
  Services/       ← Infrastructure service implementations
XxxModuleRegistration.cs  ← Root-level DI registration only
```

**Never** create secondary registration files inside `Application/` or `Infrastructure/` subfolders — they cause duplicate type definitions that shadow the real types.

## Diagnosing CS0738 / CS0535 (Interface Not Implemented)

**Before changing any implementation code**, run this diagnostic:

```powershell
Get-ChildItem -Recurse -Path "src\AnimStudio.XxxModule" -Filter "*.cs" |
  Select-String -Pattern "class <TypeName>|interface I<TypeName>" |
  Select-Object Filename, LineNumber, Line
```

**Root cause in this codebase**: stale draft files in *parent* directories define types with the same name as canonical types in sub-namespaces. C# resolves enclosing-namespace names *before* `using` directives, so the wrong type wins silently.

Known stale files previously deleted (do not recreate):
- `Infrastructure/IdentityDbContext.cs` (bare `Plan` class)
- `Infrastructure/Repository.cs` (generic stub)
- `Application/IdentityModuleRegistration.cs` (stale CacheService stub)
- `Application/Queries/GetCurrentUserQuery.cs` (inline `UserDto`)

If CS0738 fires: **always grep for duplicate type/file definitions first**.

## MediatR v12 Registration

```csharp
// ✅ Correct (MediatR v12)
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(XxxModuleRegistration).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
});

// ❌ Wrong (MediatR v1–v11 overload — CS1503)
services.AddMediatR(typeof(XxxModuleRegistration).Assembly);
```

## ICacheService API

The canonical `ICacheService` interface uses:
- `GetOrSetAsync<T>(key, factory, absoluteExpiry, ct)`
- `GetAsync<T>(key, ct)`
- `SetAsync<T>(key, value, absoluteExpiry, ct)`
- `InvalidateAsync(key, ct)`        ← not `RemoveAsync`
- `InvalidateByPrefixAsync(prefix, ct)`  ← not `RemoveByPrefixAsync`

**Never** call `.RemoveAsync()` on `ICacheService` — it doesn't exist on the interface.

## EF Core Migrations

Always run from the solution root, targeting the API startup project:

```powershell
# Shared schema migrations
dotnet ef migrations add <Name> `
  --context SharedDbContext `
  --project src\AnimStudio.SharedKernel `
  --startup-project src\AnimStudio.API

# Module-specific migrations
dotnet ef migrations add <Name> `
  --context ContentDbContext `
  --project src\AnimStudio.ContentModule `
  --startup-project src\AnimStudio.API
```

## API Versioning

Always include `Asp.Versioning.Mvc` package and the `using Asp.Versioning;` directive when using `[ApiVersion]`:

```csharp
using Asp.Versioning;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/resource")]
public sealed class MyController : ControllerBase { }
```

## Exception Handler

Implement `Microsoft.AspNetCore.Diagnostics.IExceptionHandler` (not a custom interface):

```csharp
using Microsoft.AspNetCore.Diagnostics;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // ...
        return true;
    }
}
```

## General C# Style

- Use primary constructors (`public sealed class Foo(IBar bar)`) for services and handlers.
- Use `sealed` on non-inherited service/handler classes.
- DTOs are `sealed record`, not `class`.
- Return `Result<T>` (SharedKernel) from all command/query handlers — never throw from handlers.
- Avoid `async void`; always `async Task`.
