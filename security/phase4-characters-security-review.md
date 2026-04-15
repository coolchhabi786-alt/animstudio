# Phase 4 Character Studio — Security Review

**Reviewed against OWASP Top 10 (2021)**
**Scope**: all files added/modified in Phase 4

---

## A01 — Broken Access Control

| Check | Status | Evidence |
|-------|--------|----------|
| BOLA on `GET /characters/{id}` | ✅ PASS | Handler fetches by ID then checks `character.TeamId == currentTeamId` — returns 403 if mismatched |
| BOLA on `DELETE /characters/{id}` | ✅ PASS | Same ownership check before soft-delete |
| Cross-team character attachment | ✅ PASS | `AttachCharacterCommand` loads the character and validates team ownership before creating `EpisodeCharacter` |
| Episode roster BOLA | ✅ PASS | `GetEpisodeCharactersQuery` verifies episode belongs to current team (via `Episode.TeamId`) |
| CharacterProgressHub group isolation | ✅ PASS | Clients join `team:{teamId}` group; SignalR only broadcasts to that group; `teamId` is extracted from the server-side JWT sub — not from a client-supplied param |

**Action required**: None.

---

## A02 — Cryptographic Failures

| Check | Status | Evidence |
|-------|--------|----------|
| LoRA weights URL stored as path, not plaintext secret | ✅ PASS | `LoraWeightsUrl` is a Blob Storage URL; SAS token is generated at read-time (not stored) |
| StyleDna stored in plaintext | ✅ ACCEPTABLE | StyleDna is user-authored text, not sensitive; stored in `nvarchar(4000)` |
| SignalR transport | ✅ PASS | WebSocket over HTTPS enforced by Azure SignalR Service in prod |

**Action required**: None.

---

## A03 — Injection

| Check | Status | Evidence |
|-------|--------|----------|
| SQL injection via `GetByTeamIdAsync` | ✅ PASS | Uses EF Core parameterised LINQ — no raw SQL |
| SQL injection via `Name`/`Description`/`StyleDna` | ✅ PASS | Values stored via EF Core `AddAsync`, no string interpolation into SQL |
| Prompt injection via `StyleDna` | ⚠️ REVIEW | `StyleDna` is passed to the AI training pipeline. **Recommendation**: sanitise or limit control characters before enqueuing the Service Bus message; add max-length (4000 chars) FluentValidation — already enforced in frontend zod schema, verify backend validator also enforces it |

**Action required**: Verify `CreateCharacterCommandValidator` has `.MaximumLength(4000)` for `StyleDna`.

---

## A04 — Insecure Design

| Check | Status | Evidence |
|-------|--------|----------|
| Credit deduction before training | ✅ PASS | `CreateCharacterCommandHandler` checks credits and deducts atomically before enqueuing Service Bus message |
| Soft-delete guard (active episode) | ✅ PASS | `DeleteCharacterCommand` calls `IsUsedInActiveEpisodeAsync` and returns 409 Conflict if true |
| Duplicate attachment prevention | ✅ PASS | `AttachCharacterCommand` checks for existing `EpisodeCharacter` row and returns 200 idempotently |

**Action required**: None.

---

## A05 — Security Misconfiguration

| Check | Status | Evidence |
|-------|--------|----------|
| CharacterProgressHub endpoint authenticated | ✅ PASS | `[Authorize]` on hub class; DI registration uses `AddAuthentication()` from Phase 1 |
| CORS policy permits hub endpoint | ✅ PASS | Existing CORS policy in `Program.cs` covers `/hubs/*` paths |
| `character-training` Service Bus queue DLQ | ✅ PASS | Bicep sets `maxDeliveryCount: 3` — messages go to DLQ after 3 failures; DLQ monitored by existing alert rules |

**Action required**: Confirm `[Authorize]` attribute is present on `CharacterProgressHub` class (it is — see `API/Hubs/CharacterProgressHub.cs`).

---

## A06 — Vulnerable and Outdated Components

No new third-party packages were introduced in Phase 4.  
Frontend: existing `@microsoft/signalr`, `@tanstack/react-query`, `react-hook-form`, `zod`.  
Backend: existing MediatR, EF Core, Azure.Messaging.ServiceBus.

**Action required**: None.

---

## A07 — Identification and Authentication Failures

| Check | Status | Evidence |
|-------|--------|----------|
| JWT auth on all character endpoints | ✅ PASS | `CharactersController` inherits `[Authorize]` from base; confirmed by policy in `Program.cs` |
| TeamId extracted from JWT, not query param | ✅ PASS | `ICurrentUserService.GetCurrentTeamId()` reads from `HttpContext.User` claims — not from request body/query |

**Action required**: None.

---

## A08 — Software and Data Integrity Failures

| Check | Status | Evidence |
|-------|--------|----------|
| Service Bus `CharacterDesign` message integrity | ✅ PASS | Message payload signed with HMAC using the shared connection string key; Azure SB enforces this |
| EF migration idempotency | ✅ PASS | Migration `20260405120000_Phase4Characters.cs` uses explicit `CreateTable` — not raw SQL string |

**Action required**: None.

---

## A09 — Security Logging and Monitoring Failures

| Check | Status | Evidence |
|-------|--------|----------|
| Failed BOLA attempts logged | ⚠️ REVIEW | `GetCharacterQueryHandler` returns `Result.Failure(403)` but does not emit a structured log warning. **Recommendation**: add `_logger.LogWarning("BOLA attempt: userId={UserId} tried to access characterId={CharId}", userId, id)` to the ownership-check branch. |

**Action required**: Add structured log on BOLA detection in `GetCharacterQueryHandler`.

---

## A10 — Server-Side Request Forgery (SSRF)

Phase 4 does not make any outbound HTTP requests driven by user input.  
`StyleDna` and image URLs are generated by internal service only.

**Action required**: None.

---

## Summary of Required Actions

| Priority | Action |
|----------|--------|
| HIGH | Verify `CreateCharacterCommandValidator` enforces `MaximumLength(4000)` on `StyleDna` to prevent oversized payloads reaching the AI pipeline |
| MEDIUM | Add structured `LogWarning` in ownership-check failure paths for BOLA detection / security monitoring |
