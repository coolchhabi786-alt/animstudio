# Phase 6 — Storyboard Studio — File Manifest

This manifest lists every file each downstream agent must create or modify for
Phase 6. Paths are relative to the repository root `C:\Projects\animstudio\`.

## Entity summary

| Entity            | Key fields |
|-------------------|-----------|
| Storyboard        | Id, EpisodeId (unique), ScreenplayTitle, RawJson, DirectorNotes, timestamps, RowVersion |
| StoryboardShot    | Id, StoryboardId (FK), SceneNumber, ShotIndex, ImageUrl?, Description, StyleOverride?, RegenerationCount, timestamps, RowVersion |

## API endpoints

| Verb + Path                                               | Operation          | Returns        |
|-----------------------------------------------------------|--------------------|----------------|
| POST `/api/v1/episodes/{id}/storyboard`                   | generateStoryboard | 202 JobDto     |
| GET  `/api/v1/episodes/{id}/storyboard`                   | getStoryboard      | 200 StoryboardDto / 404 |
| POST `/api/v1/storyboard/shots/{shotId}/regenerate`       | regenerateShot     | 202 JobDto     |
| PUT  `/api/v1/storyboard/shots/{shotId}/style`            | updateShotStyle    | 202 JobDto     |

SignalR events broadcast on `/hubs/progress` (group `team:{teamId}`):
- `ShotUpdated` — `{ shotId, storyboardId, episodeId, imageUrl, regenerationCount }`

## ContentModule — Domain files

- `backend/src/AnimStudio.ContentModule/Domain/Entities/Storyboard.cs`
- `backend/src/AnimStudio.ContentModule/Domain/Entities/StoryboardShot.cs`
- `backend/src/AnimStudio.ContentModule/Domain/Events/StoryboardDomainEvents.cs`

## ContentModule — Application files

### DTOs
- `backend/src/AnimStudio.ContentModule/Application/DTOs/StoryboardDtos.cs`

### Interfaces (ports)
- `backend/src/AnimStudio.ContentModule/Application/Interfaces/IStoryboardRepository.cs`
- `backend/src/AnimStudio.ContentModule/Application/Interfaces/IStoryboardShotNotifier.cs`

### Commands
- `backend/src/AnimStudio.ContentModule/Application/Commands/GenerateStoryboard/GenerateStoryboardCommand.cs`
- `backend/src/AnimStudio.ContentModule/Application/Commands/RegenerateShot/RegenerateShotCommand.cs`
- `backend/src/AnimStudio.ContentModule/Application/Commands/UpdateShotStyle/UpdateShotStyleCommand.cs`

### Queries
- `backend/src/AnimStudio.ContentModule/Application/Queries/GetStoryboard/GetStoryboardQuery.cs`

## ContentModule — Infrastructure files

- `backend/src/AnimStudio.ContentModule/Infrastructure/Persistence/ContentDbContext.cs`
  *(extend to add Storyboards + StoryboardShots DbSets and model configs)*
- `backend/src/AnimStudio.ContentModule/Infrastructure/Repositories/StoryboardRepository.cs`
- `backend/src/AnimStudio.ContentModule/ContentModuleRegistration.cs` *(add repository)*

### Migrations (EF Core generated)
- `backend/src/AnimStudio.ContentModule/Migrations/20260414120000_Phase6Storyboard.cs`
- `backend/src/AnimStudio.ContentModule/Migrations/20260414120000_Phase6Storyboard.Designer.cs`
- `backend/src/AnimStudio.ContentModule/Migrations/ContentDbContextModelSnapshot.cs` *(updated)*

## AnimStudio.API files

- `backend/src/AnimStudio.API/Controllers/StoryboardController.cs`
- `backend/src/AnimStudio.API/Services/SignalRStoryboardShotNotifier.cs`
- `backend/src/AnimStudio.API/Program.cs` *(wire the notifier adapter)*
- Optional: continue to use existing `ProgressHub`; `ShotUpdated` event sent to `team:{teamId}` group.

## Frontend files

### Types
- `frontend/src/types/index.ts` *(append Phase 6 types)*

### Hook
- `frontend/src/hooks/use-storyboard.ts`

### Page
- `frontend/src/app/(dashboard)/projects/[id]/episodes/[episodeId]/storyboard/page.tsx`

### Components
- `frontend/src/components/storyboard/shot-grid.tsx`
- `frontend/src/components/storyboard/shot-card.tsx`
- `frontend/src/components/storyboard/shot-viewer-modal.tsx`
- `frontend/src/components/storyboard/style-override-popover.tsx`

## Tests

- `backend/tests/AnimStudio.UnitTests/Storyboard/GenerateStoryboardHandlerTests.cs`
- `backend/tests/AnimStudio.UnitTests/Storyboard/RegenerateShotHandlerTests.cs`
- `backend/tests/AnimStudio.UnitTests/Storyboard/UpdateShotStyleHandlerTests.cs`
- `backend/tests/AnimStudio.UnitTests/Storyboard/GetStoryboardHandlerTests.cs`
- `backend/tests/AnimStudio.UnitTests/Storyboard/StoryboardTests.cs` *(entity behaviour)*
- `frontend/tests/e2e/storyboard.spec.ts` *(Playwright — when e2e project exists)*

## Security review

- `specs/phase6-storyboard-security-review.md`

## Architecture notes

- `specs/phase6-storyboard-architecture.md`
