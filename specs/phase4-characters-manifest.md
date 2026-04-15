# Phase 4 — Character Studio: File Manifest

## Overview
Extends **AnimStudio.ContentModule** with Character domain entities and adds
`CharactersController` + `CharacterProgressHub` to **AnimStudio.API**.
No new project (module) is needed — Characters are content-module domain objects.

---

## [ContentModule.Domain]

| File | Description |
|------|-------------|
| `backend/src/AnimStudio.ContentModule/Domain/Entities/Character.cs` | Character aggregate with LoRA training lifecycle |
| `backend/src/AnimStudio.ContentModule/Domain/Entities/EpisodeCharacter.cs` | Many-to-many join |
| `backend/src/AnimStudio.ContentModule/Domain/Enums/TrainingStatusEnum.cs` | TrainingStatus enum |
| `backend/src/AnimStudio.ContentModule/Domain/Events/CharacterDomainEvents.cs` | CharacterCreated, TrainingProgressed, CharacterReady, CharacterFailed |

## [ContentModule.Application]

| File | Description |
|------|-------------|
| `backend/src/AnimStudio.ContentModule/Application/Interfaces/ICharacterRepository.cs` | Repository interface |
| `backend/src/AnimStudio.ContentModule/Application/DTOs/CharacterDtos.cs` | CharacterDto, PagedCharactersResponse |
| `backend/src/AnimStudio.ContentModule/Application/Commands/CreateCharacter/CreateCharacterCommand.cs` | Create + enqueue training |
| `backend/src/AnimStudio.ContentModule/Application/Commands/DeleteCharacter/DeleteCharacterCommand.cs` | Soft-delete guard |
| `backend/src/AnimStudio.ContentModule/Application/Commands/AttachCharacter/AttachCharacterCommand.cs` | Attach to episode |
| `backend/src/AnimStudio.ContentModule/Application/Commands/DetachCharacter/DetachCharacterCommand.cs` | Detach from episode |
| `backend/src/AnimStudio.ContentModule/Application/Commands/CompleteCharacterTraining/CompleteCharacterTrainingCommand.cs` | Called by Service Bus handler |
| `backend/src/AnimStudio.ContentModule/Application/Queries/GetCharacters/GetCharactersQuery.cs` | Paginated team library |
| `backend/src/AnimStudio.ContentModule/Application/Queries/GetCharacter/GetCharacterQuery.cs` | Single character |
| `backend/src/AnimStudio.ContentModule/Application/Queries/GetEpisodeCharacters/GetEpisodeCharactersQuery.cs` | Characters for episode |
| `backend/src/AnimStudio.ContentModule/Application/Services/CharacterTrainingService.cs` | SignalR broadcast logic |

## [ContentModule.Infrastructure]

| File | Description |
|------|-------------|
| `backend/src/AnimStudio.ContentModule/Infrastructure/Persistence/ContentDbContext.cs` | **Updated** — add Character + EpisodeCharacter DbSets + Fluent config |
| `backend/src/AnimStudio.ContentModule/Infrastructure/Repositories/CharacterRepository.cs` | EF Core implementation |
| `backend/src/AnimStudio.ContentModule/Migrations/20260405120000_Phase4Characters.cs` | EF migration |

## [AnimStudio.API]

| File | Description |
|------|-------------|
| `backend/src/AnimStudio.API/Controllers/CharactersController.cs` | REST endpoints |
| `backend/src/AnimStudio.API/Hubs/CharacterProgressHub.cs` | SignalR hub for training updates |

## [ContentModule — Registration update]

| File | Description |
|------|-------------|
| `backend/src/AnimStudio.ContentModule/ContentModuleRegistration.cs` | **Updated** — register ICharacterRepository |

## [Frontend]

| File | Description |
|------|-------------|
| `frontend/src/app/(dashboard)/studio/[id]/characters/page.tsx` | Character Studio page |
| `frontend/src/components/character/character-form.tsx` | Create character form |
| `frontend/src/components/character/character-card.tsx` | Character card with status badge + progress bar |
| `frontend/src/components/character/training-badge.tsx` | Animated training status badge |
| `frontend/src/hooks/use-characters.ts` | TanStack Query + SignalR subscription |
| `frontend/src/types/index.ts` | **Updated** — add CharacterDto, TrainingStatus |

## [QA]

| File | Description |
|------|-------------|
| `backend/tests/AnimStudio.UnitTests/Commands/CreateCharacterCommandHandlerTests.cs` | Unit tests |
| `backend/tests/AnimStudio.UnitTests/Commands/AttachCharacterToEpisodeCommandHandlerTests.cs` | Unit tests |
| `backend/tests/AnimStudio.UnitTests/Queries/GetCharactersQueryHandlerTests.cs` | Query tests |
| `e2e/specs/character-studio.spec.ts` | Playwright E2E tests |
| `security/phase4-characters-security-review.md` | OWASP review |
