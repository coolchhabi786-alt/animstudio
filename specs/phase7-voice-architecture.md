# Phase 7 — Voice Studio: Architecture Notes

## Overview

The Voice Studio allows users to assign voices to characters within an episode, preview TTS audio inline, and (for Studio-tier subscribers) clone custom voices from audio samples.

## Key Design Decisions

### 1. VoiceAssignment as Separate Entity (Not Embedded in Episode)
Voice assignments are stored as a separate `VoiceAssignment` table rather than JSON in the Episode entity. This enables:
- Per-character voice queries without loading the entire episode
- Independent concurrency control via RowVersion
- Clean FK relationships to both Episode and Character

### 2. Built-in Voices Enum
The 6 OpenAI TTS voices (Alloy, Echo, Fable, Onyx, Nova, Shimmer) are defined as a `BuiltInVoice` enum for type safety. However, `VoiceAssignment.VoiceName` is stored as a string to accommodate custom clone names beyond the enum set.

### 3. TTS Preview Service
- Calls Azure OpenAI TTS API (`/audio/speech`) with the selected voice and text
- Uploads the resulting audio to Azure Blob Storage in a `tmp-tts/` container with auto-delete lifecycle policy (1 hour)
- Returns a SAS-signed URL with 60-second expiry
- Uses the `http-ai-api` Polly resilience pipeline (timeout + retry + bulkhead)

### 4. Voice Cloning — Stub Architecture
Voice cloning is a future integration point:
- The `IVoiceCloneService` interface and `VoiceCloneService` stub are created now
- The stub returns a "not implemented" response with a clear error code (`CLONE_NOT_AVAILABLE`)
- Tier gate: `CloneVoiceCommand` checks the subscription tier via `ICurrentUserService` — only Studio tier is allowed
- Future implementation will integrate with ElevenLabs API or Azure Custom Neural Voice

### 5. Batch Update Pattern
Voice assignments use a batch update (PUT) rather than individual PATCH endpoints:
- The frontend sends the complete set of assignments for an episode
- The handler upserts (insert or update) each assignment
- Orphaned assignments (characters removed from episode) are soft-deleted
- This simplifies the UI — one "Save All" button instead of per-character save

### 6. Episode Pipeline Integration
- `EpisodeStatus.Voice = 5` and `JobType.Voice = 5` already exist in the codebase
- `HandleJobCompletion` already maps `JobType.Voice → EpisodeStatus.Animation`
- Voice assignment is a manual step (no job queue) — the user assigns voices and confirms
- The storyboard completion naturally leads to the Voice stage

### 7. No SignalR for Voice
Voice assignment is a synchronous operation (batch save). TTS preview is request-response. No real-time broadcasting is needed — unlike character training or shot generation, there are no long-running background processes to stream updates for.

## Azure Services Used
- **Azure OpenAI Service** — TTS API (`gpt-4o-audio` or `tts-1` model)
- **Azure Blob Storage** — Temporary TTS audio files (tmp-tts container, 1hr lifecycle)
- **Azure Key Vault** — AzureOpenAIKey, AzureOpenAIEndpoint secrets (already provisioned)

## Security Considerations
- TTS preview rate-limited to prevent abuse (tied to authenticated user rate limit tier)
- Voice clone endpoint gated to Studio subscription tier
- SAS URLs use 60-second expiry to prevent link sharing
- Audio file uploads validated for format (WAV/MP3) and size (max 10MB)
