# Phase 4 — Character Studio: Architecture Notes

## Overview
Phase 4 adds the Character Studio — a LoRA (Low-Rank Adaptation) training pipeline
that lets teams define visual characters with style guidance, then trains a personalised
model weight file. Characters are team-scoped and reusable across episodes.

---

## Architecture Decisions

### 1. Module placement — ContentModule, not a new CharacterModule
Characters are first-class content objects. Adding them to `AnimStudio.ContentModule`
avoids proliferating projects while keeping clean separation via sub-namespaces and
dedicated interfaces. The EF DbContext (`ContentDbContext`) is extended with two new
tables.

### 2. Training pipeline — Azure Service Bus + SignalR
- `POST /characters` creates a `Character` in `Draft` state, deducts credits, enqueues
  a `CharacterDesign` message to the Service Bus `character-training` queue, and returns
  `202 Accepted` with a `jobId`.
- A GPU worker (outside this repo) picks up the message, performs pose generation and
  LoRA training, then publishes completion/progress messages to the `completions` queue.
- `CompletionMessageProcessor` (existing hosted service) deserialises those messages and
  invokes `CompleteCharacterTrainingCommand` via MediatR.
- `CharacterTrainingService` updates the DB and broadcasts `CharacterTrainingUpdate` via
  `CharacterProgressHub` (SignalR group `team-{teamId}`).

### 3. Credit validation
Before queuing training, the handler checks `team.CreditsRemaining >= character.CreditsCost`.
If insufficient, it returns `Result.Failure("INSUFFICIENT_CREDITS")` → HTTP 402.
Credit deduction is immediate (not on completion) to prevent double-spend across parallel requests.

### 4. Soft-delete guard
`DELETE /characters/{id}` is blocked when any `EpisodeCharacter` row exists where the
linked Episode has a non-terminal status (not Done/Failed). The check is done in the
`DeleteCharacterCommandHandler` before calling `character.Delete()`.

### 5. EpisodeCharacter join table
`EpisodeCharacter` is a pure join (EpisodeId + CharacterId composite PK, plus `AttachedAt`
timestamp). Only `Ready` characters can be attached. The validation is enforced in
`AttachCharacterCommandHandler`.

### 6. SignalR group naming
`CharacterProgressHub` uses group `team-{teamId}` (same namespace as `ProgressHub`)
so a single frontend `JoinTeamGroup` call receives both episode progress and character
training updates. The frontend discriminates on message type.

### 7. Frontend routing
Characters page lives at `(dashboard)/studio/[id]/characters/page.tsx` where `[id]`
is the `projectId`. This keeps the URL structure `/<projectId>/characters` consistent
with the studio route group established in Phase 2.

### 8. LoRA training cost estimate
Training cost is fixed at **50 credits** per character (stored as `Character.CreditsCost`).
The `CreateCharacterRequest` POST response includes `estimatedCreditsCost` so the frontend
can display the cost before confirming.

---

## Service Bus Message Contracts

### Outbound (AnimStudio → Worker)
```json
{
  "messageType": "CharacterDesign",
  "characterId": "<guid>",
  "teamId": "<guid>",
  "name": "Professor Whiskerbolt",
  "description": "...",
  "styleDna": "anime, vibrant, 2D flat",
  "creditsCost": 50
}
```

### Inbound (Worker → AnimStudio completions queue)
```json
{
  "messageType": "CharacterTrainingProgress",
  "characterId": "<guid>",
  "teamId": "<guid>",
  "status": "Training",
  "progressPercent": 45,
  "imageUrl": null,
  "loraWeightsUrl": null,
  "triggerWord": null
}
```
```json
{
  "messageType": "CharacterTrainingComplete",
  "characterId": "<guid>",
  "teamId": "<guid>",
  "status": "Ready",
  "progressPercent": 100,
  "imageUrl": "https://cdn.animstudio.io/...",
  "loraWeightsUrl": "https://blobs.animstudio.io/...",
  "triggerWord": "PROF_WHISKERBOLT"
}
```

---

## Database Schema

- Schema: `content.*` (existing ContentModule schema)
- New tables: `content.Characters`, `content.EpisodeCharacters`
- Migration: `Phase4Characters`

```sql
CREATE TABLE content.Characters (
    Id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    TeamId          UNIQUEIDENTIFIER NOT NULL,
    Name            NVARCHAR(200)    NOT NULL,
    Description     NVARCHAR(2000)   NULL,
    StyleDna        NVARCHAR(4000)   NULL,
    ImageUrl        NVARCHAR(2048)   NULL,
    LoraWeightsUrl  NVARCHAR(2048)   NULL,
    TriggerWord     NVARCHAR(100)    NULL,
    TrainingStatus  NVARCHAR(30)     NOT NULL DEFAULT 'Draft',
    TrainingProgressPercent INT      NOT NULL DEFAULT 0,
    CreditsCost     INT              NOT NULL DEFAULT 50,
    CreatedAt       DATETIMEOFFSET   NOT NULL,
    UpdatedAt       DATETIMEOFFSET   NOT NULL,
    IsDeleted       BIT              NOT NULL DEFAULT 0,
    DeletedAt       DATETIMEOFFSET   NULL,
    DeletedByUserId UNIQUEIDENTIFIER NULL,
    RowVersion      ROWVERSION       NOT NULL,
    INDEX IX_Characters_TeamId (TeamId)
);

CREATE TABLE content.EpisodeCharacters (
    EpisodeId   UNIQUEIDENTIFIER NOT NULL,
    CharacterId UNIQUEIDENTIFIER NOT NULL,
    AttachedAt  DATETIMEOFFSET   NOT NULL,
    CONSTRAINT PK_EpisodeCharacters PRIMARY KEY (EpisodeId, CharacterId),
    CONSTRAINT FK_EpisodeCharacters_Episodes   FOREIGN KEY (EpisodeId)   REFERENCES content.Episodes(Id),
    CONSTRAINT FK_EpisodeCharacters_Characters FOREIGN KEY (CharacterId) REFERENCES content.Characters(Id)
);
```

---

## OWASP Considerations
- **Broken Object Level Authorization**: every character read/write validates that `character.TeamId == currentUser.TeamId`.
- **Injection**: parameterised EF Core queries only; no raw SQL.
- **Insecure Direct Object References**: `GetCharacterQuery` enforces team ownership.
- **Credit manipulation**: credit check is atomic with character creation in a DB transaction.
