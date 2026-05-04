# AnimStudio — Production Readiness: Gap Analysis & Implementation Plan

## Context

Both sub-projects (AnimStudio .NET/Next.js SaaS platform and the Python `cartoon_automation` pipeline) are individually functional in local dev but operate in complete isolation. The goal of this plan is to connect them through Azure cloud infrastructure and bring the combined system to production quality for real consumers.

**Key facts driving this plan:**
- `cartoon_automation` writes output to `C:\Users\Vaibhav\cartoon_automation\output` (local disk)
- AnimStudio reads from the same local disk path (hardcoded in Program.cs line 605)
- In production both systems must communicate via **Azure Blob Storage + Service Bus**
- `CompletionMessageProcessor` (the Service Bus listener) is a **stub** — it logs but does not process results
- `AnimationJobHangfireProcessor.ProcessKlingStubAsync()` just logs a warning
- `AzureBlobFileStorageService.SaveFileAsync()` **throws NotSupportedException**
- Azure Bicep modules exist but are **never deployed** (no scripts, no CI/CD)
- Phases 11 (Sharing/Review) and 12 (Analytics/Admin) are **not implemented** on the backend
- Frontend still has mock interceptors active for projects, episodes, characters, saga state, voice preview

---

## Gap Analysis

### GAP-1: Azure Infrastructure — Exists in Bicep, Not Deployed

**What exists:** Bicep modules for Container Apps, SQL, Redis, Service Bus (4 queues), Blob Storage, Key Vault, ACR, SignalR, Front Door.

**What's missing:**
- No `azuredeploy.parameters.*.json` files for dev/staging/prod environments
- No deployment script (`.ps1` or `.sh`) that runs `az deployment sub create`
- Hardcoded resource names throughout Bicep (not parameterised per environment)
- No Managed Identity wiring (Container App MSI → Key Vault, MSI → Blob, MSI → Service Bus)
- No CI/CD pipelines for Bicep (`infra.yml` GitHub Action references but may be incomplete)
- `AzureBlobFileStorageService.SaveFileAsync()` throws `NotSupportedException` — files can't be written to Blob
- No Azure CDN profile/endpoint on the Blob Storage account
- No App Registration configured for Entra External ID (auth Authority is empty string in dev)

---

### GAP-2: Integration Bridge — cartoon_automation ↔ AnimStudio

**What exists (local dev only):**
- `LocalFileStorageService` serves files from `C:\Users\Vaibhav\cartoon_automation\output`
- `AnimationJobHangfireProcessor` scans local `animation/` folder for mp4s
- `RenderHangfireProcessor` reads from local `final/` folder

**What's missing (production path):**
- `cartoon_automation` has **no Service Bus producer** — it doesn't publish to `jobs-queue`
- `cartoon_automation` has **no Blob Storage uploader** — it writes to local disk only
- `CompletionMessageProcessor` in AnimStudio is a **stub** — it never dispatches `HandleJobCompletionCommand`
- `AnimationJobHangfireProcessor.ProcessKlingStubAsync()` is a no-op
- No webhook endpoint in AnimStudio for Kling AI callbacks
- No shared contract file (`job_schemas.json` or similar) defining the Service Bus message payloads

**The missing data flow in production:**
```
AnimStudio (jobs-queue) → cartoon_automation (consumes) → runs pipeline → uploads to Blob
                       → publishes to completions-queue → CompletionMessageProcessor (processes)
                       → HandleJobCompletion → advances episode saga → SignalR to browser
```

---

### GAP-3: Backend Stubs — Must Be Made Real

| Component | File | Current State | Must Become |
|---|---|---|---|
| `AzureBlobFileStorageService.SaveFileAsync` | `API/Services/AzureBlobFileStorageService.cs` | `throw NotSupportedException` | Real Blob SDK upload |
| `AzureBlobFileStorageService.GetFileUrl` | same | Returns raw blob URI | Returns SAS URL (30-day) |
| `VoicePreviewService` | `API/Services/VoicePreviewService.cs` | Returns placeholder MP3 URL | Real Azure OpenAI TTS synthesis |
| `VoiceCloneService` | `API/Services/VoiceCloneService.cs` | Returns `NotAvailable` | ElevenLabs API integration |
| `CompletionMessageProcessor` | `API/Hosted/CompletionMessageProcessor.cs` | Logs but ignores | Routes by `jobType` → `HandleJobCompletionCommand` |
| `AnimationJobHangfireProcessor` (Kling path) | `API/Hosted/AnimationJobHangfireProcessor.cs` | Logs stub warning | Enqueues job on Service Bus for Python pipeline |
| `RenderHangfireProcessor` | `API/Hosted/RenderHangfireProcessor.cs` | Uses local final/ fallback | Real FFmpeg render pipeline |
| `HandleJobCompletionCommand` | `ContentModule/Application/Commands/HandleJobCompletion/` | Generic handler exists | Needs type-dispatch to per-phase result handlers |

---

### GAP-4: Missing Backend Phases (11 & 12)

**Phase 11 — Sharing & Review** (0% implemented):
- No `ReviewLink` entity/table/repository
- No `ReviewComment` entity/table/repository  
- No `BrandKit` entity/table/repository
- No `SocialPublish` (YouTube OAuth) entity
- No `ReviewController`
- No public `/review/{token}` endpoint

**Phase 12 — Analytics & Admin** (0% implemented):
- No `VideoView` tracking entity
- No `Notification` entity/table/repository
- No `AnalyticsController` or `AdminController`
- `AnalyticsModule` exists as project but has no entities or handlers
- No usage metering increments on episode completion

---

### GAP-5: Frontend De-Mocking

**Active mock interceptors that must connect to real backend:**
- `GET /api/v1/projects` → mock projects
- `POST /api/v1/projects` → mock create
- `GET /api/v1/episodes` → mock episodes
- `POST /api/v1/episodes` → mock create
- `GET /api/v1/characters` → mock character page
- `POST /api/v1/voices/preview` → SoundHelix placeholder URL
- `GET /api/v1/episodes/{id}/saga` → mock saga state

