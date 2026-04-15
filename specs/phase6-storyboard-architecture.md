# Phase 6 — Storyboard Studio — Architecture

## Goal
Turn an approved screenplay into a navigable shot grid: up to 5 shots per
scene, each with a generated image, description, regeneration action, and
per-shot style override. Expose real-time updates over SignalR so the
director sees new shot thumbnails appear as the Python engine completes them.

## Flow of control

```
Frontend                API (.NET 8)              Python engine (Service Bus)
────────                ────────────              ───────────────────────────
POST /storyboard  ───►  GenerateStoryboardCommand
                        │ validate: Script ready
                        │ enqueue Job(StoryboardPlan)
                        │ advance Episode → Storyboard
                        │ Service Bus → jobs-queue
                        ▼                          ┌── StoryboardPlanCrew
                        202 JobDto                 │   → returns StoryboardPlan JSON
                                                   │
                        Service Bus ──completion──►│
                        HandleJobCompletionCommand │
                        │ persist Storyboard +     │
                        │   Shots rows              │
                        │ enqueue Job(StoryboardGen)│
                        │ ▼                         ├── StoryboardGenCrew (per-shot)
                        │                           │   → returns shot image URL
                        │◄──────────── completion───┘
                        │ update Shot.ImageUrl
                        │ broadcast ShotUpdated to team:{teamId}
                        ▼
SignalR ShotUpdated ◄── IHubContext<ProgressHub>
```

## Data model

Two new tables in `content.*`:

- `content.Storyboards` — one row per episode (unique EpisodeId index).
- `content.StoryboardShots` — up to 5 per scene; unique
  (StoryboardId, SceneNumber, ShotIndex) index prevents dup shots.

`RowVersion` on both tables provides optimistic concurrency for concurrent
regeneration calls.

## Domain decisions

- **Storyboard is an aggregate**, StoryboardShot is an **entity owned by it**.
  All shot mutations go through the aggregate (`storyboard.IncrementShotRegeneration(shotId)`)
  to keep invariants in one place and to raise the right domain events.
- **Warning-not-error on regeneration > 3** — the operation still succeeds
  (we return 202) but the Handler attaches a `REGEN_LIMIT_WARNING` code to the
  JobDto payload so the frontend can show a "credits will apply" toast.
- **Script prerequisite** — storyboarding requires a completed script,
  mirroring the "characters must be Ready" check in Phase 5.
- **StyleOverride is persisted on the shot** (not on the job) so the override
  survives re-regeneration and can be inspected by the UI.

## SignalR contract

- Hub: **existing** `ProgressHub` at `/hubs/progress`.
- Group: `team:{teamId}` (already used by character training).
- Event name: `ShotUpdated`
- Payload:
  ```json
  {
    "shotId": "uuid",
    "storyboardId": "uuid",
    "episodeId": "uuid",
    "imageUrl": "https://...",
    "regenerationCount": 2
  }
  ```
- Adapter: `AnimStudio.API.Services.SignalRStoryboardShotNotifier` implements
  the `IStoryboardShotNotifier` port defined in
  `AnimStudio.ContentModule.Application.Interfaces`.

We reuse `ProgressHub` rather than adding a third hub — the frontend already
joins `team:{teamId}` and distinguishing events by method name is cheap.

## Service Bus contract

Payload added to existing `jobs-queue`:

- `JobType.StoryboardPlan` — input: `{ episodeId, directorNotes?, attempt }`
- `JobType.StoryboardGen`  — input: `{ episodeId, storyboardId, shotId, sceneNumber, shotIndex, prompt, styleOverride?, attempt }`

The Python engine's `service_bus_listener` already dispatches these — no
backend contract change required on the Python side.

## Authorisation

All endpoints require `RequireTeamMember` policy.
BOLA: the controller/handler resolves the storyboard via `EpisodeId`, and the
Episode repository already scopes by team (soft-delete filter). For the
shot-level endpoints we fetch the Shot → Storyboard → Episode → TeamId
and compare to `ICurrentUserService.GetCurrentTeamId()`.

## Credits / rate-limiting

Regeneration increments a counter (`RegenerationCount`) and we warn at > 3.
A hard limit ties into Phase 1's `PlanDto.MaxRegenerations` in a future phase
— for Phase 6 we only warn.

## Rollout plan

1. Deploy the backend with the new migration (additive — no drops).
2. Warm-deploy the frontend with the new route guarded behind the
   `isEpisodeStatus >= Script` server check (handled implicitly by the API
   returning 400 `SCRIPT_NOT_READY`).
3. Python engine already handles `StoryboardPlan` / `StoryboardGen` commands
   from Phase 1 plumbing.
