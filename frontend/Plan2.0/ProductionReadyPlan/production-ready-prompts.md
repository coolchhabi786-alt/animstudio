# AnimStudio — Production Readiness: Step-by-Step Claude Prompts

**Purpose:** Each section below is a self-contained prompt ready to paste into a new Claude Code session.  
**Coverage:** Azure Infra (Bicep) → Backend (.NET) → Python (cartoon_automation) → Frontend (Next.js) → CI/CD  
**Status as of 2026-05-02:** Phases 1–9 backend complete, Phases 10–12 not started on backend; infra Bicep written but never deployed; cartoon_automation isolated on local disk; several frontend pages still using mock interceptors.

---

## EXECUTION SEQUENCE (dependency order)

```
WAVE 1 — Infrastructure (blocks everything)
  INFRA-1: Deploy core Azure resources (Bicep)
  INFRA-2: Managed Identity role assignments
  INFRA-3: Key Vault secrets bootstrap

WAVE 2 — Backend stubs → real (parallel after INFRA-1)
  BACKEND-1: AzureBlobFileStorageService (needs Blob account from INFRA-1)
  BACKEND-2: CompletionMessageProcessor + HandleJobCompletion routing
  BACKEND-3: AnimationJobHangfireProcessor → Service Bus dispatch
  BACKEND-4: VoicePreviewService (Azure OpenAI TTS) + VoiceCloneService (ElevenLabs)

WAVE 3 — Missing backend phases (parallel after WAVE 2)
  BACKEND-5: Phase 11 — ReviewLink + ReviewComment + BrandKit + ReviewController
  BACKEND-6: Phase 12 — Analytics, Admin, Notifications
  BACKEND-7: FFmpeg render pipeline (FfmpegFilterGraphBuilder + RenderHangfireProcessor)

WAVE 4 — Python integration bridge (after INFRA-1 + BACKEND-2)
  PYTHON-1: animstudio_bridge.py (Service Bus producer/consumer)
  PYTHON-2: blob_uploader.py + azure config
  PYTHON-3: Wrap each pipeline phase to use bridge + blob

WAVE 5 — Frontend (after WAVE 2 + WAVE 3)
  FRONTEND-1: De-mock existing phases (projects, episodes, characters, saga, voices)
  FRONTEND-2: Phase 11 UI — review links, public review page, brand kit
  FRONTEND-3: Phase 12 UI — analytics dashboard, admin, notifications

WAVE 6 — CI/CD (after all above)
  CICD-1: GitHub Actions for infra, API docker build, SWA web deploy
  CICD-2: Smoke tests + end-to-end integration validation
```

---

---

# WAVE 1: AZURE INFRASTRUCTURE

---

## INFRA-1 — Deploy Core Azure Resources

**Paste this entire prompt into a new Claude Code session. Attach no files — Claude reads from disk.**

```
You are an Azure Bicep expert working on the AnimStudio SaaS platform.

WORKING DIRECTORY: C:\Projects\animstudio\infra

CONTEXT:
- AnimStudio is a .NET 8 + Next.js 14 SaaS platform for AI cartoon production.
- Azure Bicep modules are written but have NEVER been deployed.
- All modules live in infra/modules/. The orchestrator is infra/main.bicep.
- Deployment targets: dev and prod resource groups in Azure East US.
- Three cost optimisations are already baked into main.bicep:
  OPT-1: Frontend on Azure Static Web Apps (not a Container App)
  OPT-2: Python worker with KEDA scale-to-zero on Service Bus queue depth
  OPT-3: Redis removed for launch (Program.cs already falls back to IMemoryCache)

WHAT HAS BEEN DONE (do NOT redo):
- infra/main.bicep: complete, parameterised, references all modules
- infra/modules/staticwebapp.bicep: provisions Microsoft.Web/staticSites
- infra/modules/containerapp-worker.bicep: Python worker with KEDA rules
- infra/modules/containerapp.bicep: API container app with SystemAssigned MSI
- infra/modules/containerapp-env.bicep: Container App Environment + VNet
- infra/modules/servicebus.bicep: Standard tier, 4 queues
- infra/modules/storage.bicep: Blob storage with soft-delete
- infra/modules/keyvault.bicep: Key Vault with RBAC + inline role assignments
- infra/modules/sql.bicep, hangfire-sql.bicep, signalr.bicep, acr.bicep
- infra/parameters/dev.json and prod.json: environment parameter files
- Redis module exists but is COMMENTED OUT in main.bicep (intentional)
- frontdoor.bicep: conditional (prod only), routes SWA + API through Front Door

WHAT IS MISSING (your job):
1. Create infra/deploy.ps1 — PowerShell deployment script
2. Create infra/modules/cdn.bicep — CDN profile over Blob Storage assets container
3. Add cdn.bicep call to main.bicep
4. Verify all module param names match the calls in main.bicep (read both files)
5. Create a dry-run command the user can run to validate before real deploy

SPECIFIC REQUIREMENTS:

infra/deploy.ps1:
- Parameters: -Environment (dev|prod), -ResourceGroup, -Location (default: eastus)
- Steps: az login → az group create (if not exists) → az deployment group what-if → prompt user → az deployment group create
- Use --parameters @parameters/$Environment.json
- Print output URLs (apiUrl, swaUrl, swaName) at end
- Fail fast with clear error messages (set -e equivalent in PowerShell: $ErrorActionPreference = "Stop")

infra/modules/cdn.bicep (new file):
- Resource: Microsoft.Cdn/profiles (Standard_Microsoft tier)
- Endpoint: Points to Blob Storage account's blob endpoint
- Origin: animstudio${environment}stor.blob.core.windows.net
- Cache rules: Cache images + videos for 7 days, bust on content hash
- CORS: Allow all origins for assets (public media files)
- Output: cdnEndpointUrl string

main.bicep additions:
- Add module cdnModule './modules/cdn.bicep' = { ... }
- Pass environment and storageModule.outputs.blobEndpoint
- Add output cdnUrl string to outputs section

VALIDATION:
After creating files, run:
  az bicep build --file infra/main.bicep
to confirm no Bicep compilation errors. Report any errors found.

FILES TO READ FIRST (before writing anything):
- infra/main.bicep
- infra/modules/containerapp.bicep
- infra/modules/keyvault.bicep
- infra/modules/storage.bicep
- infra/parameters/dev.json

DELIVERABLES:
1. infra/deploy.ps1 (complete PowerShell script)
2. infra/modules/cdn.bicep (new module)
3. Edit infra/main.bicep to add cdn module call + cdnUrl output
4. Report: list any param mismatches found between main.bicep calls and module param declarations
5. Also provide instructions how to run this deploy.ps1 created if not already deployed the resources
```

---

## INFRA-2 — Managed Identity Role Assignments

```
You are an Azure Bicep expert working on AnimStudio infrastructure.

WORKING DIRECTORY: C:\Projects\animstudio\infra

CONTEXT:
- The API Container App (animstudio-${environment}-api) and Python Worker (animstudio-${environment}-worker)
  both have SystemAssigned Managed Identity already set in their Bicep definitions.
- Key Vault already has inline role assignments for both MSIs (Key Vault Secrets User role).
- MISSING: Role assignments on Storage and Service Bus for the MSIs.

YOUR TASK:
Add Managed Identity role assignments so both Container Apps can access resources
WITHOUT connection strings in environment variables.

ROLE ASSIGNMENTS NEEDED:

For API Container App MSI (principalId from apiContainerApp.outputs.principalId):
1. Storage Blob Data Contributor on the storage account
   Role definition ID: ba92f5b4-2d11-453d-a403-e96b0029c9fe
2. Azure Service Bus Data Owner on the Service Bus namespace
   Role definition ID: 090c5cfd-751d-490a-894a-3ce6f1109419
3. AcrPull on the Container Registry
   Role definition ID: 7f951dda-4ed3-4680-a7ca-43fe172d538d

For Python Worker MSI (principalId from workerContainerApp.outputs.workerPrincipalId):
1. Storage Blob Data Contributor on the storage account (same role ID as above)
2. Azure Service Bus Data Owner on the Service Bus namespace (same role ID as above)
3. AcrPull on the Container Registry (same role ID as above)

IMPLEMENTATION APPROACH:
- Add these as inline Microsoft.Authorization/roleAssignments resources in infra/main.bicep
- Use guid() function to generate deterministic role assignment names:
  guid(resourceGroup().id, principalId, roleDefinitionId)
- Scope each assignment to the specific resource (storage account, service bus namespace, ACR)
- All role assignments must depend on the respective module deployments

PATTERN TO FOLLOW (already done in keyvault.bicep for reference):
resource apiKvRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, apiContainerApp.outputs.principalId, kvSecretsUserId)
  scope: keyVault  // scope to the specific resource
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', kvSecretsUserId)
    principalId: apiContainerApp.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}

IMPORTANT: For resources in modules, use the module's output to get the resource ID for scoping.
You may need to add output resourceId strings from storage.bicep, servicebus.bicep, and acr.bicep
if they don't already exist.

FILES TO READ FIRST:
- infra/main.bicep (see current structure)
- infra/modules/storage.bicep (check if storageAccountId output exists)
- infra/modules/servicebus.bicep (check if namespaceId output exists)
- infra/modules/acr.bicep (check if registryId output exists)
- infra/modules/keyvault.bicep (copy the role assignment pattern used here)

DELIVERABLES:
1. Edit infra/modules/storage.bicep: add output storageAccountId string
2. Edit infra/modules/servicebus.bicep: add output namespaceId string
3. Edit infra/modules/acr.bicep: add output registryId string
4. Edit infra/main.bicep: add 6 role assignment resources (2 principals × 3 resources)
5. Report: confirm no circular dependency issues
```

---

## INFRA-3 — Key Vault Secrets Bootstrap