**UI gaps (hardcoded/coming-soon):**
- Voice cloning upload: `toast.info('...feature coming soon')`
- Phase 11 (Review/Sharing) pages: not built
- Phase 12 (Analytics) pages: have mock data but no real hooks

---

### GAP-6: FFmpeg Pipeline — Not Integrated

- `RenderHangfireProcessor` uses file-size heuristic for duration; no actual video assembly
- No FFmpeg filter graph generator for: clip sequence, transitions, audio mixing, text overlays
- No SRT caption generation from screenplay dialogue timeline
- Timeline editor saves track/clip data to DB but there is no code that converts this to an FFmpeg command

---

### GAP-7: CI/CD & Production Deployment

- `.github/workflows/deploy-api.yml` and `deploy-web.yml` exist but their contents are unknown
- No staging environment defined
- Secrets not populated in Key Vault
- Container image not built/pushed to ACR
- No smoke-test job after deployment

---

## Cost Optimisation Decisions

Three infrastructure changes have been evaluated and approved to reduce fixed monthly cost before the first consumer launch. These decisions are factored into Track 1 below and all Bicep work.

---

### OPT-1 — Replace Frontend Container App with Azure Static Web Apps ✅ FEASIBLE

**Saving: $22–31/month** (SWA Standard $9 vs Container App $31–39)

**Why it works:**
- The Next.js frontend communicates with the API **only** via REST (`apiFetch()`) and SignalR WebSocket — it has no direct DB access, so it can live on a different hosting platform with no code changes.
- Azure Static Web Apps (SWA) has **native Next.js App Router support** including SSR via Hybrid Rendering.
- Built-in global CDN, custom domain, SSL, and GitHub Actions deployment — no extra work vs. a Container App.
- SWA Standard ($9/mo) supports custom auth rules (needed for Entra External ID redirect) and has no bandwidth cap (overages at $0.20/GB). Free tier ($0/mo) works for very early traffic (<100GB/month bandwidth).
- SignalR WebSocket connections: SWA supports API proxying (`staticwebapp.config.json` → proxy `/hubs/*` to the API Container App URL).

**What changes (Bicep):**
- **New file:** `infra/modules/staticwebapp.bicep` — provisions `Microsoft.Web/staticSites` resource
- **Remove** from `main.bicep`: the Container App call that deploys the Next.js frontend image
- **Update** `infra/modules/frontdoor.bicep`: change the frontend origin group from Container App to SWA default hostname
- **Update** `infra/parameters/dev.json` and `prod.json`: remove `frontendImageTag`, add `swaRepoUrl` and `swaBranch`
- **Update** `.github/workflows/deploy-web.yml`: replace `docker build → containerapp update` with the SWA GitHub Action (`Azure/static-web-apps-deploy@v1`)

**Caveat:** If the frontend ever needs direct server-to-server calls (e.g., reading Key Vault from an RSC), it would need the Standard tier API backend option in SWA (additional $9/mo). Not needed currently.

---

### OPT-2 — KEDA Scale-to-Zero for Python Worker ✅ FEASIBLE

**Saving: $14–17/month** at startup traffic (worker idle >85% of the time)

**Why it works:**
- Azure Container Apps has **built-in KEDA support** — no extra tooling needed.
- Setting `minReplicas: 0` and adding Service Bus KEDA scaling rules means the container is completely stopped when both `jobs-queue` and `character-training` are empty.
- Container App charges only for **active vCPU/memory seconds** — zero replicas = zero cost.
- Cold start for the Python worker is ~30–60 seconds. This is acceptable because jobs are already queued and the user is watching a progress screen, not waiting for a synchronous response.
- At startup volume (say 10 episodes/month, each requiring ~40 minutes of worker time): 10 × 40 min = 6.7 hours active out of 720 hours → **<1% active time** → cost ≈ $0.50–1.00/month instead of $5–18/month.

**What changes (Bicep):**
- **New file:** `infra/modules/containerapp-worker.bicep` — separate module for the Python worker with KEDA rules (see code below). Do **not** modify the generic `containerapp.bicep` — keep that clean for the API.
- **Update `main.bicep`**: add the new `containerapp-worker` module call; remove the Python worker from any existing generic Container App call.
- **Key KEDA scaling block** (within `containerapp-worker.bicep`):

```bicep
scale: {
  minReplicas: 0
  maxReplicas: 5
  rules: [
    {
      name: 'jobs-queue-keda'
      custom: {
        type: 'azure-servicebus'
        metadata: {
          queueName: 'jobs-queue'
          messageCount: '1'   // 1 message → 1 replica; 2 messages → 2 replicas, etc.
          namespace: serviceBusNamespaceName
        }
        auth: [{ secretRef: 'sb-connection-string', triggerParameter: 'connection' }]
      }
    }
    {
      name: 'training-queue-keda'
      custom: {
        type: 'azure-servicebus'
        metadata: {
          queueName: 'character-training'
          messageCount: '1'
          namespace: serviceBusNamespaceName
        }
        auth: [{ secretRef: 'sb-connection-string', triggerParameter: 'connection' }]
      }
    }
  ]
}
```

**Full `infra/modules/containerapp-worker.bicep`** (new file to create):
```bicep
param environment string
param location string
param containerAppEnvironmentId string
param serviceBusNamespaceName string
param serviceBusConnectionString string
param acrLoginServer string
param imageTag string = 'latest'
param maxReplicas int = 5

resource workerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: 'animstudio-${environment}-worker'
  location: location
  identity: { type: 'SystemAssigned' }
  properties: {
    environmentId: containerAppEnvironmentId
    configuration: {
      secrets: [
        { name: 'sb-connection-string', value: serviceBusConnectionString }
      ]
      registries: [{ server: acrLoginServer, identity: 'system' }]
    }
    template: {
      containers: [
        {
          image: '${acrLoginServer}/cartoon-automation:${imageTag}'
          name: 'cartoon-automation'
          resources: { cpu: json('0.5'), memory: '1Gi' }
          env: [
            { name: 'AZURE_SERVICE_BUS_CONNECTION_STRING', secretRef: 'sb-connection-string' }
            { name: 'AZURE_SERVICE_BUS_JOBS_QUEUE',        value: 'jobs-queue' }
            { name: 'AZURE_SERVICE_BUS_COMPLETIONS_QUEUE', value: 'completions-queue' }
            { name: 'AZURE_SERVICE_BUS_TRAINING_QUEUE',    value: 'character-training' }
            { name: 'ENVIRONMENT',                         value: environment }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: maxReplicas
        rules: [
          {
            name: 'jobs-queue-keda'
            custom: {
              type: 'azure-servicebus'
              metadata: { queueName: 'jobs-queue', messageCount: '1', namespace: serviceBusNamespaceName }
              auth: [{ secretRef: 'sb-connection-string', triggerParameter: 'connection' }]
            }
          }
          {
            name: 'training-queue-keda'
            custom: {
              type: 'azure-servicebus'
              metadata: { queueName: 'character-training', messageCount: '1', namespace: serviceBusNamespaceName }
              auth: [{ secretRef: 'sb-connection-string', triggerParameter: 'connection' }]
            }
          }
        ]
      }
    }
  }
}

output workerPrincipalId string = workerApp.identity.principalId
output workerName string = workerApp.name
```

