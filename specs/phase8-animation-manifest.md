# Phase 8 — Animation Studio File Manifest

## Backend — Domain
- `backend/src/AnimStudio.ContentModule/Domain/Enums/AnimationBackend.cs` — enum: Kling, Local
- `backend/src/AnimStudio.ContentModule/Domain/Enums/AnimationStatus.cs` — enum: PendingApproval, Approved, Running, Completed, Failed, Cancelled
- `backend/src/AnimStudio.ContentModule/Domain/Enums/ClipStatus.cs` — enum: Pending, Rendering, Ready, Failed
- `backend/src/AnimStudio.ContentModule/Domain/Entities/AnimationJob.cs` — aggregate: approval lifecycle, cost tracking
- `backend/src/AnimStudio.ContentModule/Domain/Entities/AnimationClip.cs` — aggregate: per-shot clip with status lifecycle
- `backend/src/AnimStudio.ContentModule/Domain/Events/AnimationDomainEvents.cs` — AnimationJobApprovedEvent, AnimationJobCompletedEvent, AnimationClipReadyEvent

## Backend — Application Interfaces
- `backend/src/AnimStudio.ContentModule/Application/Interfaces/IAnimationJobRepository.cs`
- `backend/src/AnimStudio.ContentModule/Application/Interfaces/IAnimationClipRepository.cs`
- `backend/src/AnimStudio.ContentModule/Application/Interfaces/IAnimationEstimateService.cs`
- `backend/src/AnimStudio.ContentModule/Application/Interfaces/IAnimationClipNotifier.cs` — SignalR abstraction
- `backend/src/AnimStudio.ContentModule/Application/Interfaces/IFileStorageService.cs` — storage abstraction (shared with Phase 6/7)
- `backend/src/AnimStudio.ContentModule/Application/Interfaces/IClipUrlSigner.cs` — legacy Azure SAS signer (kept for BlobClipUrlSigner)

## Backend — Application Commands
- `backend/src/AnimStudio.ContentModule/Application/Commands/ApproveAnimation/ApproveAnimationCommand.cs` — validates + creates AnimationJob + seeds clips + enqueues Job row

## Backend — Application Queries
- `backend/src/AnimStudio.ContentModule/Application/Queries/GetAnimationEstimate/GetAnimationEstimateQuery.cs` — itemised cost estimate from storyboard
- `backend/src/AnimStudio.ContentModule/Application/Queries/GetAnimationClips/GetAnimationClipsQuery.cs` — list clips; ClipUrl resolved via IFileStorageService
- `backend/src/AnimStudio.ContentModule/Application/Queries/GetAnimationClipSignedUrl/GetAnimationClipSignedUrlQuery.cs` — per-clip URL via IFileStorageService (local: permanent; blob: SAS)

## Backend — Application Event Handlers
- `backend/src/AnimStudio.ContentModule/Application/EventHandlers/AnimationClipReadyEventHandler.cs` — MediatR INotificationHandler<AnimationClipReadyEvent>; resolves TeamId (Episode→Project) and calls IAnimationClipNotifier

## Backend — Application DTOs
- `backend/src/AnimStudio.ContentModule/Application/DTOs/AnimationDtos.cs` — AnimationJobDto, AnimationClipDto, AnimationEstimateDto, AnimationEstimateLineItem, ApproveAnimationRequest, SignedClipUrlDto

## Backend — Infrastructure Repositories
- `backend/src/AnimStudio.ContentModule/Infrastructure/Repositories/AnimationJobRepository.cs`
- `backend/src/AnimStudio.ContentModule/Infrastructure/Repositories/AnimationClipRepository.cs`

## Backend — Infrastructure Persistence
- `backend/src/AnimStudio.ContentModule/Infrastructure/Persistence/ContentDbContext.cs` — AnimationJobs + AnimationClips DbSets + OnModelCreating config (table/schema/indices/query filters/RowVersion)
- `backend/src/AnimStudio.ContentModule/Migrations/20260419120000_Phase8Animation.cs` — EF migration for AnimationJobs + AnimationClips tables

## Backend — API Services
- `backend/src/AnimStudio.API/Services/AnimationEstimateService.cs` — IAnimationEstimateService impl; reads shot count from storyboard; rates from config
- `backend/src/AnimStudio.API/Services/SignalRAnimationClipNotifier.cs` — IAnimationClipNotifier impl; broadcasts ClipReady to "team:{teamId}" SignalR group
- `backend/src/AnimStudio.API/Services/LocalFileStorageService.cs` — IFileStorageService for local dev (FileStorage:Provider=Local)
- `backend/src/AnimStudio.API/Services/AzureBlobFileStorageService.cs` — IFileStorageService for prod (Provider=AzureBlob); wraps IClipUrlSigner
- `backend/src/AnimStudio.API/Services/BlobClipUrlSigner.cs` — IClipUrlSigner; stamps SAS token onto blob paths

