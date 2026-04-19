# Phase 8 — Animation Studio Architecture

## Scope
Animate approved storyboards into video clips via a configurable backend
(cloud **Kling** or on-prem **Local**), gate the spend with an explicit
per-episode cost-approval, and stream clips back to the studio UI as they
render.

## Bounded context
Animation lives inside `ContentModule`. It references `StoryboardShot` by FK
but owns its own lifecycle aggregates (`AnimationJob`, `AnimationClip`). No
cross-module changes; no new DbContext.

## Aggregates

### `AnimationJob` (aggregate root)
Tracks the **cost + approval lifecycle** of an animation run for an episode.

| Field               | Type                    | Notes                                                  |
|---------------------|-------------------------|--------------------------------------------------------|
| Id                  | Guid                    | PK                                                     |
| EpisodeId           | Guid                    | FK, indexed                                            |
| Backend             | AnimationBackend        | Kling \| Local                                         |
| EstimatedCostUsd    | decimal(10,4)           | Frozen at approval time                                |
| ActualCostUsd       | decimal(10,4)?          | Populated on completion                                |
| ApprovedByUserId    | Guid?                   | Audit                                                  |
| ApprovedAt          | DateTime?               |                                                        |
| Status              | AnimationStatus         | PendingApproval → Approved → Running → Completed/Failed |
| CreatedAt/UpdatedAt | DateTime                |                                                        |
| RowVersion          | byte[]                  | Concurrency                                            |
| IsDeleted           | bool                    | Soft delete                                            |

Raises: `AnimationJobApprovedEvent`, `AnimationJobCompletedEvent`.

### `AnimationClip` (aggregate root)
One row per rendered (or in-flight) clip. Owned by the episode; linked to the
source `StoryboardShot` when available.

Unique index: `(EpisodeId, SceneNumber, ShotIndex)` — idempotent creation
keyed off the storyboard grid. FK to `StoryboardShots` uses `OnDelete(SetNull)`
so deleting a storyboard shot does not delete already-rendered footage.

Raises: `AnimationClipReadyEvent` when `Status` transitions to `Ready`.

## Cost model

```
unitCostUsd(Kling)  = 0.056
unitCostUsd(Local)  = 0.000
totalCostUsd        = shotCount × unitCostUsd
```

`shotCount` is the number of `StoryboardShot` rows that belong to the
storyboard for the episode (no branching, no variants in Phase 8).

Rates live in configuration (`Animation:Rates:Kling`, `Animation:Rates:Local`)
so finance can rotate without a deploy, but default to the values above.

## Endpoints (AnimationController, `/api/v1`)

| Verb | Route                                             | Purpose                           |
|------|---------------------------------------------------|-----------------------------------|
| GET  | `/episodes/{id}/animation/estimate?backend=…`     | Itemised cost breakdown           |
| POST | `/episodes/{id}/animation`                        | Approve + enqueue job (body: backend) |
| GET  | `/episodes/{id}/animation`                        | List clips with status            |
| GET  | `/episodes/{id}/animation/clips/{clipId}`         | Signed Blob URL for one clip      |

All are gated by `[Authorize(Policy = "RequireTeamMember")]` and the
`authenticated` rate-limit policy.

## Orchestration

1. `POST /episodes/{id}/animation` persists a new `AnimationJob` in
   `Approved` state (we collapse PendingApproval+Approved into one atomic step
   because the estimate endpoint is the approval ceremony on the client), sets
   `Episode.Status = Animation`, and enqueues a `JobType.Animation` via the
   existing `Job` aggregate. The transition `Animation → PostProduction` is
   already wired in `HandleJobCompletionCommand`.
2. The worker (existing Hangfire pipeline) renders each shot, persists an
   `AnimationClip` row, and publishes `AnimationClipReadyEvent`.
3. `SignalRAnimationClipNotifier` (MediatR handler) broadcasts `ClipReady` on
   the existing `ProgressHub` to group `team:{teamId}` — consistent with Phase
   6.
4. When all clips are `Ready` (or the job times out), the worker updates
   `AnimationJob.Status` and `ActualCostUsd`.

## SignalR contract

Reuses `ProgressHub` (no new hub). Event name: `ClipReady`. Payload:

```ts
{
  episodeId: string;
  sceneNumber: number;
  shotIndex: number;
  clipId: string;
  clipUrl: string;      // signed SAS URL, 60s TTL
}
```

## Clip delivery

`ClipUrl` on the row is the blob path (`clips/{episodeId}/{clipId}.mp4`). The
signed URL endpoint issues a 60-second SAS token at request time — same TTL
and pattern used for storyboard frames in Phase 6.

## Frontend shape

- Studio route: `projects/[id]/episodes/[episodeId]/animation/page.tsx`.
- `CostEstimateCard` — backend radio (Kling/Local), shot breakdown table, total.
- `ApprovalDialog` — shows total cost, shot count, backend; confirm triggers
  the approval mutation.
- `ClipPlayer` — HTML5 `<video>` with loop toggle, fed by signed URL.
- `use-animation` — React Query estimate+clips + mutation, subscribes to
  `ClipReady` via the shared SignalR connection and invalidates the clips
  query.

## Decisions & alternatives considered

- **Separate `AnimationJob` vs reuse `Job`.** Keep both: `Job` still tracks
  engine progress, `AnimationJob` tracks the cost/approval lifecycle that the
  generic job row cannot express.
- **Dedicated hub vs reuse `ProgressHub`.** Reuse, matching Phase 6. One fewer
  connection per client.
- **Bake rates into code vs config.** Config — finance changes quarterly.
- **Atomic approve-and-enqueue vs two-step.** One step: approval implies
  intent; the estimate GET is the "review" stage. A separate pending state
  would add UX without value.

## Non-goals (Phase 8)
- Retrying individual failed clips from the UI (reuse episode regenerate).
- Variant/branching shots.
- Frame-accurate edits — handled in Phase 9 Post-Production.