---

### OPT-3 — Remove Redis Cache (Initial Launch) ✅ FEASIBLE — zero code changes required

**Saving: $16–55/month** (C0 Basic $16/mo or C1 Standard $55/mo removed entirely)

**Why it works — the fallback already exists:**
`Program.cs` (lines 131–136) already contains this exact code:
```csharp
var redisConn = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisConn))
    builder.Services.AddStackExchangeRedisCache(o => o.Configuration = redisConn);
else
    builder.Services.AddDistributedMemoryCache();   // ← already live in dev
```
Simply **do not provision Redis** and **do not set the `Redis` connection string** in Key Vault. The app silently falls back to `IMemoryCache` with no crashes, no retries, and no code edits.

**Cache behaviour with in-memory fallback:**
- `ICacheService.GetOrSetAsync` / `GetAsync` / `SetAsync` / `InvalidateAsync` — all work identically against `IDistributedCache`, which now points to in-memory storage.
- `InvalidateByPrefixAsync` — currently a no-op in the implementation (requires `IConnectionMultiplexer`). Already no-op; no regression.
- `CachingBehaviour` (MediatR pipeline): Works transparently — queries tagged with `ICacheKey` are still cached in-process.

**Constraint:** In-memory cache is **per-replica**. If the API Container App scales to 2+ replicas, cache invalidation will not propagate between instances (one replica invalidates, another still returns stale data). This is acceptable at launch with single-replica API (`minReplicas: 1`, `maxReplicas: 1`). Add Redis back when you increase `maxReplicas` to 2+.

**What changes (Bicep):**
- **`infra/main.bicep`**: Comment out (do not delete) the `redis` module call — one line change. Add a `// TODO: uncomment when API scales beyond 1 replica` note.
- **`infra/modules/keyvault.bicep`**: Remove the `RedisConnectionString` secret resource (or set its value to empty string — Key Vault does not allow empty secrets, so remove the resource).
- **`infra/parameters/dev.json` and `prod.json`**: Remove `redisSkuName`, `redisCacheName`, `redisCapacity` parameters.
- **Keep `infra/modules/redis.bicep`** — do not delete, just stop referencing it from `main.bicep`. Re-enable later by un-commenting the module call.

**Trigger to re-add Redis:** When `maxReplicas` for the API Container App is raised above 1 (growth phase). At that point: uncomment the redis module, add the Key Vault secret, redeploy infra — no code changes needed.

---

### Revised Fixed Monthly Infrastructure Cost

All figures are Azure Pay-As-You-Go, East US, based on active optimisations above.

#### Startup Tier (launch → first ~100 users)

| Service | SKU | Before Opt | After Opt | Saving |
|---------|-----|-----------|----------|--------|
| Container App — API | Consumption 1 vCPU / 2 GB, min 1 | $62 | $62 | — |
| Container App — Frontend | Consumption 0.5 vCPU / 1 GB, min 1 | $31 | **$0–$9** (SWA Free/Standard) | $22–31 |
| Container App — Python Worker | Consumption 0.5 vCPU / 1 GB, **min 0 KEDA** | $5 | **$1** (~5% active) | $4 |
| Azure SQL Database | Serverless Gen5, 1–2 vCores, auto-pause | $40 | $40 | — |
| Azure Redis Cache | C0 Basic 250 MB | $16 | **$0** (removed) | $16 |
| Azure Service Bus | Standard, 4 queues | $10 | $10 | — |
| Azure Blob Storage | Hot tier ~50 GB + basic CDN | $5 | $5 | — |
| Azure Key Vault | Standard, ~50K ops/mo | $3 | $3 | — |
| Azure Container Registry | Basic | $5 | $5 | — |
| Azure SignalR Service | Standard 1 unit | $49 | $49 | — |
| Azure Monitor / App Insights | ~5 GB/mo | $5 | $5 | — |
| **TOTAL** | | **~$231** | **~$180–$189** | **$42–51/mo** |

#### Growth Tier (100+ users, API scales to 2 replicas)

| Change from Startup | Impact |
|--------------------|--------|
| Add Redis C1 Standard (shared cache across 2 API replicas) | +$55/mo |
| API scales to 2 replicas (auto on load) | +$62/mo |
| Python Worker more active (~20% time, more episodes) | +$8/mo |
| SWA bandwidth overages (>100 GB) | +$0–$20/mo |
| **Estimated growth total** | **~$325–$335/mo** |

#### Full Production Tier (post-optimisation, vs original estimate)

| | Original Estimate | After Optimisations | Saving |
|--|------------------|--------------------|----|
| Startup | $231/mo | $180–$189/mo | $42–51/mo |
| Full production | $506/mo | $406–$461/mo | $45–100/mo |

> The $406 figure assumes Redis stays off at full scale. The $461 figure adds Redis back when needed. Either way, Front Door ($40) and SQL HA ($30) are included at full production.

---

## Implementation Plan

The work is organized into **6 tracks** that can run in parallel where dependencies allow.