```
You are a PowerShell + Azure CLI expert working on AnimStudio infrastructure.

WORKING DIRECTORY: C:\Projects\animstudio\infra

CONTEXT:
- AnimStudio uses Azure Key Vault for all production secrets.
- The Key Vault is named animstudio-${environment}-kv (dev or prod).
- Key Vault uses RBAC authorization (enableRbacAuthorization: true).
- The developer running this script needs Key Vault Secrets Officer role on the vault.
- Program.cs reads secrets by name via Azure.Extensions.AspNetCore.Configuration.Secrets.

SECRETS REQUIRED (from infra/modules/keyvault.bicep secretNames array):
1. SqlConnectionString
2. HangfireSqlConnectionString
3. StripeSecretKey
4. StripeWebhookSecret
5. ServiceBusConnectionString
6. SignalRConnectionString
7. AzureOpenAIKey
8. AzureOpenAIEndpoint
9. FalApiKey
10. ElevenLabsApiKey
11. AzureCommunicationServicesConnectionString

YOUR TASK:
Create infra/seed-keyvault.ps1 that:

1. Accepts parameters: -Environment (dev|prod), -KeyVaultName (optional override)
2. Prompts the user interactively for each secret value (Read-Host -MaskInput for secrets)
3. Skips secrets already set in Key Vault (check with az keyvault secret show)
4. Uploads each provided value with az keyvault secret set
5. Prints a summary of which secrets were set vs skipped vs left blank
6. Includes a --dry-run flag that lists what would be set without actually setting anything

ALSO CREATE infra/seed-keyvault-dev.ps1 (for local/CI dev environment):
- Reads from a local .env.secrets file (gitignored) 
- Populates Key Vault non-interactively from that file
- Format: SECRET_NAME=value per line

ALSO CREATE infra/.env.secrets.template:
- Template file showing all required secret names with placeholder values
- Include comments explaining where to get each value
- This file IS committed (it has no real values)

IMPORTANT NOTES:
- Never log or print secret values to console
- Use $ErrorActionPreference = "Stop"
- Use az keyvault secret set --vault-name $kvName --name $secretName --value $secretValue
- SqlConnectionString format: Server=animstudio-${env}-sql.database.windows.net;Database=AnimStudio;Authentication=Active Directory Default
  (Use Managed Identity auth for SQL in prod — no password in connection string)

DELIVERABLES:
1. infra/seed-keyvault.ps1
2. infra/seed-keyvault-dev.ps1
3. infra/.env.secrets.template
4. Edit infra/.gitignore (create if missing) to exclude .env.secrets
```

---

---

# WAVE 2: BACKEND STUBS → REAL

---

## BACKEND-1 — AzureBlobFileStorageService

```
You are a senior .NET 8 developer working on the AnimStudio SaaS platform.

WORKING DIRECTORY: C:\Projects\animstudio\backend

CONTEXT:
- AnimStudio uses IFileStorageService abstraction for all file operations.
- In local dev: LocalFileStorageService serves files from C:\Users\Vaibhav\cartoon_automation\output
- In production: AzureBlobFileStorageService must be used.
- AzureBlobFileStorageService currently THROWS NotSupportedException on SaveFileAsync.
- The service must use DefaultAzureCredential (Managed Identity in prod, env vars in dev).
- Storage account name: animstudio${environment}stor
- Container names: "assets" (for media files), "previews" (for TTS previews)

READ THESE FILES FIRST:
- backend/src/AnimStudio.API/Services/AzureBlobFileStorageService.cs (current stub)
- backend/src/AnimStudio.API/Services/LocalFileStorageService.cs (working reference)
- backend/src/AnimStudio.SharedKernel/Interfaces/IFileStorageService.cs (the interface)
- backend/src/AnimStudio.API/Program.cs (lines 100-160, see how services are registered)

YOUR TASK:
Implement AzureBlobFileStorageService with these methods:

1. SaveFileAsync(Stream content, string blobPath, string contentType) → string (blob URL)
   - Upload to "assets" container
   - Set content type and cache-control headers
   - Return the CDN URL (not the raw blob URL) using the CDN endpoint prefix from config

2. GetFileUrl(string blobPath, TimeSpan? expiry = null) → string
   - If expiry is null: return CDN URL (for public media files)
   - If expiry is set: generate SAS URL with UserDelegationKey (MSI-compatible SAS)
   - 30-day expiry for rendered videos, 1-hour for TTS previews

3. SavePreviewAsync(Stream content, string previewPath, string contentType) → string
   - Same as SaveFileAsync but uploads to "previews" container
   - Returns 1-hour SAS URL

4. ExistsAsync(string blobPath) → bool
   - Check blob exists in "assets" container

5. DeleteAsync(string blobPath) → void
   - Soft delete (set metadata DeletedAt, then hard delete after 7 days via lifecycle)

CONFIGURATION (read from IConfiguration / Key Vault):
- BlobStorage:AccountName — the storage account name
- BlobStorage:CdnEndpoint — CDN URL prefix (e.g. https://animstudio-dev-cdn.azureedge.net)
- Use DefaultAzureCredential() — works with both MSI (prod) and az login (dev)

DEPENDENCIES TO ADD (if not already in AnimStudio.API.csproj):
- Azure.Storage.Blobs
- Azure.Identity
- Azure.Storage.Blobs.Models

IMPORTANT: The existing interface may need new method signatures. If you add overloads,
update IFileStorageService in SharedKernel AND update LocalFileStorageService to implement them
(LocalFileStorageService can throw NotSupportedException for SAS URL generation — that's fine).

DELIVERABLES:
1. Edit backend/src/AnimStudio.API/Services/AzureBlobFileStorageService.cs (full implementation)
2. Edit backend/src/AnimStudio.SharedKernel/Interfaces/IFileStorageService.cs (add new methods if needed)
3. Edit backend/src/AnimStudio.API/Services/LocalFileStorageService.cs (implement new interface methods)
4. Edit backend/src/AnimStudio.API/Program.cs if registration needs updating
5. Report: exact NuGet packages added and their versions
```

---

## BACKEND-2 — CompletionMessageProcessor + HandleJobCompletion Routing

```
You are a senior .NET 8 developer working on the AnimStudio SaaS platform.
Backend uses Clean Architecture: Domain → Application → Infrastructure → API.
CQRS via MediatR. All commands return Result<T>.

WORKING DIRECTORY: C:\Projects\animstudio\backend

READ THESE FILES FIRST:
- backend/src/AnimStudio.API/Hosted/CompletionMessageProcessor.cs (current stub)
- backend/src/AnimStudio.ContentModule/Application/Commands/HandleJobCompletion/ (all files in folder)
- backend/src/AnimStudio.ContentModule/Domain/Entities/ (read Character.cs, Episode.cs, Storyboard.cs, AnimationClip.cs)
- backend/src/AnimStudio.ContentModule/Application/DTOs/ (read relevant DTOs)
- backend/src/AnimStudio.SharedKernel/ (read AggregateRoot, Result, domain events pattern)

CONTEXT:
- CompletionMessageProcessor is an IHostedService that polls Azure Service Bus "completions-queue".
- Currently it receives messages and LOGS them but does NOT process them.
- The Python cartoon_automation pipeline will publish to this queue when jobs complete.
- Messages follow this JSON schema (the shared contract):

{
  "jobId": "guid",
  "episodeId": "guid",
  "jobType": "CharacterDesign|LoraTraining|Script|StoryboardPlan|StoryboardGen|Animation|PostProd",
  "status": "Completed|Failed",
  "result": { /* type-specific, see below */ },
  "errorMessage": "string|null",
  "completedAt": "ISO8601"
}

Result payloads per jobType:
- CharacterDesign:  { "imageUrl": "string" }
- LoraTraining:     { "loraWeightsUrl": "string", "triggerWord": "string" }
- Script:           { "screenplay": { /* screenplay JSON object */ } }
- StoryboardPlan:   { "plan": { /* plan JSON object */ } }
- StoryboardGen:    { "shots": [{ "sceneNumber": int, "shotIndex": int, "imageUrl": "string" }] }
- Animation:        { "clips": [{ "sceneNumber": int, "shotIndex": int, "clipUrl": "string", "durationSeconds": float }] }
- PostProd:         { "videoUrl": "string", "srtUrl": "string", "durationSeconds": float }

YOUR TASK — PART 1: Define the shared contract DTO

Create: backend/src/AnimStudio.ContentModule/Application/DTOs/JobCompletionMessageDto.cs
- JobCompletionMessageDto with all fields
- JobType enum
- JobStatus enum  
- Per-type result record classes (CharacterDesignResult, LoraTrainingResult, etc.)
- Use System.Text.Json with [JsonPropertyName] attributes

Also create: specs/job-message-schema.json (in C:\Projects\animstudio\specs\)
- Full JSON Schema draft-07 document describing the message format
- Include all jobType variants with their result schemas

YOUR TASK — PART 2: Implement CompletionMessageProcessor

Edit: backend/src/AnimStudio.API/Hosted/CompletionMessageProcessor.cs
- Deserialize the Service Bus message body to JobCompletionMessageDto
- Dispatch to IMediator.Send(new HandleJobCompletionCommand(...))
- Complete message on success, dead-letter on persistent failure (after 3 retries)
- Log structured data (episodeId, jobType, status, duration) at Information level
- Never throw unhandled exceptions — catch, log, dead-letter

YOUR TASK — PART 3: Implement HandleJobCompletion handler with type dispatch

Edit: backend/src/AnimStudio.ContentModule/Application/Commands/HandleJobCompletion/HandleJobCompletionCommandHandler.cs

The handler must switch on jobType and call the appropriate private method:
- CharacterDesign → find Character by episodeId + sceneNumber, set ImageUrl, emit CharacterUpdatedEvent
- LoraTraining → find Character, set LoraWeightsUrl + TriggerWord + TrainingStatus = Ready
- Script → find Episode's Script, update RawJson
- StoryboardPlan → find Episode's Storyboard, update PlanJson
- StoryboardGen → find each StoryboardShot by sceneNumber+shotIndex, set ImageUrl
- Animation → find each AnimationClip by sceneNumber+shotIndex, set ClipUrl + DurationSeconds + Status = Ready
- PostProd → find Render by episodeId (latest), set CdnUrl + CaptionsSrtUrl + DurationSeconds + Status = Complete
  → emit RenderCompleteEvent → SignalR notification to episode creator

RULES:
- All DB writes via repositories (never call DbContext directly in handlers)
- Emit domain events using entity.AddDomainEvent()
- BOLA check: verify entity belongs to correct team before updating
- If entity not found: log Warning and return Result.Failure (do NOT throw)

DELIVERABLES:
1. NEW: backend/src/AnimStudio.ContentModule/Application/DTOs/JobCompletionMessageDto.cs
2. NEW: specs/job-message-schema.json
3. EDIT: backend/src/AnimStudio.API/Hosted/CompletionMessageProcessor.cs
4. EDIT: backend/src/AnimStudio.ContentModule/Application/Commands/HandleJobCompletion/HandleJobCompletionCommandHandler.cs
5. Report: list all domain events emitted and confirm they have SignalR handlers registered
```

