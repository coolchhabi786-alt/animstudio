# Phase 8 — Animation Studio File Manifest

## Backend — Domain
- `backend/src/AnimStudio.ContentModule/Domain/Enums/AnimationBackend.cs`
- `backend/src/AnimStudio.ContentModule/Domain/Enums/AnimationStatus.cs`
- `backend/src/AnimStudio.ContentModule/Domain/Enums/ClipStatus.cs`
- `backend/src/AnimStudio.ContentModule/Domain/Entities/AnimationJob.cs`
- `backend/src/AnimStudio.ContentModule/Domain/Entities/AnimationClip.cs`
- `backend/src/AnimStudio.ContentModule/Domain/Events/AnimationDomainEvents.cs`
  - `AnimationJobApprovedEvent`
  - `AnimationJobCompletedEvent`
  - `AnimationClipReadyEvent`

## Backend — Application
- `backend/src/AnimStudio.ContentModule/Application/DTOs/AnimationDtos.cs`
  - `AnimationJobDto`, `AnimationClipDto`, `AnimationEstimateDto`, `AnimationEstimateLineItem`, `SignedClipUrlDto`, `ApproveAnimationRequest`
- `backend/src/AnimStudio.ContentModule/Application/Interfaces/IAnimationJobRepository.cs`
- `backend/src/AnimStudio.ContentModule/Application/Interfaces/IAnimationClipRepository.cs`
- `backend/src/AnimStudio.ContentModule/Application/Interfaces/IAnimationEstimateService.cs`
- `backend/src/AnimStudio.ContentModule/Application/Interfaces/IAnimationClipNotifier.cs`
- `backend/src/AnimStudio.ContentModule/Application/Commands/ApproveAnimation/ApproveAnimationCommand.cs`
- `backend/src/AnimStudio.ContentModule/Application/Queries/GetAnimationEstimate/GetAnimationEstimateQuery.cs`
- `backend/src/AnimStudio.ContentModule/Application/Queries/GetAnimationClips/GetAnimationClipsQuery.cs`
- `backend/src/AnimStudio.ContentModule/Application/Queries/GetAnimationClipSignedUrl/GetAnimationClipSignedUrlQuery.cs`

## Backend — Infrastructure
- `backend/src/AnimStudio.ContentModule/Infrastructure/Repositories/AnimationJobRepository.cs`
- `backend/src/AnimStudio.ContentModule/Infrastructure/Repositories/AnimationClipRepository.cs`
- `backend/src/AnimStudio.ContentModule/Infrastructure/Persistence/ContentDbContext.cs` **(updated)**
- `backend/src/AnimStudio.ContentModule/Migrations/20260419120000_Phase8Animation.cs` **(new)**
- `backend/src/AnimStudio.ContentModule/Migrations/20260419120000_Phase8Animation.Designer.cs` **(new)**
- `backend/src/AnimStudio.ContentModule/Migrations/ContentDbContextModelSnapshot.cs` **(updated)**
- `backend/src/AnimStudio.ContentModule/ContentModuleRegistration.cs` **(updated — repository + estimate service registration)**

## Backend — API
- `backend/src/AnimStudio.API/Controllers/AnimationController.cs`
- `backend/src/AnimStudio.API/Services/AnimationEstimateService.cs`
- `backend/src/AnimStudio.API/Services/SignalRAnimationClipNotifier.cs`
- `backend/src/AnimStudio.API/Program.cs` **(updated — register services)**

## Frontend
- `frontend/src/types/index.ts` **(updated — Animation DTOs)**
- `frontend/src/hooks/use-animation.ts`
- `frontend/src/components/animation/cost-estimate-card.tsx`
- `frontend/src/components/animation/clip-player.tsx`
- `frontend/src/components/animation/approval-dialog.tsx`
- `frontend/src/app/(dashboard)/projects/[id]/episodes/[episodeId]/animation/page.tsx`

## Tests
- `backend/tests/AnimStudio.UnitTests/Commands/AnimationCommandHandlerTests.cs`
- `frontend/tests/e2e/animation.spec.ts`

## Specs
- `specs/openapi/phase8-animation-api.yaml`
- `specs/database/phase8-animation-entities.cs`
- `specs/phase8-animation-architecture.md`
- `specs/phase8-animation-manifest.md`
- `specs/phase8-animation-security-review.md`