```
Track 1: Azure Infrastructure      (week 1-2)  ← blocks everything else
Track 2: Integration Bridge        (week 2-5)  ← depends on Track 1
Track 3: Backend Stubs → Real      (week 2-4)  ← depends on Track 1 for Blob
Track 4: Missing Phases 11 & 12    (week 3-6)  ← depends on Track 1
Track 5: FFmpeg Pipeline           (week 4-7)  ← depends on Track 1 Blob + Track 2
Track 6: Frontend & CI/CD          (week 5-8)  ← depends on Tracks 2-5
```

---

## Track 1: Azure Infrastructure Provisioning

### 1.0 Cost Optimisation Bicep Changes (implement first — unblocks all cost savings)

Apply all three OPT decisions from the "Cost Optimisation Decisions" section above. These are Track 1's first commit.

**Files to create:**
| File | Action | Purpose |
|------|--------|---------|
| `infra/modules/staticwebapp.bicep` | **Create new** | Provisions Azure Static Web Apps for Next.js frontend (OPT-1) |
| `infra/modules/containerapp-worker.bicep` | **Create new** | Python worker Container App with KEDA scale-to-zero (OPT-2) |

**Files to modify:**
| File | Change |
|------|--------|
| `infra/main.bicep` | Remove frontend Container App call; add `staticwebapp` module; add `containerapp-worker` module; comment out `redis` module |
| `infra/modules/keyvault.bicep` | Remove `RedisConnectionString` secret resource |
| `infra/parameters/dev.json` | Remove `redisSkuName`, `redisCacheName`, `redisCapacity`; add `swaRepoUrl`, `swaBranch` |
| `infra/parameters/prod.json` | Same as dev.json changes |
| `.github/workflows/deploy-web.yml` | Replace `docker build → containerapp update` with `Azure/static-web-apps-deploy@v1` action |

**`infra/modules/staticwebapp.bicep`** (full content — create this file):
```bicep
param environment string
param location string = resourceGroup().location
param skuName string = 'Standard'   // 'Free' for $0, 'Standard' for $9/mo (needed for custom auth)
param apiBaseUrl string              // URL of the API Container App for proxy config

resource swa 'Microsoft.Web/staticSites@2022-09-01' = {
  name: 'animstudio-${environment}-web'
  location: location
  sku: { name: skuName, tier: skuName }
  properties: {
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
    enterpriseGradeCdnStatus: 'Disabled'
  }
}

// App settings passed to Next.js at build/runtime
resource swaSettings 'Microsoft.Web/staticSites/config@2022-09-01' = {
  parent: swa
  name: 'appsettings'
  properties: {
    NEXT_PUBLIC_API_BASE_URL: apiBaseUrl
  }
}

output swaDefaultHostname string = swa.properties.defaultHostname
output swaName string = swa.name
output swaId string = swa.id
```

**`staticwebapp.config.json`** (add to `frontend/` directory — routes SignalR through SWA proxy):
```json
{
  "navigationFallback": { "rewrite": "/index.html", "exclude": ["/api/*", "/hubs/*", "/_next/*"] },
  "routes": [
    { "route": "/hubs/*", "methods": ["GET"], "rewrite": "https://animstudio-prod-api.<env>.azurecontainerapps.io/hubs/*" },
    { "route": "/api/*",  "methods": ["GET","POST","PUT","PATCH","DELETE"],
      "rewrite": "https://animstudio-prod-api.<env>.azurecontainerapps.io/api/*" }
  ],
  "globalHeaders": { "X-Frame-Options": "SAMEORIGIN" }
}
```
> Replace the rewrite hostnames with the actual API Container App URL per environment. Use SWA app settings (`NEXT_PUBLIC_API_BASE_URL`) so the Next.js client builds API URLs dynamically.

**Full `containerapp-worker.bicep`** content: see OPT-2 section above (verbatim Bicep block).

---

### 1.1 Parameterise Bicep

**Files to modify:** `infra/main.bicep` and all modules.

- Add `environment` parameter (`dev` | `staging` | `prod`) to `main.bicep`
- Derive all resource names: `animstudio-${environment}-api`, `animstudio${environment}stor`, etc.
- Create `infra/parameters/dev.bicepparam`, `staging.bicepparam`, `prod.bicepparam`
- **Do NOT include** `redisSkuName` / `redisCacheName` in new param files (Redis removed per OPT-3)
- **Do include** `swaSkuName` (`Free` for dev, `Standard` for prod), `swaRepoUrl`, `swaBranch`

### 1.2 Managed Identities

- Add `identity: { type: 'SystemAssigned' }` to API Container App and Python Worker Container App
- Grant API Container App MSI:
  - `Storage Blob Data Contributor` on the storage account
  - `Azure Service Bus Data Owner` on the Service Bus namespace
  - `Key Vault Secrets User` on Key Vault
  - `AcrPull` on ACR
- Grant Python Worker MSI: `Storage Blob Data Contributor` + `Azure Service Bus Data Owner`
- Remove all connection strings from app settings; replace with MSI-based `DefaultAzureCredential`

### 1.3 Blob Storage CDN

- Add `cdn.bicep` module: CDN profile (Standard Microsoft) with origin → Blob Storage `assets` container
- Configure CORS on storage account to allow AnimStudio domain and SWA hostname
- Enable soft-delete on containers (7-day retention)

### 1.4 Deployment Script

**New file:** `infra/deploy.ps1`

```powershell
param([string]$Environment = "dev")
az login --use-device-code
az deployment sub create `
  --location eastus `
  --template-file main.bicep `
  --parameters parameters/$Environment.bicepparam