---

## BACKEND-3 — AnimationJobHangfireProcessor → Service Bus Dispatch

```
You are a senior .NET 8 developer working on AnimStudio.

WORKING DIRECTORY: C:\Projects\animstudio\backend

READ THESE FILES FIRST:
- backend/src/AnimStudio.API/Hosted/AnimationJobHangfireProcessor.cs (full file)
- backend/src/AnimStudio.ContentModule/Domain/Entities/AnimationJob.cs
- backend/src/AnimStudio.ContentModule/Domain/Entities/AnimationClip.cs
- backend/src/AnimStudio.ContentModule/Domain/Entities/Episode.cs
- backend/src/AnimStudio.ContentModule/Domain/Entities/Character.cs
- backend/src/AnimStudio.API/Services/ (look for IServiceBusPublisher or OutboxPublisher)
- backend/src/AnimStudio.API/Controllers/AnimationController.cs

CONTEXT:
- AnimationJobHangfireProcessor has a "Kling" path that currently just logs a warning.
- In production, "Kling" means: publish a job message to Azure Service Bus "jobs-queue"
  so the Python cartoon_automation worker picks it up and runs the animation pipeline.
- The Python worker will later publish to "completions-queue" when done.
- This is fire-and-forget: the Hangfire job publishes the message then exits.
  Results come back asynchronously via CompletionMessageProcessor.

JOB MESSAGE FORMAT sent to jobs-queue (animstudio → python):
{
  "jobId": "guid",
  "episodeId": "guid",
  "jobType": "Animation",
  "requestedAt": "ISO8601",
  "payload": {
    "clips": [
      {
        "sceneNumber": 1,
        "shotIndex": 0,
        "storyboardShotId": "guid",
        "storyboardImageUrl": "string",
        "style": "string",
        "durationSeconds": 5,
        "characterTriggerWords": ["girl_lora_v1", "dog_lora_v2"]
      }
    ],
    "episodeStyle": "cartoon",
    "loraWeightsUrls": { "characterId": "blobUrl" }
  }
}

YOUR TASK:
1. Create interface IServiceBusPublisher (if it doesn't exist):
   File: backend/src/AnimStudio.SharedKernel/Interfaces/IServiceBusPublisher.cs
   Method: Task PublishAsync<T>(string queueName, T message, CancellationToken ct = default)

2. Create AzureServiceBusPublisher implementation:
   File: backend/src/AnimStudio.API/Services/AzureServiceBusPublisher.cs
   - Use ServiceBusClient with DefaultAzureCredential
   - Serialize message to JSON with System.Text.Json
   - Set message ContentType = "application/json"
   - Set message SessionId = episodeId.ToString() (for ordered processing)

3. Create local dev stub NoOpServiceBusPublisher:
   File: backend/src/AnimStudio.API/Services/NoOpServiceBusPublisher.cs
   - Logs the message that would be sent
   - Registered when ServiceBus:ConnectionString is not configured

4. Edit AnimationJobHangfireProcessor.cs:
   - In the Kling path (ProcessKlingStubAsync), replace the stub with:
     a. Load episode + all storyboard shots + character LoRA URLs from DB
     b. Build the AnimationJobMessage payload
     c. Call _serviceBusPublisher.PublishAsync("jobs-queue", message)
     d. Update AnimationJob.Status = Running in DB
   - Keep the Local path (ProcessLocalAsync) exactly as it is

5. Edit Program.cs:
   - Register AzureServiceBusPublisher when ServiceBus:Namespace config is present
   - Register NoOpServiceBusPublisher otherwise

6. Add config key to appsettings.Development.json:
   "ServiceBus": { "Namespace": "" }  (empty = NoOp in dev)

DELIVERABLES:
1. NEW: backend/src/AnimStudio.SharedKernel/Interfaces/IServiceBusPublisher.cs
2. NEW: backend/src/AnimStudio.API/Services/AzureServiceBusPublisher.cs
3. NEW: backend/src/AnimStudio.API/Services/NoOpServiceBusPublisher.cs
4. EDIT: backend/src/AnimStudio.API/Hosted/AnimationJobHangfireProcessor.cs
5. EDIT: backend/src/AnimStudio.API/Program.cs (registration)
6. EDIT: backend/src/AnimStudio.API/appsettings.Development.json
```

---

## BACKEND-4 — VoicePreviewService + VoiceCloneService

```
You are a senior .NET 8 developer working on AnimStudio.

WORKING DIRECTORY: C:\Projects\animstudio\backend

READ THESE FILES FIRST:
- backend/src/AnimStudio.API/Services/VoicePreviewService.cs (current stub)
- backend/src/AnimStudio.API/Services/VoiceCloneService.cs (current stub)
- backend/src/AnimStudio.API/Controllers/VoiceController.cs
- backend/src/AnimStudio.ContentModule/Domain/Entities/VoiceAssignment.cs
- backend/src/AnimStudio.API/Program.cs (lines around voice service registration)

TASK 1 — VoicePreviewService (Azure OpenAI TTS):

Replace the placeholder MP3 URL stub with real Azure OpenAI TTS synthesis:

Implementation:
- Use Azure.AI.OpenAI SDK
- Endpoint: config["AzureOpenAI:Endpoint"]
- Key: from Key Vault secret "AzureOpenAIKey"
- Model: "tts-1" (standard quality)
- Call: client.GenerateSpeechAsync with voice name + text + output format mp3
- Upload synthesized audio stream to Blob Storage:
  path: previews/{episodeId}/{characterId}/{timestamp}.mp3
  Use IFileStorageService.SavePreviewAsync()
- Return 1-hour SAS URL

Fallback for local dev (when AzureOpenAI:Endpoint is empty):
- Return a hardcoded public MP3 URL for testing
- Log a Warning that TTS is in fallback mode

TASK 2 — VoiceCloneService (ElevenLabs):

Replace the NotAvailable stub with ElevenLabs Voice Clone API:

Implementation:
- POST to https://api.elevenlabs.io/v1/voices/add
- Header: xi-api-key: {ElevenLabsApiKey from Key Vault}
- Multipart form: name={character name}, files={audio file}
- Receive response: { "voice_id": "string" }
- Store voice_id in VoiceAssignment.VoiceCloneId field (add this column if missing)
- Return success with voiceId

Subscription gate:
- Check that current user's team has Studio tier subscription
- If Basic tier: return Result.Failure("Voice cloning requires Studio subscription")
- Use ISubscriptionService.GetCurrentTierAsync() (look for this service in IdentityModule)

ElevenLabs error handling:
- 400: invalid audio file format → return Result.Failure with user-friendly message
- 401: bad API key → log Error, return Result.Failure
- 429: rate limit → return Result.Failure("ElevenLabs rate limit reached, try again in 1 minute")

Fallback for local dev (when ElevenLabsApiKey is empty):
- Return a fake voice_id "dev-clone-{guid}"
- Log Warning that voice cloning is in mock mode

DELIVERABLES:
1. EDIT: backend/src/AnimStudio.API/Services/VoicePreviewService.cs
2. EDIT: backend/src/AnimStudio.API/Services/VoiceCloneService.cs
3. EDIT (if needed): backend/src/AnimStudio.ContentModule/Domain/Entities/VoiceAssignment.cs (add VoiceCloneId)
4. EF migration (if entity changed): dotnet ef migrations add AddVoiceCloneId --project src\AnimStudio.ContentModule --startup-project src\AnimStudio.API --context ContentDbContext
5. Report: list exact NuGet packages needed
```

---

---

# WAVE 3: MISSING BACKEND PHASES

---

## BACKEND-5 — Phase 11: Sharing & Review Backend

