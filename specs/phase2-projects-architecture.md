# Phase 2: Architecture Notes

## Azure Service Choices
### Database
- Azure SQL Database: All content-related entities stored using `content.*` schema.
- Shared kernel data like episode saga states stored in `shared.*` schema.
- Optimistic concurrency setup with row versioning on `Episode` entity.

### Service Bus
- Azure Service Bus: Used for job dispatch and completion tracking.
- Namespace contains two queues:
  - `completions`: For listening to job completion notifications.
  - `deadletter-retry`: For processing failed messages and retrying logic.
- Idempotency Strategy: Use `MessageId = "{{episodeId}}-{{jobType}}-{{attempt}}"` to enforce retries without duplication on Python render engine.

### SignalR
- Azure SignalR Service: Real-time notifications for episode progress:
  - Group-per-episode (`episode-{{episodeId}}`).
  - Group-per-team (`team-{{teamId}}`) for Phase 4.

## Authentication Flow
- ASP.NET Core Identity extended to include team membership validation.
- Bearer token authorization with [Authorize] and scoped [RequireTeamMember] or [RequireTeamEditor] attributes.
- Frontend integrates authentication middleware using `auth.ts` helpers.

## Service Bus Message Contracts
### `JobQueuedEvent`
```json
{
  "episodeId": "GUID",
  "jobType": "JobType",
  "attempt": 1,
  "payload": {
    ...
  }
}
```
### `JobCompletedEvent`
```json
{
  "episodeId": "GUID",
  "jobType": "JobType",
  "attempt": 1,
  "result": {
    ...
  }
}
```

## Stripe Integration Patterns
- Episode dispatch enforces subscription tier limits dynamically via middleware gate (`SubscriptionGate`).
- Stripe's metered billing features track workflow credits consumed per team.
  - `usage meter` on dashboard visualizes tier data.

## Key Design Decisions
### Optimistic Concurrency
- Concurrency token configured at the entity level (`Episode`) ensures no overlapping job dispatch race conditions.

### Outbox Pattern
- Outbox storage in SQL ensures reliable Service Bus event ingestion.
- Workflow job synchronizes with Service Bus within 1-minute SLA.

### Frontend Optimizations
- TanStack Query + SignalR subscription models ensure stale-free, highly responsive UI.
- Side drawer for episode creation avoids excessive page transitions.

### Pipeline Saga
- Pipeline stages modeled using shared enum.
- Saga states updates atomic within job context.
- Compensation logic handles error recovery transparently.

### Deadletter Management
- Retry processor scans for DLQ messages and retries or escalates if `RetryCount` threshold exceeded.

### Data Cleanup Policies
- Soft delete approach avoids immediate data loss.
- Backend implements conditional filters to ensure paginated views exclude deleted items (`IsDeleted`).