```

### 1.5 Key Vault Secrets Bootstrap

**New file:** `infra/seed-keyvault.ps1`

Populates Key Vault with:
- `SqlConnectionString`, `HangfireSqlConnectionString`
- `ServiceBusConnectionString` (or use MSI — prefer MSI)
- `StripeSecretKey`, `StripeWebhookSecret`
- `AzureOpenAIKey`, `AzureOpenAIEndpoint`
- `FalApiKey` (cartoon_automation Kling/FAL proxy)
- `ElevenLabsApiKey` (voice cloning)
- `EntraExternalIdClientId`, `EntraExternalIdTenantId`

### 1.6 Fix AzureBlobFileStorageService

**File:** `backend/src/AnimStudio.API/Services/AzureBlobFileStorageService.cs`

- Use `BlobServiceClient` with `DefaultAzureCredential` (MSI in prod, env vars in dev)
- Implement `SaveFileAsync`: upload stream to `assets` container
- Implement `GetFileUrl`: generate SAS URL (30-day expiry for media, 1-hour for previews)
- Implement `ExistsAsync`: check blob metadata

---

## Track 2: Integration Bridge (cartoon_automation ↔ AnimStudio)

### 2.1 AnimStudio Side — Complete CompletionMessageProcessor

**File:** `backend/src/AnimStudio.API/Hosted/CompletionMessageProcessor.cs`

Current stub receives Service Bus messages but ignores them. Must:

```csharp
// Deserialize by JobType and dispatch
var message = JsonSerializer.Deserialize<JobCompletionMessage>(body);
await mediator.Send(new HandleJobCompletionCommand(message.JobId, message.Status, message.Result, message.ErrorMessage));
```

**Define `JobCompletionMessage` DTO** (shared contract):
```json
{
  "jobId": "guid",
  "episodeId": "guid",
  "jobType": "CharacterDesign|LoraTraining|Script|StoryboardPlan|StoryboardGen|Animation|PostProd",
  "status": "Completed|Failed",
  "result": { /* type-specific payload */ },
  "errorMessage": "string|null"
}
```

**File:** `ContentModule/Application/Commands/HandleJobCompletion/HandleJobCompletionCommand.cs`

Extend to route by `jobType`:
- `CharacterDesign` → update `Character.ImageUrl`
- `LoraTraining` → update `Character.LoraWeightsUrl`, `TriggerWord`, `TrainingStatus = Ready`
- `Script` → update `Script.RawJson`
- `StoryboardPlan` → update `Storyboard.RawJson`
- `StoryboardGen` → update `StoryboardShot.ImageUrl` per shot
- `Animation` → update `AnimationClip.ClipUrl` per clip, `Status = Ready`
- `PostProd` → update `Render.CdnUrl`, `CaptionsSrtUrl`, `DurationSeconds`, `Status = Complete`

### 2.2 AnimStudio Side — Dispatch Jobs via Service Bus

**File:** `backend/src/AnimStudio.API/Hosted/AnimationJobHangfireProcessor.cs`

The `ProcessKlingStubAsync` path must:
1. Build job payload (episodeId, clips to render, style, LoRA weights URL, character TriggerWord)
2. Publish `JobQueuedEvent` to Service Bus `jobs-queue` via `OutboxPublisher`
3. Set `AnimationJob.Status = Running`
4. Wait for completion via `CompletionMessageProcessor` (no polling — event-driven)

### 2.3 Shared Contract File

**New file:** `specs/job-message-schema.json`

Define the exact JSON schema for Service Bus messages in both directions:
- `jobs-queue` message format (AnimStudio → cartoon_automation)
- `completions-queue` message format (cartoon_automation → AnimStudio)
- Per-jobType result payload schemas

### 2.4 cartoon_automation Side — Service Bus Producer

**In `cartoon_automation` Python project**, add a new module:

**New file:** `cartoon_automation/src/cartoon_automation/services/animstudio_bridge.py`

```python
class AnimStudioBridge:
    def __init__(self, connection_string: str, jobs_queue: str, completions_queue: str):
        ...
    
    async def receive_job(self) -> JobMessage:
        """Poll jobs-queue for next job to process."""
    
    async def report_completion(self, job_id: str, job_type: str, result: dict):
        """Publish to completions-queue."""
    
    async def report_failure(self, job_id: str, error: str):
        """Publish failure to completions-queue."""
```

**New config file:** `cartoon_automation/config/azure.yaml`
```yaml
service_bus:
  connection_string: "${AZURE_SERVICE_BUS_CONNECTION_STRING}"
  jobs_queue: "jobs-queue"
  completions_queue: "completions-queue"

blob_storage:
  account_url: "https://{account}.blob.core.windows.net"
  container: "assets"
```

### 2.5 cartoon_automation Side — Blob Storage Uploader

**New file:** `cartoon_automation/src/cartoon_automation/services/blob_uploader.py`

```python
class BlobUploader:
    async def upload_file(self, local_path: str, blob_path: str) -> str:
        """Upload file to Azure Blob, return blob URL."""
    
    async def upload_bytes(self, data: bytes, blob_path: str, content_type: str) -> str:
        """Upload in-memory bytes to blob."""
