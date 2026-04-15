# Phase 5 — Script Workshop: Architecture Notes

## Overview
Phase 5 adds the Script Workshop — an AI-powered screenplay generation and manual
editing workflow. Users can generate scripts from trained characters, manually edit
dialogue and scenes, and regenerate with director's notes for iterative refinement.

---

## Architecture Decisions

### 1. Module placement — ContentModule, not a new ScriptModule
Scripts are first-class content objects tightly coupled to Episodes and Characters.
Adding them to `AnimStudio.ContentModule` maintains the existing domain boundary
while keeping clean separation via sub-namespaces. The `ContentDbContext` is extended
with one new table (`content.Scripts`).

### 2. One script per episode
A unique index on `EpisodeId` enforces the one-to-one relationship. The workflow
supports overwriting: regeneration replaces the existing script entity rather than
creating a new one. This simplifies the downstream pipeline (storyboard, animation)
which always references "the script" without version disambiguating.

### 3. Data model — JSON blob vs. normalised tables
The screenplay structure (`title → scenes → dialogue lines`) is stored as a JSON
blob (`RawJson` nvarchar(max)) rather than normalised into Scene and DialogueLine
tables. Rationale:
- **Schema flexibility**: The Python ScriptwritingCrew may evolve its output schema
  (e.g. add `stage_direction`, `camera_angle`) without requiring EF Core migrations.
- **Atomic read/write**: The entire screenplay is always loaded/saved as a unit —
  there are no use cases for querying individual dialogue lines via SQL.
- **Python ↔ C# parity**: The JSON maps 1:1 to the `Screenplay` Pydantic model in
  `models.py`, avoiding translation layers.
- **Tradeoff**: No SQL-level querying of scene/dialogue fields. This is acceptable
  because script search/filtering is not a product requirement.

### 4. Script generation — Job pipeline
- `POST /episodes/{id}/script` validates at least one attached character has
  `TrainingStatus.Ready`, then creates a `Job` entity (type=`Script`), enqueues it
  to the Azure Service Bus `jobs` queue, and returns `202 Accepted` with the job ID.
- The Python `ScriptwritingCrew` (outside this repo) picks up the job, generates the
  screenplay JSON, and publishes a completion message to the `completions` queue.
- The existing `CompletionMessageProcessor` hosted service receives the completion
  and invokes `HandleJobCompletionCommand`, which calls `script.UpdateFromJob()` and
  persists the result.

### 5. Job payload structure
The job payload sent to Service Bus includes:
```json
{
  "episodeId": "uuid",
  "directorNotes": "optional string",
  "characters": [{ "name": "...", "styleDna": "..." }],
  "isRegeneration": false,
  "attempt": 1
}
```
The `characters` array provides the Python engine with character names and style DNA
to generate contextually accurate dialogue. The `isRegeneration` flag lets the engine
know it can reference the previous script for continuity.

### 6. Director notes — iterative refinement
Director notes are free-text guidance (max 5000 chars) stored on the `Script` entity.
When the user clicks "Regenerate":
1. The existing `Script.DirectorNotes` is updated via `SetDirectorNotes()`.
2. A new `Job` is created with the notes in the payload.
3. The Python engine receives the notes and adjusts its generation accordingly.
This enables iterative refinement without losing the original script state (which
remains in `RawJson` until the new job completes and calls `UpdateFromJob()`).

### 7. Character validation on manual edits
When a user saves manual edits via `PUT /episodes/{id}/script`, the handler validates
that every `character` field in every `dialogue` line matches a character name in the
episode's roster (`EpisodeCharacter` join table). Comparison is case-insensitive. If
unknown characters are found, the request is rejected with error code `INVALID_CHARACTERS`.

### 8. IsManuallyEdited flag
The `IsManuallyEdited` boolean distinguishes between AI-generated and human-edited scripts:
- Set to `true` by `SaveManualEdits()` (PUT endpoint).
- Reset to `false` by `UpdateFromJob()` (AI regeneration completion).
The frontend displays a "Manually edited" badge and can use this flag to warn users
before regenerating (which would overwrite their edits).

### 9. Frontend routing
The Script Workshop page lives at:
```
(dashboard)/projects/[id]/episodes/[episodeId]/script/page.tsx
```
This follows the established URL structure where `[id]` is the project ID and
`[episodeId]` is the episode ID, consistent with the episode pipeline flow.

### 10. Frontend state management
- **Server state**: TanStack Query manages all script data via `use-script.ts` hooks.
  `useScript()` fetches the current script; mutations handle generate/save/regenerate.
- **Local state**: Edit mode uses React `useState` for `editedScenes` — a local copy
  of the screenplay scenes that the user can modify. On save, the local state is sent
  to the PUT endpoint. On cancel, it is discarded without a network call.

### 11. Emotional tone badges
Scene cards display the emotional tone as a colour-coded badge:
| Tone         | Colour  |
|--------------|---------|
| Happy        | Yellow  |
| Sad          | Blue    |
| Suspenseful  | Purple  |
| Funny        | Orange  |
| Dramatic     | Red     |
| Calm         | Green   |
| Other        | Grey    |

### 12. Duration estimation
`script-stats.tsx` calculates estimated duration by summing the `endTime` of the last
dialogue line in each scene. This is a rough estimate — the actual animation duration
will be determined in the storyboarding and animation phases.

---

## Data Flow

```
User clicks "Generate"
  → POST /episodes/{id}/script
  → GenerateScriptCommand → Job.Create(Script) → Service Bus → 202 Accepted

Python ScriptwritingCrew
  → Generates screenplay JSON
  → Publishes completion to Service Bus completions queue

CompletionMessageProcessor
  → HandleJobCompletionCommand → Script.UpdateFromJob(rawJson, title)
  → Script saved to DB → ScriptUpdatedEvent raised

User opens Script Workshop
  → GET /episodes/{id}/script
  → GetScriptQuery → Script.RawJson → deserialise → ScriptDto → 200 OK

User edits and saves
  → PUT /episodes/{id}/script with modified ScreenplayDto
  → SaveScriptCommand → validate characters → Script.SaveManualEdits(rawJson)
  → Script saved → ScriptManuallyEditedEvent raised → 200 OK

User regenerates with notes
  → POST /episodes/{id}/script/regenerate with director notes
  → RegenerateScriptCommand → Script.SetDirectorNotes() → Job.Create(Script)
  → Service Bus → 202 Accepted
```

---

## Security Considerations
- All Script endpoints require `[Authorize(Policy = "RequireTeamMember")]`.
- Episode ownership is validated by the MediatR handler (episode must belong to the team).
- Character names in manual edits are validated against the roster to prevent injection
  of arbitrary strings into downstream AI pipelines.
- Director notes are max 5000 chars to prevent payload abuse.
- RawJson is treated as opaque storage; deserialization uses `System.Text.Json` with
  strict options (no type handling, no comments) to prevent deserialization attacks.