```
You are a senior .NET 8 developer working on AnimStudio using Clean Architecture.
Backend: ASP.NET Core 8, MediatR CQRS, EF Core 8, SQL Server.
DB schema prefix: content.* for ContentModule entities.
All entities extend AggregateRoot<Guid> from AnimStudio.SharedKernel.

WORKING DIRECTORY: C:\Projects\animstudio\backend

READ THESE FILES FIRST (to understand existing patterns):
- backend/src/AnimStudio.ContentModule/Domain/Entities/Episode.cs
- backend/src/AnimStudio.ContentModule/Domain/Entities/Storyboard.cs
- backend/src/AnimStudio.ContentModule/Infrastructure/Persistence/ContentDbContext.cs
- backend/src/AnimStudio.ContentModule/Infrastructure/Repositories/ (pick any one repository)
- backend/src/AnimStudio.ContentModule/Application/Commands/GenerateStoryboard/ (command pattern)
- backend/src/AnimStudio.API/Controllers/EpisodeController.cs (controller pattern + auth pattern)

YOUR TASK: Implement Phase 11 — Sharing & Review

STEP 1: Domain Entities (in ContentModule/Domain/Entities/)

ReviewLink.cs:
- Properties: Token (string, unique), EpisodeId, RenderId, ExpiresAt (DateTime?), IsRevoked (bool),
  PasswordHash (string?), CreatedByUserId (Guid), ViewCount (int, default 0)
- Methods: Revoke(), IncrementViewCount(), IsValid() → bool (not expired, not revoked)
- Generates token in constructor: Token = Guid.NewGuid().ToString("N")[..16] (16 hex chars)

ReviewComment.cs:
- NOT an aggregate — child entity referenced by ReviewLinkId
- Properties: ReviewLinkId, AuthorName (string), Text (string), TimestampSeconds (double),
  IsResolved (bool), ResolvedAt (DateTime?), ResolvedByUserId (Guid?)
- Method: Resolve(userId)

BrandKit.cs:
- Aggregate scoped to TeamId (one per team, unique)
- Properties: TeamId (Guid, unique index), LogoUrl (string?), LogoBlobPath (string?),
  PrimaryColor (string, hex), SecondaryColor (string, hex),
  WatermarkPosition (enum: TopLeft, TopRight, BottomLeft, BottomRight, Center),
  WatermarkOpacity (decimal, 0.0-1.0)
- Method: Update(...)

SocialPublish.cs:
- Properties: EpisodeId, RenderId, Platform (enum: YouTube), ExternalVideoId (string?),
  Status (enum: Pending, Published, Failed), PublishedAt (DateTime?), ErrorMessage (string?)

STEP 2: ContentDbContext (add DbSets + EF config)

Add to ContentDbContext.cs:
- DbSet<ReviewLink> ReviewLinks
- DbSet<ReviewComment> ReviewComments
- DbSet<BrandKit> BrandKits
- DbSet<SocialPublish> SocialPublishes

EF configuration (in OnModelCreating or separate IEntityTypeConfiguration files):
- ReviewLink: unique index on Token, index on EpisodeId
- ReviewComment: FK to ReviewLink, owned collection or separate table
- BrandKit: unique index on TeamId
- All tables in content schema: ToTable("ReviewLinks", "content") etc.

STEP 3: EF Migration

Run:
  dotnet ef migrations add AddPhase11ReviewSharing \
    --project src\AnimStudio.ContentModule \
    --startup-project src\AnimStudio.API \
    --context ContentDbContext

STEP 4: Repositories

Create interfaces + implementations:
- IReviewLinkRepository: GetByToken(token), GetByEpisodeId(episodeId), Add, Update
- IBrandKitRepository: GetByTeamId(teamId), Upsert(brandKit)
- IReviewCommentRepository: GetByReviewLinkId(reviewLinkId), Add, Update

STEP 5: Commands & Queries (in ContentModule/Application/)

Commands:
- CreateReviewLinkCommand(EpisodeId, RenderId, ExpiresAt?, Password?) → Result<ReviewLinkDto>
  Handler: create ReviewLink entity, hash password with BCrypt if provided, save, return DTO with URL
- RevokeReviewLinkCommand(ReviewLinkId, UserId) → Result (must be creator)
- AddReviewCommentCommand(Token, AuthorName, Text, TimestampSeconds) → Result<ReviewCommentDto>
  (public — no auth required)
- ResolveReviewCommentCommand(CommentId, UserId) → Result (must be review link owner)
- UpsertBrandKitCommand(TeamId, LogoFile?, PrimaryColor, SecondaryColor, WatermarkPosition, WatermarkOpacity) → Result<BrandKitDto>

Queries:
- GetReviewLinkQuery(Token, Password?) → Result<ReviewLinkDetailDto>
  Handler: validate token, check password hash if set, increment ViewCount, return render + comments
- GetReviewCommentsQuery(Token) → Result<IReadOnlyList<ReviewCommentDto>>
- GetBrandKitQuery(TeamId) → Result<BrandKitDto>

STEP 6: DTOs (in ContentModule/Application/DTOs/)
- ReviewLinkDto: { Id, Token, ShareUrl, EpisodeId, ExpiresAt, IsRevoked, ViewCount, CreatedAt }
- ReviewLinkDetailDto: ReviewLinkDto + RenderInfo (videoUrl, durationSeconds) + Comments
- ReviewCommentDto: { Id, AuthorName, Text, TimestampSeconds, IsResolved, CreatedAt }
- BrandKitDto: all BrandKit fields

STEP 7: ReviewController (in AnimStudio.API/Controllers/)

Endpoints:
  [Authorize] POST   /api/v1/renders/{renderId}/review-links        → 201 ReviewLinkDto
  [Authorize] DELETE /api/v1/review-links/{id}                       → 204 (revoke)
  [AllowAnonymous] GET /api/v1/review/{token}                        → ReviewLinkDetailDto
  [AllowAnonymous] GET /api/v1/review/{token}/comments               → ReviewCommentDto[]
  [AllowAnonymous] POST /api/v1/review/{token}/comments              → 201 ReviewCommentDto
  [Authorize] PATCH /api/v1/review/{token}/comments/{commentId}/resolve → 204
  [Authorize] GET   /api/v1/teams/{teamId}/brand-kit                 → BrandKitDto
  [Authorize] PUT   /api/v1/teams/{teamId}/brand-kit                 → BrandKitDto

NOTE: Public endpoints (AllowAnonymous) must work with the DevAuthHandler in Development.
Increment ViewCount inside GetReviewLinkQuery handler (not the controller).

DELIVERABLES:
1. NEW: Domain/Entities/ReviewLink.cs, ReviewComment.cs, BrandKit.cs, SocialPublish.cs
2. EDIT: Infrastructure/Persistence/ContentDbContext.cs
3. NEW: Infrastructure/Repositories/IReviewLinkRepository.cs + ReviewLinkRepository.cs
4. NEW: Infrastructure/Repositories/IBrandKitRepository.cs + BrandKitRepository.cs
5. NEW: Application/Commands/CreateReviewLink/, RevokeReviewLink/, AddReviewComment/, UpsertBrandKit/
6. NEW: Application/Queries/GetReviewLink/, GetBrandKit/
7. NEW: Application/DTOs/ReviewLinkDto.cs, ReviewCommentDto.cs, BrandKitDto.cs
8. NEW: API/Controllers/ReviewController.cs
9. EF Migration applied
```

---

## BACKEND-6 — Phase 12: Analytics, Admin, Notifications Backend

```
You are a senior .NET 8 developer working on AnimStudio using Clean Architecture.
AnimStudio has an AnalyticsModule project that exists but has NO entities or handlers yet.

WORKING DIRECTORY: C:\Projects\animstudio\backend

READ THESE FILES FIRST:
- backend/src/AnimStudio.AnalyticsModule/ (read all existing files — likely just project file)
- backend/src/AnimStudio.ContentModule/Domain/Entities/Episode.cs (to understand Guid IDs)
- backend/src/AnimStudio.IdentityModule/Domain/Entities/ (find Subscription.cs, Team.cs)
- backend/src/AnimStudio.API/Controllers/EpisodeController.cs (auth patterns)
- backend/src/AnimStudio.SharedKernel/ (AggregateRoot, Result, domain events)

YOUR TASK: Implement Phase 12 — Analytics & Admin

STEP 1: Analytics Entities (in AnalyticsModule/Domain/Entities/)

VideoView.cs:
- Properties: EpisodeId (Guid), RenderId (Guid), ViewerIpHash (string — SHA256 of IP),
  ViewedAt (DateTime), Source (enum: Direct, Embed, ReviewLink), ReviewLinkId (Guid?)
- NOT an aggregate (no AggregateRoot) — simple entity tracked for analytics

Notification.cs:
- Aggregate owned by UserId
- Properties: UserId (Guid), Type (enum: EpisodeComplete, JobFailed, UsageWarning, SystemAlert),
  Title (string), Body (string), IsRead (bool), ReadAt (DateTime?),
  RelatedEntityId (Guid?), RelatedEntityType (string?), CreatedAt (DateTime)
- Method: MarkRead()

STEP 2: AnalyticsDbContext

Create: backend/src/AnimStudio.AnalyticsModule/Infrastructure/Persistence/AnalyticsDbContext.cs
- DbSet<VideoView> VideoViews
- DbSet<Notification> Notifications
- Tables in analytics schema: ToTable("VideoViews", "analytics") etc.
- Registered in Program.cs alongside existing DbContexts

EF Migration:
  dotnet ef migrations add InitAnalyticsModule \
    --project src\AnimStudio.AnalyticsModule \
    --startup-project src\AnimStudio.API \
    --context AnalyticsDbContext

STEP 3: Usage Metering

Edit HandleJobCompletionCommandHandler (from BACKEND-2):
When jobType = PostProd and status = Completed:
- Find team's Subscription via ISubscriptionRepository
- Call subscription.IncrementEpisodeUsage()
- If usage hits 80% of quota: publish SubscriptionUsageWarningEvent
- If usage hits 100%: publish SubscriptionQuotaExceededEvent

STEP 4: Notification Triggers (INotificationHandler<TEvent> in AnalyticsModule)

Create MediatR notification handlers:
- EpisodeCompletedEventHandler → create Notification(UserId=episode.CreatorId, Type=EpisodeComplete)
- JobFailedEventHandler → create Notification(Type=JobFailed, Body=errorMessage)
- SubscriptionUsageWarningEventHandler → create Notification(Type=UsageWarning) for team owner

STEP 5: Repositories
- INotificationRepository: GetByUserId(userId, unreadOnly?), MarkRead(id), MarkAllRead(userId)
- IVideoViewRepository: Add(view), GetViewCountByEpisode(episodeId), GetViewCountByRender(renderId)

STEP 6: Queries & Commands
- GetEpisodeAnalyticsQuery(EpisodeId) → EpisodeAnalyticsDto { ViewCount, UniqueViewers, RenderCount, ShareCount }
- GetTeamAnalyticsQuery(TeamId) → TeamAnalyticsDto { TotalEpisodes, TotalViews, ActiveSubscription, UsagePercent }
- GetAdminStatsQuery() → AdminStatsDto { TotalUsers, TotalEpisodes, ActiveJobs, RevenueThisMonth }
- GetNotificationsQuery(UserId, UnreadOnly) → IReadOnlyList<NotificationDto>
- MarkNotificationReadCommand(NotificationId, UserId) → Result
- MarkAllNotificationsReadCommand(UserId) → Result
- TrackVideoViewCommand(EpisodeId, RenderId, IpAddress, Source, ReviewLinkId?) → Result (fire-and-forget)

STEP 7: Controllers (in AnimStudio.API/Controllers/)

AnalyticsController:
  GET /api/v1/episodes/{id}/analytics  → EpisodeAnalyticsDto
  GET /api/v1/teams/{id}/analytics     → TeamAnalyticsDto

AdminController: [Authorize(Roles = "Admin")]
  GET /api/v1/admin/stats  → AdminStatsDto
  GET /api/v1/admin/users  → paged list (PagedResult<AdminUserDto>)
  GET /api/v1/admin/jobs   → list of active Hangfire jobs

NotificationController:
  GET   /api/v1/notifications           → NotificationDto[]
  PATCH /api/v1/notifications/{id}/read → 204
  PATCH /api/v1/notifications/read-all  → 204

Wire TrackVideoViewCommand:
- In ReviewController.GetReview (public endpoint): after returning data, enqueue TrackVideoViewCommand via Hangfire (fire-and-forget, "low" queue)

DELIVERABLES:
1. NEW: AnalyticsModule/Domain/Entities/VideoView.cs, Notification.cs
2. NEW: AnalyticsModule/Infrastructure/Persistence/AnalyticsDbContext.cs
3. NEW: AnalyticsModule/Infrastructure/Repositories/ (INotificationRepository + impl, IVideoViewRepository + impl)
4. NEW: AnalyticsModule/Application/Commands/ (MarkNotificationRead, MarkAllRead, TrackVideoView)
5. NEW: AnalyticsModule/Application/Queries/ (GetEpisodeAnalytics, GetTeamAnalytics, GetAdminStats, GetNotifications)
6. NEW: AnalyticsModule/Application/EventHandlers/ (EpisodeCompleted, JobFailed, UsageWarning)
7. NEW: API/Controllers/AnalyticsController.cs, AdminController.cs, NotificationController.cs
8. EDIT: ContentModule/Application/Commands/HandleJobCompletion handler (add usage metering)
9. EF Migration applied
```

