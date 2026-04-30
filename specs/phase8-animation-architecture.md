# Phase 8 — Animation Studio Architecture

## Scope
Animate approved storyboards into video clips via a configurable backend
(cloud **Kling** or on-prem **Local**), gate the spend with an explicit
user approval step, and stream per-clip completion events to the browser
via SignalR.

---

## Storage Strategy: Local vs Azure Blob

All file URL generation flows through **`IFileStorageService`** (defined in
`AnimStudio.ContentModule/Application/Interfaces/IFileStorageService.cs`).

| Config `FileStorage:Provider` | Implementation | Clip URL pattern |
|---|---|---|
| `Local` (dev default) | `LocalFileStorageService` | `http://localhost:5001/api/v1/files/{relativePath}` |
| `AzureBlob` (prod) | `AzureBlobFileStorageService` | SAS URL with 1-hour TTL |

Switch is done at startup in `Program.cs` — no code changes required.

**Local file-serving** (`FileStorageController`):
- Route: `GET /api/v1/files/{**filePath}`
- `[AllowAnonymous]` — `<video>` tags can't send auth headers
- Path traversal guard: resolved path must be inside `LocalRootPath`
- `PhysicalFile(..., enableRangeProcessing: true)` for video seeking

---

## Data Flow

```
User clicks "Approve & Process"
    │
    ▼
POST /api/v1/episodes/{id}/animation
    │
    ├─ ApproveAnimationCommand
    │    ├─ Create AnimationJob (Approved)
    │    ├─ Seed AnimationClips (Pending, one per storyboard shot)
    │    └─ Create generic Job row (JobType.Animation)
    │
    └─ AnimationController enqueues Hangfire job
         │
         ▼
    AnimationJobHangfireProcessor.ProcessAsync(animationJobId)
         │
         ├─ [Local backend]
         │    ├─ MarkRunning on AnimationJob
         │    ├─ For each AnimationClip:
         │    │    ├─ Resolve file: {LocalRootPath}/animation/{subfolder}/scene_{sc}_shot_{sh}.mp4
         │    │    ├─ Found → MarkReady(relativePath, estimatedDuration)
         │    │    └─ Not found → MarkFailed()
         │    ├─ Publish AnimationClipReadyEvent (domain event)
         │    └─ MarkCompleted / MarkFailed on AnimationJob
         │
         └─ [Kling backend]
              └─ Stub log — clips stay Pending until Python webhook fires
                   │
                   ▼ (future)
              POST /api/v1/jobs/{id}/complete
                   │
                   ▼
              HandleJobCompletionCommand (JobType.Animation)
                   └─ Parse Python result JSON → MarkReady per clip → domain events

    AnimationClipReadyEvent (domain event)
         │
         ▼
    AnimationClipReadyEventHandler (MediatR INotificationHandler)
         │
         └─ IAnimationClipNotifier.PublishClipReadyAsync(teamId, ...)
              │
              ▼
         SignalRAnimationClipNotifier
              └─ hubContext.Clients.Group("team:{teamId}").SendAsync("ClipReady", payload)
                   │
                   ▼
              Frontend: useAnimationRealtime() hook
                   └─ Updates TanStack Query clips cache → UI re-renders
```

---

## Key Design Decisions

### 1. Hangfire over Service Bus for Local backend
The Python pipeline dispatches via Azure Service Bus (unavailable in local dev).
Hangfire provides a SQL-backed queue that works without Azure credentials,
enabling end-to-end animation testing locally.

### 2. IFileStorageService for all URL generation
`GetAnimationClipSignedUrlQuery` and `GetAnimationClipsQuery` both use
`IFileStorageService.GetFileUrl()` rather than `IClipUrlSigner`. This means:
- Local: permanent `/api/v1/files/...` URLs, no TTL
- Azure Blob: SAS URL with 1-hour TTL embedded by `AzureBlobFileStorageService`
- `IClipUrlSigner` is kept only for `BlobClipUrlSigner` (legacy, used by voice preview SAS)

### 3. Domain event → SignalR bridge
`AnimationClipReadyEvent` is a domain event raised by `AnimationClip.MarkReady()`.
`AnimationClipReadyEventHandler` (Application layer, injected with `IAnimationClipNotifier`)
bridges to SignalR. This keeps the domain model infrastructure-agnostic.

### 4. teamId resolution
`AnimationClipReadyEventHandler` walks `Episode.ProjectId → Project.TeamId` to
determine the SignalR group. Episode does not carry TeamId directly (to avoid
denormalization). Both repositories are in scope (same DbContext scope, no extra I/O).

### 5. Duration estimation without FFprobe
The Hangfire local processor estimates clip duration from file size (~500KB/s)
as a temporary measure. Accurate duration is set when:
- Python pipeline delivers results via the webhook (Kling/remote)
- Future: `ffprobe` integration or metadata sidecar file

### 6. Idempotent dev seeder
`SeedDevContentAsync` in `Program.cs` seeds an `AnimationJob` (Completed, Local)
and 8 `AnimationClips` (Ready) pointing to existing `.mp4` files in
`animation/23MarAnimation/`. These are idempotent (`IF NOT EXISTS`) so they
survive server restarts without duplicating data.

---

## Configuration

### appsettings.Development.json
```json
{
  "FileStorage": {
    "Provider": "Local",
    "LocalRootPath": "C:\\Users\\Vaibhav\\cartoon_automation\\output",
    "BackendBaseUrl": "http://localhost:5001"
  },
  "Animation": {
    "LocalSubfolder": "23MarAnimation",
    "Rates": {
      "Kling": "0.056",
      "Local": "0"
    }
  }
}
```

### appsettings.json (production defaults)
```json
{
  "FileStorage": {
    "Provider": "AzureBlob",
    "LocalRootPath": "",
    "BackendBaseUrl": "https://api.animstudio.ai"
  }
}
```

---

## SignalR Contract

**Event name**: `ClipReady`  
**Group**: `team:{teamId}`  
**Payload**:
```json
{
  "episodeId": "guid",
  "clipId": "guid",
  "sceneNumber": 1,
  "shotIndex": 1,
  "clipUrl": "http://localhost:5001/api/v1/files/animation/23MarAnimation/scene_01_shot_01.mp4"
}
```

Frontend `useAnimationRealtime()` hook patches the TanStack Query clips cache
directly so the UI updates without a full refetch.

---

## Python Webhook Contract (Kling backend)

When Python finishes rendering, it calls:
```
POST /api/v1/jobs/{jobId}/complete
{
  "isSuccess": true,
  "result": "{\"clips\":[{\"sceneNumber\":1,\"shotIndex\":1,\"clipUrl\":\"animation/.../scene_01_shot_01.mp4\",\"durationSeconds\":4.5}],\"actualCostUsd\":0.448}"
}
```

`HandleJobCompletionCommand` (extended in Phase 8) deserializes the result JSON
and calls `AnimationClip.MarkReady()` per clip, publishing domain events that
flow to SignalR via `AnimationClipReadyEventHandler`.

---

## Security

- `[Authorize(Policy = "RequireTeamMember")]` on all animation endpoints
- `[AllowAnonymous]` on `FileStorageController` (browser media elements)
- Path traversal prevention: resolved path must start with `LocalRootPath`
- No secrets in clip URLs for Local provider (filesystem, not blob SAS)
- Azure Blob SAS URLs: 1-hour TTL, read-only scope
