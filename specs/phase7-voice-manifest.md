# Phase 7 — Voice Studio: File Manifest

## [ContentModule.Domain]
- `backend/src/AnimStudio.ContentModule/Domain/Entities/VoiceAssignment.cs` — VoiceAssignment aggregate root entity
- `backend/src/AnimStudio.ContentModule/Domain/Enums/BuiltInVoice.cs` — Built-in TTS voice enum (Alloy, Echo, Fable, Onyx, Nova, Shimmer)
- `backend/src/AnimStudio.ContentModule/Domain/Events/VoiceDomainEvents.cs` — Domain events for voice assignment changes

## [ContentModule.Application]
- `backend/src/AnimStudio.ContentModule/Application/DTOs/VoiceDtos.cs` — VoiceAssignmentDto, VoicePreviewResponse, VoiceCloneResponse, request records
- `backend/src/AnimStudio.ContentModule/Application/Interfaces/IVoiceAssignmentRepository.cs` — Repository interface
- `backend/src/AnimStudio.ContentModule/Application/Interfaces/IVoicePreviewService.cs` — TTS preview service interface
- `backend/src/AnimStudio.ContentModule/Application/Interfaces/IVoiceCloneService.cs` — Voice cloning service interface
- `backend/src/AnimStudio.ContentModule/Application/Commands/UpdateVoiceAssignments/UpdateVoiceAssignmentsCommand.cs` — Batch update voice assignments command + handler + validator
- `backend/src/AnimStudio.ContentModule/Application/Commands/PreviewVoice/PreviewVoiceCommand.cs` — TTS preview command + handler + validator
- `backend/src/AnimStudio.ContentModule/Application/Commands/CloneVoice/CloneVoiceCommand.cs` — Voice cloning command + handler + validator
- `backend/src/AnimStudio.ContentModule/Application/Queries/GetVoiceAssignments/GetVoiceAssignmentsQuery.cs` — Get voice assignments for episode query + handler

## [ContentModule.Infrastructure]
- `backend/src/AnimStudio.ContentModule/Infrastructure/Repositories/VoiceAssignmentRepository.cs` — EF Core repository implementation
- `backend/src/AnimStudio.ContentModule/Infrastructure/Persistence/ContentDbContext.cs` — UPDATE: Add DbSet<VoiceAssignment>
- `backend/src/AnimStudio.ContentModule/Migrations/20260418120000_Phase7Voice.cs` — EF Core migration
- `backend/src/AnimStudio.ContentModule/Migrations/20260418120000_Phase7Voice.Designer.cs` — Migration designer
- `backend/src/AnimStudio.ContentModule/Migrations/ContentDbContextModelSnapshot.cs` — UPDATE: Add VoiceAssignment snapshot
- `backend/src/AnimStudio.ContentModule/ContentModuleRegistration.cs` — UPDATE: Register IVoiceAssignmentRepository

## [API]
- `backend/src/AnimStudio.API/Controllers/VoiceController.cs` — REST controller (GET/PUT voices, POST preview, POST clone)
- `backend/src/AnimStudio.API/Services/VoicePreviewService.cs` — Azure OpenAI TTS → tmp Blob → signed URL
- `backend/src/AnimStudio.API/Services/VoiceCloneService.cs` — Stub for ElevenLabs/Azure custom voice integration
- `backend/src/AnimStudio.API/Program.cs` — UPDATE: Register IVoicePreviewService and IVoiceCloneService

## [Frontend-Config]
- `frontend/src/types/index.ts` — UPDATE: Add VoiceAssignmentDto, BuiltInVoice, VoicePreviewResponse, VoiceCloneResponse types

## [Frontend-Pages]
- `frontend/src/hooks/use-voice-assignments.ts` — TanStack Query hooks for voice CRUD + preview mutation
- `frontend/src/components/voice/voice-picker.tsx` — shadcn/ui Select with built-in voice names
- `frontend/src/components/voice/audio-preview-player.tsx` — HTML5 audio play/pause with progress bar
- `frontend/src/components/voice/language-selector.tsx` — Language dropdown with flag labels
- `frontend/src/components/voice/voice-clone-upload.tsx` — Drag-and-drop upload with tier gate
- `frontend/src/app/(dashboard)/projects/[id]/episodes/[episodeId]/voice/page.tsx` — Voice Studio page

## [Tests]
- `backend/tests/AnimStudio.UnitTests/Commands/VoiceCommandHandlerTests.cs` — Unit tests for voice command handlers
- `frontend/tests/e2e/voice.spec.ts` — Playwright e2e tests for Voice Studio