---

## BACKEND-7 — FFmpeg Render Pipeline

```
You are a senior .NET 8 developer working on AnimStudio.
This task replaces the stub render pipeline with a real FFmpeg-based one.

WORKING DIRECTORY: C:\Projects\animstudio\backend

READ THESE FILES FIRST:
- backend/src/AnimStudio.API/Hosted/RenderHangfireProcessor.cs (current implementation)
- backend/src/AnimStudio.ContentModule/Domain/Entities/ (read Timeline.cs, TimelineTrack.cs, TimelineClip.cs if they exist; if not, note they are missing)
- backend/src/AnimStudio.ContentModule/Application/Queries/GetStoryboard/ (pattern for DB queries)
- backend/src/AnimStudio.DeliveryModule/ (read all files — Render entity, RenderController)
- backend/src/AnimStudio.API/Services/AzureBlobFileStorageService.cs (from BACKEND-1)

CONTEXT:
- Timeline data IS saved to DB by the frontend (timeline editor API exists).
- RenderHangfireProcessor currently uses a file-size heuristic to estimate duration and has no real FFmpeg execution.
- FFmpeg must be installed on the Container App (add to Dockerfile).
- All clip files are in Azure Blob Storage (CDN URLs from BACKEND-1).
- Output must be uploaded to Blob, then URL returned to frontend via SignalR.

STEP 1: FfmpegFilterGraphBuilder Service

Create: backend/src/AnimStudio.ContentModule/Application/Services/FfmpegFilterGraphBuilder.cs

This class takes a Timeline object and produces an FFmpeg command string.

Interface (create IFfmpegFilterGraphBuilder):
  string BuildCommand(Timeline timeline, string[] inputFilePaths, string outputPath)

Logic:
- Video clips: Use concat filter for sequential clips
  [0:v]trim=start={trimStart}:end={trimEnd},setpts=PTS-STARTPTS[v0]
  [1:v]trim=start={trimStart}:end={trimEnd},setpts=PTS-STARTPTS[v1]
  [v0][v1]concat=n=2:v=1:a=0[vout]
- Audio clips: amix filter with volume
  [N:a]volume={vol/100}[aN]
- Transitions between video clips: xfade filter
  [v0][v1]xfade=transition=fade:duration={transitionDuration/1000}:offset={offset}[vout]
- Output codec: -c:v libx264 -crf 23 -c:a aac -b:a 128k
- Aspect ratio: -vf "scale=1920:1080:force_original_aspect_ratio=decrease,pad=1920:1080:(ow-iw)/2:(oh-ih)/2"

STEP 2: SrtGeneratorService

Create: backend/src/AnimStudio.ContentModule/Application/Services/SrtGeneratorService.cs

Interface: string GenerateSrt(Script script)

Logic:
- Read script.RawJson — extract dialogue lines with startTime, endTime, text, characterName
- Format as SRT:
  {index}
  {HH:MM:SS,mmm} --> {HH:MM:SS,mmm}
  {characterName}: {text}

STEP 3: Update RenderHangfireProcessor

Replace the stub with:
1. Load timeline from DB (ITimelineRepository.GetByEpisodeId)
2. Load all AnimationClips for the episode (get their CDN URLs)
3. Download clip files to temp directory using HttpClient (from CDN URLs)
   OR: if FFmpeg can read URLs directly, pass URLs as inputs (prefer this)
4. Build FFmpeg command via IFfmpegFilterGraphBuilder
5. Execute FFmpeg: Process.Start("ffmpeg", args) with stdout/stderr captured
6. If FFmpeg exits non-zero: log stderr, mark render failed, notify via SignalR
7. Generate SRT via ISrtGeneratorService
8. Upload output.mp4 to Blob: renders/{renderId}/output.mp4
9. Upload captions.srt to Blob: renders/{renderId}/captions.srt
10. Call render.MarkComplete(cdnUrl, srtUrl, durationSeconds)
11. Emit RenderCompleteEvent → this triggers SignalR notification to browser

STEP 4: Dockerfile update

Read: backend/Dockerfile (or create if missing)
Add to Dockerfile:
  RUN apt-get update && apt-get install -y ffmpeg && rm -rf /var/lib/apt/lists/*

STEP 5: Register services in Program.cs

DELIVERABLES:
1. NEW: ContentModule/Application/Services/IFfmpegFilterGraphBuilder.cs
2. NEW: ContentModule/Application/Services/FfmpegFilterGraphBuilder.cs
3. NEW: ContentModule/Application/Services/ISrtGeneratorService.cs
4. NEW: ContentModule/Application/Services/SrtGeneratorService.cs
5. EDIT: API/Hosted/RenderHangfireProcessor.cs (complete rewrite)
6. EDIT: backend/Dockerfile (add FFmpeg)
7. EDIT: Program.cs (register new services)
8. Report: confirm Timeline/TimelineTrack/TimelineClip entities exist in DB schema; if not, list what's missing
```

---

---

# WAVE 4: PYTHON INTEGRATION BRIDGE

---

## PYTHON-1 — Service Bus Bridge (animstudio_bridge.py)

```
You are a Python developer working on the cartoon_automation pipeline.
This project produces animated cartoon episodes using AI models.

WORKING DIRECTORY: C:\Users\Vaibhav\cartoon_automation

READ THESE FILES FIRST:
- Look at the project structure: list all .py files in src/ or the root
- Read requirements.txt (or pyproject.toml)
- Read any existing main entry point (main.py or similar)
- Read specs/job-message-schema.json at C:\Projects\animstudio\specs\job-message-schema.json
  (this is the shared contract created by BACKEND-2)

CONTEXT:
- Currently cartoon_automation runs as a standalone CLI: it processes everything on local disk.
- In production it must: (1) receive jobs from Azure Service Bus "jobs-queue",
  (2) run the pipeline, (3) upload outputs to Blob, (4) publish completion to "completions-queue".
- The Azure Service Bus namespace: animstudio-{environment}-sb
- Authentication: DefaultAzureCredential (works with Managed Identity in prod, az login in dev)

YOUR TASK:

Create: src/cartoon_automation/services/animstudio_bridge.py

```python
"""
AnimStudio Bridge — Service Bus producer/consumer connecting
cartoon_automation pipeline to AnimStudio .NET backend.
"""
import asyncio
import json
import logging
from dataclasses import dataclass
from typing import Any, Optional
from azure.servicebus.aio import ServiceBusClient
from azure.servicebus import ServiceBusMessage
from azure.identity.aio import DefaultAzureCredential

logger = logging.getLogger(__name__)

@dataclass
class JobMessage:
    job_id: str
    episode_id: str
    job_type: str  # "Animation", "CharacterDesign", etc.
    payload: dict

@dataclass 
class CompletionMessage:
    job_id: str
    episode_id: str
    job_type: str
    status: str  # "Completed" | "Failed"
    result: Optional[dict]
    error_message: Optional[str]

class AnimStudioBridge:
    def __init__(self, namespace_fqdn: str, jobs_queue: str, completions_queue: str):
        """
        namespace_fqdn: e.g. animstudio-dev-sb.servicebus.windows.net
        """
        self._namespace_fqdn = namespace_fqdn
        self._jobs_queue = jobs_queue
        self._completions_queue = completions_queue
        self._credential = DefaultAzureCredential()

    async def receive_job(self, timeout_seconds: int = 60) -> Optional[JobMessage]:
        """Receive next job from jobs-queue. Returns None on timeout."""
        ...

    async def complete_message(self, receiver, message) -> None:
        """Mark Service Bus message as complete (removes from queue)."""
        ...

    async def dead_letter_message(self, receiver, message, reason: str) -> None:
        """Dead-letter a message that cannot be processed."""
        ...

    async def report_completion(self, job_message: JobMessage, result: dict) -> None:
        """Publish success to completions-queue."""
        msg = CompletionMessage(
            job_id=job_message.job_id,
            episode_id=job_message.episode_id,
            job_type=job_message.job_type,
            status="Completed",
            result=result,
            error_message=None,
        )
        await self._send_completion(msg)

    async def report_failure(self, job_message: JobMessage, error: str) -> None:
        """Publish failure to completions-queue."""
        msg = CompletionMessage(
            job_id=job_message.job_id,
            episode_id=job_message.episode_id,
            job_type=job_message.job_type,
            status="Failed",
            result=None,
            error_message=error,
        )
        await self._send_completion(msg)

    async def _send_completion(self, msg: CompletionMessage) -> None:
        """Internal: send message to completions-queue."""
        ...

    async def close(self) -> None:
        await self._credential.close()
```

Implement all ... methods using azure-servicebus SDK async API.
Use ServiceBusClient.from_connection_string is NOT allowed — use:
  ServiceBusClient(fully_qualified_namespace=self._namespace_fqdn, credential=self._credential)

Also create: config/azure.yaml

```yaml
service_bus:
  namespace_fqdn: "${AZURE_SERVICE_BUS_NAMESPACE}.servicebus.windows.net"
  jobs_queue: "jobs-queue"
  completions_queue: "completions-queue"
  character_training_queue: "character-training"

blob_storage:
  account_url: "https://${AZURE_STORAGE_ACCOUNT}.blob.core.windows.net"
  container: "assets"

environment: "${ENVIRONMENT:-dev}"
```

Also create: src/cartoon_automation/services/config_loader.py
- Reads config/azure.yaml
- Substitutes ${VAR} with os.environ[VAR]
- Returns typed config dataclass

Also update requirements.txt (add if not present):
- azure-servicebus>=7.12.0
- azure-storage-blob>=12.19.0
- azure-identity>=1.15.0
- pyyaml>=6.0

DELIVERABLES:
1. NEW: src/cartoon_automation/services/animstudio_bridge.py (full implementation)
2. NEW: src/cartoon_automation/services/config_loader.py
3. NEW: config/azure.yaml
4. EDIT: requirements.txt (add azure packages)
5. Report: confirm azure-servicebus async API is used (not sync)
```

---

## PYTHON-2 — Blob Storage Uploader