```

All pipeline phase outputs (images, mp4s, audio) must go through `BlobUploader` before calling `report_completion`. The `result.imageUrl`, `result.clipUrl` etc. will then be Blob CDN URLs.

### 2.6 cartoon_automation Side — Phase Integration

Each phase function must be wrapped to:
1. Call `bridge.receive_job()` and parse job type
2. Execute existing phase logic (unchanged)
3. Upload output to Blob via `BlobUploader`
4. Call `bridge.report_completion(job_id, job_type, result_with_blob_urls)`

**Phases requiring integration:**
- Phase 4 (CharacterDesign): upload `image.png` → report `{ imageUrl }`
- Phase 4 (LoraTraining): upload `.safetensors` → report `{ loraWeightsUrl, triggerWord }`
- Phase 5 (Script): report `{ screenplay }` (no file upload needed)
- Phase 6 (StoryboardPlan): report `{ plan }` (no file upload)
- Phase 6 (StoryboardGen): upload each `shot_N.png` → report `{ shots: [{sceneNumber, shotIndex, imageUrl}] }`
- Phase 8 (Animation): upload each `scene_XX_shot_YY.mp4` → report `{ clips: [{..., clipUrl, durationSeconds}] }`
- Phase 9 (PostProd): upload `output.mp4` + `captions.srt` → report `{ videoUrl, srtUrl, durationSeconds }`

---

## Track 3: Backend Stubs → Real Implementations

### 3.1 VoicePreviewService

**File:** `backend/src/AnimStudio.API/Services/VoicePreviewService.cs`

- Use `Azure.AI.OpenAI` SDK to call TTS endpoint with character text + voice name
- Upload synthesized audio to Blob (`/previews/{episodeId}/{characterId}/{timestamp}.mp3`)
- Return SAS URL (1-hour expiry)
- Fall back to placeholder URL when `AzureOpenAIKey` not configured (keeps local dev working)

### 3.2 VoiceCloneService

**File:** `backend/src/AnimStudio.API/Services/VoiceCloneService.cs`

- Integrate ElevenLabs Voice Clone API (POST to `api.elevenlabs.io/v1/voices/add`)
- Upload reference audio file, receive `voice_id`
- Store `voice_id` in `VoiceAssignment.VoiceCloneUrl` field
- Gate behind `Studio` subscription tier check (already has `SubscriptionGate` middleware)

### 3.3 HandleJobCompletion Routing

See Track 2.1 above — type-dispatch is the key change.

### 3.4 AnimationController — Enable Kling Path

**File:** `backend/src/AnimStudio.API/Controllers/AnimationController.cs`

- When `body.Backend == "Kling"`, set `AnimationJob.Backend = Kling` and enqueue `AnimationJobHangfireProcessor` which now publishes to Service Bus (Track 2.2)
- When `body.Backend == "Local"`, existing scan path (unchanged)

---

## Track 4: Missing Backend Phases (11 & 12)

### 4.1 Phase 11 — Sharing & Review

**New entities** in `ContentModule/Domain/Entities/`:
- `ReviewLink.cs` — aggregate (Token[unique/indexed], EpisodeId, RenderId, ExpiresAt, IsRevoked, PasswordHash, CreatedByUserId)
- `ReviewComment.cs` — entity (ReviewLinkId, AuthorName, Text, TimestampSeconds, IsResolved)
- `BrandKit.cs` — aggregate (TeamId unique, LogoUrl, PrimaryColor, SecondaryColor, WatermarkPosition, WatermarkOpacity)
- `SocialPublish.cs` — entity (EpisodeId, RenderId, Platform, ExternalVideoId, Status, PublishedAt)

**New tables** (schema `content.*`):
- `content.ReviewLinks`, `content.ReviewComments`, `content.BrandKits`, `content.SocialPublishes`

**New EF configuration** in `ContentDbContext.cs`

**New repository interfaces + implementations**

**New commands/queries:**
- `CreateReviewLinkCommand` → generates unique token, sets expiry
- `GetReviewLinkQuery` → public (no auth), validates token, checks expiry + IsRevoked
- `AddReviewCommentCommand` → public
- `ResolveReviewCommentCommand` → requires creator auth
- `GetBrandKitQuery`, `UpsertBrandKitCommand`
- `StartYouTubePublishCommand` → OAuth redirect URL generation
- `CompleteYouTubePublishCommand` → OAuth callback, execute YouTube upload

**New controller:** `ReviewController.cs`
```
POST   /api/v1/renders/{id}/review-links           → 201 ReviewLinkDto
GET    /api/v1/review/{token}                       → RenderDto (public)
GET    /api/v1/review/{token}/comments              → ReviewCommentDto[] (public)
POST   /api/v1/review/{token}/comments              → 201 (public, no auth)
PATCH  /api/v1/review/{token}/comments/{id}/resolve → 204
GET    /api/v1/teams/{id}/brand-kit                 → BrandKitDto
PUT    /api/v1/teams/{id}/brand-kit                 → BrandKitDto
POST   /api/v1/renders/{id}/publish/youtube         → { authUrl }
GET    /api/v1/publish/youtube/callback             → redirect (OAuth)
```

### 4.2 Phase 12 — Analytics & Admin

**New entities** in `AnalyticsModule/Domain/Entities/`:
- `VideoView.cs` — (EpisodeId, RenderId, ViewerIpHash, ViewedAt, Source[Direct|Embed|Review])
- `Notification.cs` — (UserId, Type, Title, Body, IsRead, CreatedAt, RelatedEntityId, RelatedEntityType)

**New tables**: `analytics.VideoViews`, `analytics.Notifications`

**Wire view tracking:** `ReviewController.GetReview` increments view count on load

**Usage metering:** increment `Subscription.UsageEpisodesThisMonth` in `HandleJobCompletion` when `jobType = PostProd` and status = Completed

**New controllers:**
- `AnalyticsController.cs`:
  ```
  GET /api/v1/episodes/{id}/analytics  → EpisodeAnalyticsDto
  GET /api/v1/teams/{id}/analytics     → TeamAnalyticsDto
  ```
- `AdminController.cs` (admin role only):
  ```
  GET /api/v1/admin/stats   → AdminStatsDto
  GET /api/v1/admin/users   → paged AdminUserListDto
  GET /api/v1/admin/jobs    → AdminJobListDto
  ```
- `NotificationController.cs`:
  ```
  GET   /api/v1/notifications          → NotificationDto[]
  PATCH /api/v1/notifications/{id}/read → 204
  PATCH /api/v1/notifications/read-all  → 204
  ```

**Notification triggers** (register `INotificationHandler<T>` for):
- `EpisodeCompletedEvent` → notify episode creator
- `JobFailedEvent` → notify episode creator with error
- `SubscriptionUsageAlertEvent` (80% and 100% of quota) → notify team owner

---

## Track 5: FFmpeg Pipeline

### 5.1 FFmpeg Filter Graph Generator

**New file:** `backend/src/AnimStudio.ContentModule/Application/Services/FfmpegFilterGraphBuilder.cs`

Takes `Timeline` model (from Track DB) and builds FFmpeg filter string:

```
[0:v] trim=start=0:end=5.64, setpts=PTS-STARTPTS [v0]
[1:v] trim=start=0:end=5.64, setpts=PTS-STARTPTS [v1]
[v0][v1] concat=n=2:v=1:a=0 [vout]
[2:a] volume=0.8, atrim=start=0:end=5.64, asetpts=PTS-STARTPTS [a0]
[3:a] volume=0.3, atrim=start=0:end=47.2, asetpts=PTS-STARTPTS [a_music]
[a0][a_music] amix=inputs=2:duration=first:dropout_transition=2 [aout]
[vout][aout] concat=n=1:v=1:a=1 [final]
```

Supports:
- Clip sequencing with start/end trim
- Transitions (fade = xfade filter, dissolve = xfade:fade, slide = xfade:slideleft)
- Audio track mixing with volume
- Auto-duck music when dialogue plays (using `asidedata` filter)
- Text overlay (drawtext filter)
- Aspect ratio conversion (scale + pad for letterbox/pillarbox)

### 5.2 Real RenderHangfireProcessor

**File:** `backend/src/AnimStudio.API/Hosted/RenderHangfireProcessor.cs`

Replace local-file-fallback with:
1. Load timeline from DB (via `ITimelineRepository`)
2. Download all clip files from Blob to temp directory (or use Blob URLs directly if FFmpeg supports it)
3. Build FFmpeg command using `FfmpegFilterGraphBuilder`
4. Execute FFmpeg via `Process.Start`
5. Upload output MP4 to Blob: `/renders/{renderId}/output.mp4`
6. Generate SRT from screenplay dialogue timeline → upload to `/renders/{renderId}/captions.srt`
7. Call `render.MarkComplete(cdnUrl, srtUrl, durationSeconds)`
8. Emit `RenderCompleteEvent` → SignalR

### 5.3 SRT Generator

**New file:** `backend/src/AnimStudio.ContentModule/Application/Services/SrtGeneratorService.cs`

Reads `Script.RawJson` dialogue timeline (startTime, endTime per line) → formats as SRT:
```
1
00:00:00,000 --> 00:00:02,500
Did someone say laser pointer?
```

---

## Track 6: Frontend De-Mocking & CI/CD

### 6.1 Remove Active Mock Interceptors

**File:** `frontend/src/lib/mock-data/mock-interceptor.ts`

Remove these routes from the mock interceptor (connect to real backend):
- Projects CRUD
- Episodes CRUD
- Characters list
- `/api/v1/voices/preview` (connect to real TTS)
- `/api/v1/episodes/{id}/saga`

Keep mocks only for routes where backend phase is not yet implemented:
- Phase 11 routes (until Track 4 is done)
- Phase 12 routes (until Track 4 is done)

### 6.2 Phase 11 Frontend Pages

**New pages/components** (per Plan2.0 `phase-10-12-detailed.md` specs):
- `src/app/(dashboard)/studio/[id]/review/page.tsx` — review link management
- `src/app/review/[token]/page.tsx` — public review page (no auth)
- `src/components/review/review-link-generator.tsx`
- `src/components/review/comment-panel.tsx`
- `src/components/review/youtube-publish-dialog.tsx`
- `src/components/brand-kit/brand-kit-editor.tsx`
- `src/hooks/use-review.ts`, `use-brand-kit.ts`, `use-social-publish.ts`

### 6.3 Phase 12 Frontend Pages

**New pages/components:**
- `src/app/(dashboard)/analytics/page.tsx`
- `src/app/(dashboard)/admin/page.tsx` (admin role gate)
- `src/components/analytics/metric-card.tsx`, `views-chart.tsx`, `engagement-metrics.tsx`
- `src/components/notifications/notification-bell.tsx`, `notification-panel.tsx`
- `src/hooks/use-analytics.ts`, `use-notifications.ts`, `use-admin-stats.ts`

### 6.4 CI/CD Pipelines

**File:** `.github/workflows/infra.yml`
```yaml
on: push to main with changes in infra/**
steps:
  - az login (OIDC)
  - az deployment sub create --parameters parameters/prod.bicepparam
```

**File:** `.github/workflows/deploy-api.yml`
```yaml
on: push to main with changes in backend/**
steps:
  - dotnet build + test
  - docker build → push to ACR
  - az containerapp update --image <new-tag>
  - smoke test: curl /api/v1/health
```

**File:** `.github/workflows/deploy-web.yml` — **updated for SWA (OPT-1)**
```yaml
on: push to main with changes in frontend/**
steps:
  - pnpm install + build (pnpm build)
  - Azure/static-web-apps-deploy@v1
      with:
        azure_static_web_apps_api_token: ${{ secrets.SWA_DEPLOY_TOKEN }}
        repo_token: ${{ secrets.GITHUB_TOKEN }}
        action: upload
        app_location: frontend
        output_location: .next
  - smoke test: curl https://<swa-hostname>
```
> Note: No `docker build`, no ACR push, no `az containerapp update` for the frontend. The `Azure/static-web-apps-deploy@v1` action handles build + deploy natively.

---

## Execution Order & Dependencies

```
Week 1-2:  Track 1 (Azure Infra) — runs first, unblocks everything
Week 2-3:  Track 3.1-3.2 (VoicePreview + VoiceClone stubs → real)
Week 2-4:  Track 2.1-2.3 (CompletionMessageProcessor + shared contract)
Week 3-4:  Track 2.4-2.6 (cartoon_automation bridge + Blob uploader)
Week 3-5:  Track 4 (Phase 11 + 12 backend)
Week 4-6:  Track 5 (FFmpeg pipeline)
Week 5-7:  Track 6.1-6.3 (Frontend de-mock + Phase 11/12 pages)
Week 6-8:  Track 6.4 (CI/CD) + end-to-end integration testing
```

---

## Critical Files to Modify

### Infrastructure (Bicep) — includes cost optimisations
| File | Change |
|---|---|
| `infra/main.bicep` | Parameterise env names; add `staticwebapp` + `containerapp-worker` modules; comment out `redis` module; add MSI role assignments |
| `infra/modules/staticwebapp.bicep` | **New** — Azure Static Web Apps for Next.js frontend (OPT-1) |
| `infra/modules/containerapp-worker.bicep` | **New** — Python worker Container App with KEDA scale-to-zero (OPT-2) |
| `infra/modules/redis.bicep` | **Keep but do not call from main.bicep** — re-enable when API scales to 2+ replicas (OPT-3) |
| `infra/modules/keyvault.bicep` | Remove `RedisConnectionString` secret; add SWA deploy token secret |
| `infra/modules/containerapp.bicep` | Add `identity: SystemAssigned`; add MSI-based env var injection |
| `infra/modules/cdn.bicep` | **New** — CDN profile over Blob `assets` container |
| `infra/parameters/dev.json` | Remove Redis params; add `swaSkuName: Free`, `swaRepoUrl`, `swaBranch` |
| `infra/parameters/prod.json` | Remove Redis params; add `swaSkuName: Standard`, `swaRepoUrl`, `swaBranch` |
| `infra/deploy.ps1` | **New** — deployment entrypoint |
| `infra/seed-keyvault.ps1` | **New** — one-time secrets bootstrap |
| `frontend/staticwebapp.config.json` | **New** — SWA routing rules: proxy `/hubs/*` and `/api/*` to API Container App |

### Backend (.NET)
| File | Change |
|---|---|
| `API/Services/AzureBlobFileStorageService.cs` | Implement SaveFileAsync + SAS URLs |
| `API/Services/VoicePreviewService.cs` | Real Azure OpenAI TTS |
| `API/Services/VoiceCloneService.cs` | ElevenLabs integration |
| `API/Hosted/CompletionMessageProcessor.cs` | Route by jobType to HandleJobCompletion |
| `API/Hosted/AnimationJobHangfireProcessor.cs` | Publish to Service Bus for Kling |
| `API/Hosted/RenderHangfireProcessor.cs` | Real FFmpeg execution + Blob upload |
| `ContentModule/Application/Commands/HandleJobCompletion/HandleJobCompletionCommand.cs` | Per-jobType result dispatch |
| `ContentModule/Application/Services/FfmpegFilterGraphBuilder.cs` | New — FFmpeg filter graph |
| `ContentModule/Application/Services/SrtGeneratorService.cs` | New — SRT from screenplay |
| `ContentModule/Domain/Entities/{ReviewLink,ReviewComment,BrandKit,SocialPublish}.cs` | New Phase 11 entities |
| `AnalyticsModule/Domain/Entities/{VideoView,Notification}.cs` | New Phase 12 entities |
| `ContentModule/Infrastructure/Persistence/ContentDbContext.cs` | New Phase 11 DbSets + config |
| `API/Controllers/{ReviewController,AnalyticsController,AdminController,NotificationController}.cs` | New controllers |
| `specs/job-message-schema.json` | New shared contract |
| **`Program.cs`** | **No change needed** — in-memory fallback already active when Redis string absent (OPT-3) |
| `API/Services/AzureBlobFileStorageService.cs` | Implement SaveFileAsync + SAS URLs |
| `API/Services/VoicePreviewService.cs` | Real Azure OpenAI TTS |
| `API/Services/VoiceCloneService.cs` | ElevenLabs integration |
| `API/Hosted/CompletionMessageProcessor.cs` | Route by jobType to HandleJobCompletion |
| `API/Hosted/AnimationJobHangfireProcessor.cs` | Publish to Service Bus for Kling |
| `API/Hosted/RenderHangfireProcessor.cs` | Real FFmpeg execution + Blob upload |
| `ContentModule/Application/Commands/HandleJobCompletion/HandleJobCompletionCommand.cs` | Per-jobType result dispatch |
| `ContentModule/Application/Services/FfmpegFilterGraphBuilder.cs` | New — FFmpeg filter graph |
| `ContentModule/Application/Services/SrtGeneratorService.cs` | New — SRT from screenplay |
| `ContentModule/Domain/Entities/{ReviewLink,ReviewComment,BrandKit,SocialPublish}.cs` | New Phase 11 entities |
| `AnalyticsModule/Domain/Entities/{VideoView,Notification}.cs` | New Phase 12 entities |
| `ContentModule/Infrastructure/Persistence/ContentDbContext.cs` | New Phase 11 DbSets + config |
| `API/Controllers/{ReviewController,AnalyticsController,AdminController,NotificationController}.cs` | New controllers |
| `specs/job-message-schema.json` | New shared contract |

### cartoon_automation (Python)
| File | Change |
|---|---|
| `config/azure.yaml` | New Azure config (Service Bus + Blob) |
| `services/animstudio_bridge.py` | New Service Bus producer/consumer |
| `services/blob_uploader.py` | New Blob Storage uploader |
| Each phase runner (`phase4.py`, `phase6.py`, etc.) | Wrap with bridge.receive_job() / report_completion() |
| `requirements.txt` | Add `azure-servicebus`, `azure-storage-blob`, `azure-identity` |

### Frontend
| File | Change |
|---|---|
| `src/lib/mock-data/mock-interceptor.ts` | Remove projects/episodes/characters/saga mocks |
| `src/app/(dashboard)/studio/[id]/review/page.tsx` | New Phase 11 page |
| `src/app/review/[token]/page.tsx` | New public review page |
| `src/app/(dashboard)/analytics/page.tsx` | New Phase 12 page |
| `src/app/(dashboard)/admin/page.tsx` | New admin page |
| `src/hooks/use-review.ts` | New Phase 11 hooks |
| `src/hooks/use-analytics.ts` | New Phase 12 hooks |
| `src/hooks/use-notifications.ts` | Notification bell/panel |

---

## Verification / End-to-End Test

1. **Infrastructure**: `az deployment sub create --what-if` runs clean; then deploy to `dev` environment, verify all resources provisioned and MSI roles assigned.

2. **Blob Storage**: Run `AzureBlobFileStorageService.SaveFileAsync(stream, "test/file.png", "image/png")` in integration test → verify blob appears in Azure portal and SAS URL is accessible.

3. **Service Bus round-trip**: Post a synthetic `JobQueuedEvent` to `jobs-queue` via Service Bus Explorer → cartoon_automation worker receives it → uploads to Blob → posts to `completions-queue` → AnimStudio `CompletionMessageProcessor` processes it → check DB for updated Episode/Character/Storyboard state.

4. **Full episode pipeline**: Create episode in AnimStudio UI → dispatch CharacterDesign job → watch SignalR progress updates in browser → verify each phase completes and DB is updated → confirm final render appears in Timeline Editor and preview player.

5. **Phase 11**: Create review link → open `/review/{token}` in incognito → add timestamped comment → verify creator sees comment in dashboard.

6. **Phase 12**: Verify `VideoView` row created on review link load. Check `/api/v1/episodes/{id}/analytics` returns view counts.

7. **CI/CD**: Push code change to `main` → GitHub Action builds, tests, pushes image to ACR, deploys to Container App, runs smoke test — all green.
