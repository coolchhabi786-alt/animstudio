# Phase 5 — Script Workshop: File Manifest

## Overview
Extends **AnimStudio.ContentModule** with Script domain entity and adds
`ScriptController` to **AnimStudio.API**. No new project (module) is needed —
Scripts are content-module domain objects, one per episode.

---

## [ContentModule.Domain]

| File | Description |
|------|-------------|
| `backend/src/AnimStudio.ContentModule/Domain/Entities/Script.cs` | Script aggregate with Create, UpdateFromJob, SaveManualEdits, SetDirectorNotes |
| `backend/src/AnimStudio.ContentModule/Domain/Events/ScriptDomainEvents.cs` | ScriptGeneratedEvent, ScriptUpdatedEvent, ScriptManuallyEditedEvent |

## [ContentModule.Application]

| File | Description |
|------|-------------|
| `backend/src/AnimStudio.ContentModule/Application/Interfaces/IScriptRepository.cs` | Repository interface for Script entity |
| `backend/src/AnimStudio.ContentModule/Application/DTOs/ScriptDtos.cs` | DialogueLineDto, SceneDto, ScreenplayDto, ScriptDto, request bodies |
| `backend/src/AnimStudio.ContentModule/Application/Commands/GenerateScript/GenerateScriptCommand.cs` | Create + enqueue Script job (validates Ready characters) |
| `backend/src/AnimStudio.ContentModule/Application/Commands/SaveScript/SaveScriptCommand.cs` | Save manual edits (validates character roster) |
| `backend/src/AnimStudio.ContentModule/Application/Commands/RegenerateScript/RegenerateScriptCommand.cs` | Re-enqueue with director notes |
| `backend/src/AnimStudio.ContentModule/Application/Queries/GetScript/GetScriptQuery.cs` | Get script by episode ID, deserialise RawJson → ScreenplayDto |

## [ContentModule.Infrastructure]

| File | Description |
|------|-------------|
| `backend/src/AnimStudio.ContentModule/Infrastructure/Persistence/ContentDbContext.cs` | **Updated** — add Script DbSet + Fluent API config (unique IX on EpisodeId) |
| `backend/src/AnimStudio.ContentModule/Infrastructure/Repositories/ScriptRepository.cs` | EF Core implementation of IScriptRepository |
| `backend/src/AnimStudio.ContentModule/Migrations/20260410120000_Phase5Script.cs` | EF migration creating content.Scripts table |

## [ContentModule — Registration update]

| File | Description |
|------|-------------|
| `backend/src/AnimStudio.ContentModule/ContentModuleRegistration.cs` | **Updated** — register IScriptRepository → ScriptRepository |

## [AnimStudio.API]

| File | Description |
|------|-------------|
| `backend/src/AnimStudio.API/Controllers/ScriptController.cs` | REST endpoints: POST generate, GET, PUT save, POST regenerate |

## [Frontend — Types]

| File | Description |
|------|-------------|
| `frontend/src/types/index.ts` | **Updated** — DialogueLineDto, SceneDto, ScreenplayDto, ScriptDto interfaces |

## [Frontend — Hooks]

| File | Description |
|------|-------------|
| `frontend/src/hooks/use-script.ts` | TanStack Query: useScript, useGenerateScript, useSaveScript, useRegenerateScript |

## [Frontend — Components]

| File | Description |
|------|-------------|
| `frontend/src/components/script/scene-card.tsx` | Expandable scene card with tone badge, visual description, dialogue table |
| `frontend/src/components/script/dialogue-row.tsx` | Character selector dropdown, editable text area, timing inputs |
| `frontend/src/components/script/regenerate-dialog.tsx` | Modal dialog with director notes textarea and confirm button |
| `frontend/src/components/script/script-stats.tsx` | Scene count, dialogue count, estimated duration display |

## [Frontend — Pages]

| File | Description |
|------|-------------|
| `frontend/src/app/(dashboard)/projects/[id]/episodes/[episodeId]/script/page.tsx` | Script Workshop page — generate, edit, regenerate workflow |

## [Tests]

| File | Description |
|------|-------------|
| `backend/tests/AnimStudio.UnitTests/Commands/ScriptCommandHandlerTests.cs` | xUnit tests for GenerateScript and SaveScript handlers |
| `backend/tests/AnimStudio.UnitTests/Queries/GetScriptQueryHandlerTests.cs` | xUnit tests for GetScript query handler |
| `backend/tests/AnimStudio.UnitTests/Infrastructure/ScriptRepositoryTests.cs` | Integration tests for ScriptRepository CRUD |
| `e2e/specs/script-workshop.spec.ts` | Playwright E2E tests for Script Workshop page |

## [Specs]

| File | Description |
|------|-------------|
| `specs/openapi/phase5-script-api.yaml` | OpenAPI 3.1 spec with all Script endpoints, schemas, and examples |
| `specs/database/phase5-script-entities.cs` | EF Core C# entity spec with annotations and column types |
| `specs/phase5-script-manifest.md` | This file — complete file inventory |
| `specs/phase5-script-architecture.md` | Architecture notes: data flow, design decisions, security |
| `specs/phase5-script-security-review.md` | OWASP Top 10 security review |