```
You are a Python developer working on cartoon_automation.

WORKING DIRECTORY: C:\Users\Vaibhav\cartoon_automation

READ THESE FILES FIRST:
- src/cartoon_automation/services/animstudio_bridge.py (from PYTHON-1)
- src/cartoon_automation/services/config_loader.py (from PYTHON-1)
- requirements.txt
- Look at existing phase output file patterns (read any phase*.py files)

YOUR TASK:

Create: src/cartoon_automation/services/blob_uploader.py

```python
"""
Blob Uploader — uploads cartoon_automation outputs to Azure Blob Storage
so AnimStudio can reference them by CDN URL.
"""
import asyncio
import os
import mimetypes
from pathlib import Path
from typing import Optional
from azure.storage.blob.aio import BlobServiceClient
from azure.identity.aio import DefaultAzureCredential

class BlobUploader:
    def __init__(self, account_url: str, container: str = "assets"):
        self._account_url = account_url
        self._container = container
        self._credential = DefaultAzureCredential()

    async def upload_file(self, local_path: str, blob_path: str,
                          content_type: Optional[str] = None) -> str:
        """
        Upload a local file to Blob Storage.
        Returns the blob URL (CDN URL if CDN is configured, else blob service URL).
        """
        ...

    async def upload_bytes(self, data: bytes, blob_path: str,
                           content_type: str) -> str:
        """Upload in-memory bytes. Returns blob URL."""
        ...

    async def blob_exists(self, blob_path: str) -> bool:
        """Check if blob already uploaded (skip re-upload)."""
        ...

    async def close(self) -> None:
        await self._credential.close()
```

BLOB PATH CONVENTIONS (follow exactly):
- Character design images:    episodes/{episodeId}/characters/{characterId}/design.png
- LoRA weights:               episodes/{episodeId}/characters/{characterId}/lora.safetensors
- Storyboard shots:           episodes/{episodeId}/storyboard/scene{N}_shot{M}.png
- Animation clips:            episodes/{episodeId}/animation/scene{N}_shot{M}.mp4
- Final rendered video:       episodes/{episodeId}/renders/{renderId}/output.mp4
- Caption SRT file:           episodes/{episodeId}/renders/{renderId}/captions.srt
- TTS audio previews:         episodes/{episodeId}/audio/preview_{characterId}.mp3

ALSO CREATE: src/cartoon_automation/services/__init__.py
Export: AnimStudioBridge, BlobUploader, ConfigLoader

ALSO CREATE: src/cartoon_automation/worker.py
- Main entry point for production mode (reads from Service Bus, not CLI args)
- Usage: python -m cartoon_automation.worker
- Loop: while True → receive_job → dispatch to phase handler → report completion
- Graceful shutdown on SIGTERM (drain current job, exit)
- Logging: structured JSON logs (use python-json-logger)

DELIVERABLES:
1. NEW: src/cartoon_automation/services/blob_uploader.py
2. NEW: src/cartoon_automation/services/__init__.py
3. NEW: src/cartoon_automation/worker.py
4. EDIT: requirements.txt (add python-json-logger if not present)
5. Report: confirm DefaultAzureCredential is used (not connection strings)
```

---

## PYTHON-3 — Wrap Pipeline Phases with Bridge + Blob

```
You are a Python developer working on cartoon_automation.

WORKING DIRECTORY: C:\Users\Vaibhav\cartoon_automation

READ THESE FILES FIRST (critical — understand current phase logic before changing):
- src/cartoon_automation/worker.py (from PYTHON-2)
- src/cartoon_automation/services/animstudio_bridge.py (from PYTHON-1)
- src/cartoon_automation/services/blob_uploader.py (from PYTHON-2)
- ALL existing phase files (read phase4.py, phase5.py, phase6.py, phase8.py, phase9.py or equivalent)
- Read the output directory structure (what files each phase produces)

CONTEXT:
- Currently each phase reads from and writes to local disk (C:\Users\Vaibhav\cartoon_automation\output\)
- In production: phases receive job payloads from Service Bus, upload outputs to Blob, report back
- DO NOT change the core AI generation logic in each phase — only wrap it
- The wrapper pattern: receive → run existing function → upload output → report

YOUR TASK:
For each phase listed below, add a worker-compatible wrapper function.
Keep the existing functions UNCHANGED. Add new async wrapper functions alongside them.

PHASE 4 — Character Design:
Existing output: local image file (PNG)
Add wrapper function: async def run_character_design_job(job: JobMessage, uploader: BlobUploader, bridge: AnimStudioBridge)
- Call existing character design function
- Upload output PNG to blob path: episodes/{episodeId}/characters/{characterId}/design.png
- Report completion: { "imageUrl": blob_url }

PHASE 4 — LoRA Training:
Existing output: local .safetensors file
Add wrapper: async def run_lora_training_job(job, uploader, bridge)
- Call existing training function
- Upload .safetensors to blob path: episodes/{episodeId}/characters/{characterId}/lora.safetensors
- Report completion: { "loraWeightsUrl": blob_url, "triggerWord": trigger_word_string }

PHASE 5 — Script Generation:
Existing output: screenplay JSON (likely in memory or local file)
Add wrapper: async def run_script_job(job, uploader, bridge)
- Call existing script generation function
- No file upload needed — screenplay is JSON data
- Report completion: { "screenplay": screenplay_dict }

PHASE 6 — Storyboard Planning:
Existing output: storyboard plan JSON
Add wrapper: async def run_storyboard_plan_job(job, uploader, bridge)
- Report completion: { "plan": plan_dict } (no upload)

PHASE 6 — Storyboard Image Generation:
Existing output: shot images (PNG files, one per shot)
Add wrapper: async def run_storyboard_gen_job(job, uploader, bridge)
- For each shot image, upload to: episodes/{episodeId}/storyboard/scene{N}_shot{M}.png
- Report completion: { "shots": [{ "sceneNumber": N, "shotIndex": M, "imageUrl": url }] }

PHASE 8 — Animation Generation:
Existing output: MP4 clips (one per shot)
Add wrapper: async def run_animation_job(job, uploader, bridge)
- For each clip, upload to: episodes/{episodeId}/animation/scene{N}_shot{M}.mp4
- Report completion: { "clips": [{ "sceneNumber": N, "shotIndex": M, "clipUrl": url, "durationSeconds": float }] }
- Get duration using: ffprobe -v quiet -print_format json -show_streams {file}

PHASE 9 — Post-Production:
Existing output: final MP4 + SRT file
Add wrapper: async def run_post_prod_job(job, uploader, bridge)
- Upload final MP4 to: episodes/{episodeId}/renders/{renderId}/output.mp4
- Upload SRT to: episodes/{episodeId}/renders/{renderId}/captions.srt
- Report completion: { "videoUrl": mp4_url, "srtUrl": srt_url, "durationSeconds": float }

UPDATE worker.py dispatch table:
```python
JOB_HANDLERS = {
    "CharacterDesign": run_character_design_job,
    "LoraTraining": run_lora_training_job,
    "Script": run_script_job,
    "StoryboardPlan": run_storyboard_plan_job,
    "StoryboardGen": run_storyboard_gen_job,
    "Animation": run_animation_job,
    "PostProd": run_post_prod_job,
}
```

ERROR HANDLING IN WRAPPERS:
- Catch all exceptions in each wrapper
- Call bridge.report_failure(job, str(exception)) on any error
- Never let an exception propagate to the worker loop (it handles it, but be safe)

DELIVERABLES:
1. EDIT each phase file: add async wrapper functions (do NOT change existing functions)
2. EDIT: src/cartoon_automation/worker.py (add dispatch table + error handling)
3. Report: list exact file names and function names for each wrapper added
```

---

---

# WAVE 5: FRONTEND

---

## FRONTEND-1 — De-mock Existing Phases (Connect to Real Backend)

```
You are a senior React/TypeScript developer working on the AnimStudio Next.js 14 frontend.

WORKING DIRECTORY: C:\Projects\animstudio\frontend

CONTEXT:
- Backend phases 1–9 are fully implemented and running at http://localhost:5001
- Several frontend pages are still using mock interceptors or hardcoded data
- NEXT_PUBLIC_MOCK_DATA=false is already set (mock interceptor checks this flag)
- Use apiFetch() from @/lib/api-client for ALL API calls (handles auth, errors, 401 redirect)
- All server state via TanStack Query (useQuery + useMutation)
- Never use raw fetch() — always apiFetch()

READ THESE FILES FIRST:
- src/lib/api-client.ts (the apiFetch wrapper)
- src/lib/mock-data/mock-interceptor.ts (see which routes are still intercepted)
- src/types/index.ts (existing types)
- src/hooks/ (read all existing hooks — understand patterns before adding)
- src/app/(dashboard)/ (list all page files to understand what exists)

YOUR TASK — systematically remove mocks and connect to real backend:

1. Find all mock interceptors still active in mock-interceptor.ts.
   For each one, determine if the corresponding backend endpoint exists.
   
2. For any route that IS mocked but HAS a real backend endpoint:
   a. Remove the mock intercept for that route
   b. Verify the corresponding hook uses apiFetch() (not mock data)
   c. Test: confirm the real data appears in the UI

3. Specifically ensure these are connected to real backend:
   
   Projects:
   - GET /api/v1/projects → useProjects hook → ProjectListPage
   - POST /api/v1/projects → useCreateProject mutation → CreateProjectDialog
   
   Episodes:
   - GET /api/v1/projects/{id}/episodes → useEpisodes hook
   - POST /api/v1/projects/{id}/episodes → useCreateEpisode mutation
   
   Characters:
   - GET /api/v1/episodes/{id}/characters → useCharacters hook → CharactersPage
   - The character page should show real DB characters (not hardcoded 3 characters)
   
   Saga / Episode state:
   - GET /api/v1/episodes/{id}/saga → useEpisodeSaga hook → this drives the studio sidebar progress
   - Must show real phase completion statuses from DB
   
   Voice assignments:
   - GET /api/v1/episodes/{id}/voice-assignments → useVoiceAssignments hook
   - POST /api/v1/episodes/{id}/voice-assignments/{id} → update voice assignment

4. For each hook that was previously mock-only, check TanStack Query setup:
   - queryKey must be specific enough to not collide
   - staleTime: 30000 (30 seconds)
   - Mutations must call queryClient.invalidateQueries() on success

5. Keep mocks ONLY for:
   - Phase 11 routes (review links, brand kit) — backend not done yet
   - Phase 12 routes (analytics, notifications) — backend not done yet

DELIVERABLES:
1. EDIT: src/lib/mock-data/mock-interceptor.ts (remove completed-phase mocks)
2. EDIT: any hooks that still reference mock data instead of apiFetch()
3. Report: list every route removed from mock interceptor + confirm real endpoint URL used
4. Report: list any routes where backend endpoint is MISSING (these stay mocked)
```