## Backend — API Controllers
- `backend/src/AnimStudio.API/Controllers/AnimationController.cs` — 4 routes: GET estimate, POST approve (enqueues Hangfire), GET clips, GET clip URL
- `backend/src/AnimStudio.API/Controllers/FileStorageController.cs` — GET /api/v1/files/{**filePath}; AllowAnonymous; Local provider only; range-request support

## Backend — Hangfire Jobs
- `backend/src/AnimStudio.API/Hosted/AnimationJobHangfireProcessor.cs` — enqueued by AnimationController; Local: scans LocalRootPath for mp4s, marks clips Ready, fires domain events; Kling: stub

## Backend — Module Registration
- `backend/src/AnimStudio.ContentModule/ContentModuleRegistration.cs` — registers IAnimationJobRepository, IAnimationClipRepository, AnimationClipReadyEventHandler
- `backend/src/AnimStudio.API/Program.cs` — registers AnimationEstimateService, BlobClipUrlSigner, SignalRAnimationClipNotifier, AnimationJobHangfireProcessor, IFileStorageService (provider-conditional); seeds dev AnimationJob + 8 clips

## Backend — Job Completion Extension
- `backend/src/AnimStudio.ContentModule/Application/Commands/HandleJobCompletion/HandleJobCompletionCommand.cs` — extended with HandleAnimationResultAsync for JobType.Animation; injects IAnimationJobRepository, IAnimationClipRepository, IPublisher

## Frontend — Hooks (real API)
- `frontend/src/hooks/use-animation.ts` — useAnimationEstimate, useAnimationClips, useApproveAnimation, fetchSignedClipUrl, useAnimationRealtime (SignalR)

## Frontend — Pages (de-mocked)
- `frontend/src/app/(dashboard)/studio/[id]/animation/page.tsx` — rewritten: uses useAnimationEstimate + useAnimationClips + useApproveAnimation + useAnimationRealtime; no mock imports

## Frontend — Components (updated)
- `frontend/src/components/animation/clip-preview-grid.tsx` — refactored to use AnimationClipDto from @/types (was MockAnimationClip); ClipStatus values updated to match API enum
- `frontend/src/components/animation/approval-dialog.tsx` — unchanged; mockBalance prop optional and omitted from real page
- `frontend/src/components/animation/backend-selector.tsx` — unchanged; uses AnimationBackend from @/types
- `frontend/src/components/animation/cost-breakdown-table.tsx` — unchanged

## Frontend — Mock Data (still mocked)
- `frontend/src/lib/mock-data/mock-animation.ts` — kept for timeline/composer pages that still reference mock clips; NOT used by animation studio page
- `frontend/src/lib/mock-data/mock-interceptor.ts` — animation routes removed from interceptor comment; pass-through to real backend

## Specs
- `specs/openapi/phase8-animation-api.yaml` — OpenAPI 3.1 spec for all 4 animation endpoints + /api/v1/files
- `specs/phase8-animation-manifest.md` — this file
- `specs/phase8-animation-architecture.md` — data flow, config, design decisions, SignalR contract, Python webhook contract

## Testing Checklist
- [ ] `GET /api/v1/episodes/{devEpisodeId}/animation/estimate?backend=Local` → 200, unitCostUsd=0
- [ ] `POST /api/v1/episodes/{devEpisodeId}/animation` body `{"backend":"Local"}` → 202, status=Approved
- [ ] Hangfire dashboard at /hangfire shows the AnimationJobHangfireProcessor job
- [ ] After job runs: `GET /api/v1/episodes/{devEpisodeId}/animation` → clips with status=Ready and clipUrl=http://localhost:5001/api/v1/files/animation/23MarAnimation/...
- [ ] `GET /api/v1/files/animation/23MarAnimation/scene_01_shot_01.mp4` → 200 video/mp4 stream
- [ ] `GET /api/v1/episodes/{devEpisodeId}/animation/clips/{clipId}` → 200 with url+expiresAt
- [ ] Second POST same episode → 409 Conflict
- [ ] Animation studio page loads clips from real API, plays video in ClipDialog
- [ ] SignalR: approve a new episode → watch browser console for ClipReady events