---

## FRONTEND-2 — Phase 11: Review Links & Sharing UI

```
You are a senior React/TypeScript developer working on AnimStudio Next.js 14 frontend.

WORKING DIRECTORY: C:\Projects\animstudio\frontend

CONTEXT:
- Phase 11 backend is complete (from BACKEND-5 prompt): ReviewController is live.
- Public /review/{token} pages require NO authentication.
- All authenticated pages use apiFetch() with auth headers.
- shadcn/ui for all UI components. No raw HTML elements.
- Framer Motion for animations.
- TanStack Query for server state.

READ THESE FILES FIRST:
- src/types/index.ts (add ReviewLink + ReviewComment types here)
- src/lib/api-client.ts (apiFetch pattern)
- src/hooks/use-episodes.ts (TanStack Query hook pattern to follow)
- src/app/(dashboard)/studio/[id]/ (the studio page layout)
- src/middleware.ts (see which routes are public)

PART A: Types (add to src/types/index.ts)

export interface ReviewLink {
  id: string
  token: string
  shareUrl: string
  episodeId: string
  expiresAt: string | null
  isRevoked: boolean
  viewCount: number
  createdAt: string
}

export interface ReviewComment {
  id: string
  authorName: string
  text: string
  timestampSeconds: number
  isResolved: boolean
  createdAt: string
}

export interface BrandKit {
  id: string
  teamId: string
  logoUrl: string | null
  primaryColor: string
  secondaryColor: string
  watermarkPosition: 'TopLeft' | 'TopRight' | 'BottomLeft' | 'BottomRight' | 'Center'
  watermarkOpacity: number
}

PART B: Hooks (in src/hooks/)

use-review-links.ts:
- useReviewLinks(episodeId): query GET /api/v1/episodes/{episodeId}/review-links
- useCreateReviewLink(): mutation POST /api/v1/renders/{renderId}/review-links
  body: { expiresInDays?: number, password?: string }
- useRevokeReviewLink(): mutation DELETE /api/v1/review-links/{id}

use-review.ts (PUBLIC — no auth):
- useReview(token, password?): query GET /api/v1/review/{token}?password={password}
- useReviewComments(token): query GET /api/v1/review/{token}/comments
- useAddReviewComment(): mutation POST /api/v1/review/{token}/comments
  body: { authorName, text, timestampSeconds }
- useResolveComment(): mutation PATCH /api/v1/review/{token}/comments/{id}/resolve

use-brand-kit.ts:
- useBrandKit(teamId): query GET /api/v1/teams/{teamId}/brand-kit
- useUpsertBrandKit(): mutation PUT /api/v1/teams/{teamId}/brand-kit

PART C: Components (in src/components/review/)

review-link-generator.tsx:
- Form: expiry selector (7/30/90 days or custom date) + optional password
- On submit: call useCreateReviewLink mutation
- On success: show generated URL + Copy button + QR code (use qrcode.react library)
- Copy button: navigator.clipboard.writeText() + "Copied!" toast

review-link-card.tsx:
- Show: URL, created date, view count, status badge, Revoke button
- Status: Active (green), Expired (gray), Revoked (red)

comment-panel.tsx:
- List of ReviewComment items (timestamped, with resolve button for owner)
- Add comment form: name input + text textarea + "Add Comment" button
- On comment click: seek video player to timestampSeconds

PART D: Pages

src/app/(dashboard)/studio/[id]/share/page.tsx:
- Section 1: Review Link Generator (requires a completed render to exist)
- Section 2: Active Links list (ReviewLinkCard components)
- Section 3: Brand Kit editor (logo upload, color pickers, watermark settings)
- Requires auth: redirect to login if not authenticated

src/app/review/[token]/page.tsx (PUBLIC — no auth):
- Show password input if the review link has a password
- After password entry: show video player (left 60%) + comment panel (right 40%)
- Video player: HTML5 video element with the render CDN URL
- Sync: clicking comment seeks video to timestampSeconds
- Add comment form at bottom of comment panel
- Update middleware.ts to allow /review/* without auth

PART E: Update middleware.ts
- Add /review/:path* to the public routes list (no auth required)

DELIVERABLES:
1. EDIT: src/types/index.ts (add Review types)
2. NEW: src/hooks/use-review-links.ts
3. NEW: src/hooks/use-review.ts
4. NEW: src/hooks/use-brand-kit.ts
5. NEW: src/components/review/review-link-generator.tsx
6. NEW: src/components/review/review-link-card.tsx
7. NEW: src/components/review/comment-panel.tsx
8. NEW: src/app/(dashboard)/studio/[id]/share/page.tsx
9. NEW: src/app/review/[token]/page.tsx
10. EDIT: src/middleware.ts (add /review/* to public routes)
```

---

## FRONTEND-3 — Phase 12: Analytics, Admin, Notifications UI

```
You are a senior React/TypeScript developer working on AnimStudio Next.js 14 frontend.

WORKING DIRECTORY: C:\Projects\animstudio\frontend

CONTEXT:
- Phase 12 backend is complete (from BACKEND-6).
- Charts: use recharts (already in package.json — verify with: cat package.json | grep recharts)
- Notifications: real-time via SignalR (new notification → update bell badge count)
- shadcn/ui for all components. Framer Motion for page transitions.

READ THESE FILES FIRST:
- src/types/index.ts
- src/hooks/ (all files — understand existing SignalR hook pattern)
- src/lib/api-client.ts
- src/app/(dashboard)/layout.tsx (where notification bell should be added to header)
- src/components/layout/ (find the top navigation component)

PART A: Types (add to src/types/index.ts)

export interface EpisodeAnalytics {
  episodeId: string
  viewCount: number
  uniqueViewers: number
  renderCount: number
  shareCount: number
}

export interface TeamAnalytics {
  totalEpisodes: number
  totalViews: number
  usagePercent: number
  subscriptionTier: string
}

export interface Notification {
  id: string
  type: 'EpisodeComplete' | 'JobFailed' | 'UsageWarning' | 'SystemAlert'
  title: string
  body: string
  isRead: boolean
  readAt: string | null
  relatedEntityId: string | null
  createdAt: string
}

PART B: Hooks

use-analytics.ts:
- useEpisodeAnalytics(episodeId): GET /api/v1/episodes/{id}/analytics
- useTeamAnalytics(teamId): GET /api/v1/teams/{id}/analytics

use-notifications.ts:
- useNotifications(unreadOnly?): GET /api/v1/notifications
- useMarkNotificationRead(): PATCH /api/v1/notifications/{id}/read
- useMarkAllNotificationsRead(): PATCH /api/v1/notifications/read-all
- Also: subscribe to SignalR "NewNotification" event to invalidate query on receipt

use-admin.ts (admin role only):
- useAdminStats(): GET /api/v1/admin/stats
- Only call if user has admin role (check from auth context)

PART C: Notification Bell Component

src/components/notifications/notification-bell.tsx:
- Icon: Bell icon from lucide-react
- Badge: red dot with count of unread notifications (hide if 0)
- Click: open NotificationPanel (popover or sheet)
- SignalR: when "NewNotification" event fires, increment badge count

src/components/notifications/notification-panel.tsx:
- List of notifications (newest first)
- Each notification: icon by type + title + body + time ago + mark read button
- "Mark all read" button at top
- Empty state: "No notifications"
- Max height with scroll

Add NotificationBell to the dashboard layout header.

PART D: Analytics Page

src/app/(dashboard)/analytics/page.tsx:
- Header: "Analytics"
- Section 1: Team-level stats (4 metric cards in a row)
  - Total Episodes, Total Views, Usage %, Subscription Tier
- Section 2: Per-episode table
  - Columns: Episode Name | Views | Unique Viewers | Renders | Shares
  - Sortable by Views (default: highest first)
- Use recharts BarChart for views over time (use mock dates if real time-series not available)

src/components/analytics/metric-card.tsx:
- Props: { label, value, trend?: number, icon }
- Display: large number + label + optional trend arrow (up/down %)
- Subtle border + shadow

PART E: Admin Page (admin-only)

src/app/(dashboard)/admin/page.tsx:
- Guard: if user role !== 'Admin', redirect to /dashboard
- Section 1: System stats (DAU, MAU, active jobs, error rate)
- Section 2: User list table (paginated)
- Section 3: Job queue status (active Hangfire jobs)

DELIVERABLES:
1. EDIT: src/types/index.ts (add analytics + notification types)
2. NEW: src/hooks/use-analytics.ts
3. NEW: src/hooks/use-notifications.ts
4. NEW: src/hooks/use-admin.ts
5. NEW: src/components/notifications/notification-bell.tsx
6. NEW: src/components/notifications/notification-panel.tsx
7. NEW: src/app/(dashboard)/analytics/page.tsx
8. NEW: src/components/analytics/metric-card.tsx
9. NEW: src/app/(dashboard)/admin/page.tsx
10. EDIT: src/app/(dashboard)/layout.tsx (add NotificationBell to header)
```

---

---

# WAVE 6: CI/CD

---

## CICD-1 — GitHub Actions Pipelines

```
You are a DevOps engineer working on AnimStudio GitHub Actions CI/CD.

WORKING DIRECTORY: C:\Projects\animstudio\.github\workflows

READ THESE FILES FIRST:
- .github/workflows/ (list and read all existing workflow files)
- infra/deploy.ps1 (from INFRA-1)
- backend/Dockerfile (from BACKEND-7)
- frontend/staticwebapp.config.json

CONTEXT:
- Git repository: github.com/[org]/animstudio (use placeholder — user will fill in)
- Azure subscription: use AZURE_SUBSCRIPTION_ID secret
- ACR: animstudio{environment}acr.azurecr.io
- API Container App: animstudio-{environment}-api
- SWA: deployed via Azure/static-web-apps-deploy@v1 action
- OIDC auth: Use azure/login@v2 with federated credentials (no stored passwords)
- Environments: dev (auto-deploy on push to main), prod (requires manual approval)

CREATE OR REPLACE these workflow files:

1. .github/workflows/infra.yml

Trigger: push to main when files in infra/** change; also manual workflow_dispatch

Jobs:
  validate:
    - az bicep build --file infra/main.bicep (lint check)
    - az deployment group what-if (dry run, report output)
  
  deploy-dev:
    needs: validate
    environment: dev
    steps:
    - az login (OIDC)
    - az deployment group create \
        --resource-group animstudio-dev-rg \
        --template-file infra/main.bicep \
        --parameters @infra/parameters/dev.json

  deploy-prod:
    needs: validate
    environment: prod  # has manual approval gate in GitHub
    if: github.ref == 'refs/heads/main'
    steps:
    - (same as dev but with prod parameters)

2. .github/workflows/deploy-api.yml

Trigger: push to main when backend/** changes; manual workflow_dispatch

Jobs:
  test:
    - dotnet build --configuration Release
    - dotnet test --no-build (run AnimStudio.UnitTests)
    - Upload test results as artifact
  
  build-push:
    needs: test
    - az login (OIDC)
    - az acr login --name animstudio{environment}acr
    - docker build -t animstudio{environment}acr.azurecr.io/animstudio-api:{github.sha} ./backend
    - docker push
  
  deploy-dev:
    needs: build-push
    environment: dev
    - az containerapp update \
        --name animstudio-dev-api \
        --resource-group animstudio-dev-rg \
        --image animstudio{environment}acr.azurecr.io/animstudio-api:{github.sha}
    - Smoke test: curl -f https://{API_FQDN}/api/v1/health (retry 5x with 10s sleep)
  
  deploy-prod:
    needs: build-push
    environment: prod  # manual approval gate
    - same as dev with prod names

3. .github/workflows/deploy-web.yml

Trigger: push to main when frontend/** changes; manual workflow_dispatch

Jobs:
  build-test:
    - pnpm install --frozen-lockfile
    - pnpm build (Next.js build)
    - pnpm lint
  
  deploy-dev:
    needs: build-test
    environment: dev
    - uses: Azure/static-web-apps-deploy@v1
      with:
        azure_static_web_apps_api_token: ${{ secrets.SWA_DEPLOY_TOKEN_DEV }}
        repo_token: ${{ secrets.GITHUB_TOKEN }}
        action: upload
        app_location: frontend
        output_location: .next
    - Smoke test: curl -f https://{SWA_DEV_HOSTNAME}
  
  deploy-prod:
    needs: build-test
    environment: prod  # manual approval gate
    - same with SWA_DEPLOY_TOKEN_PROD

REQUIRED GITHUB SECRETS (list in comments):
  AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID (for OIDC)
  SWA_DEPLOY_TOKEN_DEV, SWA_DEPLOY_TOKEN_PROD (from az staticwebapp secrets list)
  ACR_DEV_LOGINSERVER, ACR_PROD_LOGINSERVER

DELIVERABLES:
1. CREATE/REPLACE: .github/workflows/infra.yml
2. CREATE/REPLACE: .github/workflows/deploy-api.yml
3. CREATE/REPLACE: .github/workflows/deploy-web.yml
4. NEW: .github/workflows/README.md (list all required secrets + how to get each one)
```

---

## CICD-2 — End-to-End Integration Test Checklist

```
You are a QA engineer writing integration validation scripts for AnimStudio.

WORKING DIRECTORY: C:\Projects\animstudio

CONTEXT:
After all production readiness tracks are complete, the following must be verified
in the DEV Azure environment before promoting to prod.

YOUR TASK:
Create a structured validation script and checklist.

1. Create infra/validate-deployment.ps1
   A PowerShell script that validates a deployment is healthy:
   
   Checks to run:
   a. API health: curl https://{API_FQDN}/api/v1/health → 200 OK
   b. SWA health: curl https://{SWA_HOSTNAME} → 200 OK
   c. Key Vault access: az keyvault secret list --vault-name {kvName} → lists secrets without error
   d. Storage access: az storage container list --account-name {storAccount} --auth-mode login → success
   e. Service Bus: az servicebus queue show --namespace-name {sbNs} --name jobs-queue → exists
   f. SignalR: az signalr show --name {signalrName} → provisioned
   g. Container App logs: az containerapp logs show --name {apiAppName} → no ERROR level logs in last 10 mins
   
   Accepts params: -Environment (dev|prod), -ResourceGroup
   Outputs: PASS/FAIL per check, overall status

2. Create docs/e2e-test-checklist.md
   Manual test steps organized by feature area:

   INFRA:
   □ Run validate-deployment.ps1 --Environment dev → all checks PASS
   □ MSI: API Container App can read Key Vault secrets (check startup logs for "Key Vault connected")
   □ MSI: API can write to Blob (call POST /api/v1/test-blob-write if it exists, or check via portal)
   
   BACKEND — Blob Storage:
   □ Create episode → upload character image → verify blob appears in portal assets container
   □ Get file URL → SAS URL is valid and playable in browser
   
   BACKEND — Service Bus Round-trip:
   □ Post synthetic message to jobs-queue via Service Bus Explorer
   □ Python worker receives it (check Container App logs)
   □ Worker uploads mock output to Blob
   □ Worker posts to completions-queue
   □ CompletionMessageProcessor processes it (check API logs)
   □ DB updated (check via Swagger: GET /api/v1/episodes/{id})
   □ SignalR event delivered to browser (check browser console)
   
   BACKEND — Voice:
   □ POST /api/v1/voices/preview → returns playable MP3 URL (not placeholder)
   □ Audio plays in browser
   
   BACKEND — Phase 11 Review:
   □ Create review link → copy URL
   □ Open URL in incognito → video plays without login
   □ Add comment → timestamp marker appears on video
   □ Creator sees comment in share page
   
   BACKEND — Phase 12 Analytics:
   □ Open review link → ViewCount increments in DB
   □ GET /api/v1/episodes/{id}/analytics → viewCount > 0
   □ Notification bell shows "Episode complete" notification after render
   
   FRONTEND:
   □ Login → redirects to dashboard
   □ Create project → appears in project list (real DB, not mock)
   □ Create episode → appears in episode list
   □ Go through phases 1–9 → each phase saves real data, saga advances
   □ Timeline editor → save → render → video appears in player
   □ Share page → generate review link → public review page works
   □ Analytics page → shows real view counts
   □ Notification bell → badge count updates in real-time
   
   CI/CD:
   □ Push a change to backend/ → deploy-api.yml triggers → API deployed → smoke test passes
   □ Push a change to frontend/ → deploy-web.yml triggers → SWA deployed → page loads
   □ Push a change to infra/ → infra.yml triggers → what-if runs clean

DELIVERABLES:
1. NEW: infra/validate-deployment.ps1
2. NEW: docs/e2e-test-checklist.md
```

---

---

## APPENDIX: QUICK REFERENCE

### Sequence Summary (which wave unblocks what)

```
INFRA-1 (deploy Bicep)
  └─→ BACKEND-1 (Blob service needs storage account)
  └─→ BACKEND-3 (Service Bus publisher needs namespace)
  └─→ PYTHON-1 (bridge needs Service Bus namespace FQDN)

INFRA-3 (Key Vault secrets)
  └─→ All backend services that read secrets at startup

BACKEND-2 (CompletionMessageProcessor + HandleJobCompletion)
  └─→ PYTHON-3 (python phases need to know what completions look like)

BACKEND-5 + BACKEND-6 (Phase 11 + 12 backend)
  └─→ FRONTEND-2 (Phase 11 UI needs endpoints)
  └─→ FRONTEND-3 (Phase 12 UI needs endpoints)

BACKEND-7 (FFmpeg render)
  └─→ CICD-2 (smoke test includes render validation)

All WAVE 2+3 done
  └─→ FRONTEND-1 (de-mock all existing phases)
```

### Critical File Paths Quick Reference

```
Backend core:
  backend/src/AnimStudio.API/Services/AzureBlobFileStorageService.cs
  backend/src/AnimStudio.API/Hosted/CompletionMessageProcessor.cs
  backend/src/AnimStudio.API/Hosted/AnimationJobHangfireProcessor.cs
  backend/src/AnimStudio.API/Services/VoicePreviewService.cs
  backend/src/AnimStudio.ContentModule/Application/Commands/HandleJobCompletion/
  backend/src/AnimStudio.API/Program.cs

Backend Phase 11:
  backend/src/AnimStudio.ContentModule/Domain/Entities/ReviewLink.cs
  backend/src/AnimStudio.ContentModule/Domain/Entities/BrandKit.cs
  backend/src/AnimStudio.API/Controllers/ReviewController.cs

Backend Phase 12:
  backend/src/AnimStudio.AnalyticsModule/Domain/Entities/Notification.cs
  backend/src/AnimStudio.API/Controllers/NotificationController.cs

Python bridge:
  C:\Users\Vaibhav\cartoon_automation\src\cartoon_automation\services\animstudio_bridge.py
  C:\Users\Vaibhav\cartoon_automation\src\cartoon_automation\services\blob_uploader.py
  C:\Users\Vaibhav\cartoon_automation\src\cartoon_automation\worker.py

Frontend hooks:
  frontend/src/hooks/use-review-links.ts
  frontend/src/hooks/use-notifications.ts
  frontend/src/lib/mock-data/mock-interceptor.ts (remove routes from here)

Infra:
  infra/main.bicep
  infra/deploy.ps1
  infra/seed-keyvault.ps1
  infra/modules/cdn.bicep
  .github/workflows/deploy-api.yml
  .github/workflows/deploy-web.yml
```

### Per-prompt Pre-conditions

| Prompt | Must Run After |
|--------|---------------|
| INFRA-1 | — (first) |
| INFRA-2 | INFRA-1 |
| INFRA-3 | INFRA-1 |
| BACKEND-1 | INFRA-1 |
| BACKEND-2 | — (can start immediately) |
| BACKEND-3 | BACKEND-2 |
| BACKEND-4 | INFRA-3 (needs Key Vault keys in config) |
| BACKEND-5 | — |
| BACKEND-6 | BACKEND-2 (needs domain events from HandleJobCompletion) |
| BACKEND-7 | BACKEND-1 (needs Blob upload), BACKEND-2 |
| PYTHON-1 | INFRA-1 (Service Bus namespace must exist) |
| PYTHON-2 | PYTHON-1 |
| PYTHON-3 | PYTHON-2, BACKEND-2 (shared contract must be defined) |
| FRONTEND-1 | BACKEND-1 through BACKEND-4 deployed and running |
| FRONTEND-2 | BACKEND-5 deployed |
| FRONTEND-3 | BACKEND-6 deployed |
| CICD-1 | All above complete |
| CICD-2 | CICD-1 + first successful deployment |
