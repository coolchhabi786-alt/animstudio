# Phase 5 Security Review: OWASP Top 10 Audit

## Phase: Script Workshop
## Date: 2026-04-10
## Reviewer: QA Agent — Security Review

---

### A01: Broken Access Control
**Severity:** Low
**Evidence:** All four `ScriptController` endpoints are decorated with
`[Authorize(Policy = "RequireTeamMember")]`. The controller class itself carries
the policy attribute, ensuring no endpoint can accidentally be left unprotected.
Episode ownership is validated in each MediatR handler by loading the episode via
`IEpisodeRepository.GetByIdAsync()` — if the episode doesn't exist or doesn't
belong to the team, a `NOT_FOUND` failure is returned.

**Mitigation:** No immediate action required. Team-scoping is enforced via the
episode ownership chain. Future enhancement: add an `ICurrentUserService` check
to verify the requesting user's team matches the episode's team directly in the
controller or a pipeline behaviour.

---

### A02: Cryptographic Failures
**Severity:** Low
**Evidence:** The Script entity stores screenplay content as JSON text in the
database — no PII, no credentials. Director notes are free-text guidance (max
5000 chars) and contain no sensitive data. All database connections use encrypted
connection strings stored in Azure Key Vault.

**Mitigation:** No issues found. Continue using Key Vault for connection strings
and ensure TLS 1.2+ for all database connections.

---

### A03: Injection
**Severity:** Low
**Evidence:**
- **SQL Injection**: All database access uses EF Core LINQ queries with
  parameterized predicates (`FirstOrDefaultAsync(s => s.EpisodeId == episodeId)`).
  No raw SQL or string concatenation is used in `ScriptRepository`.
- **JSON Injection**: `RawJson` is serialized/deserialized via `System.Text.Json`
  with `JsonNamingPolicy.CamelCase` and no custom converters. No `TypeNameHandling`
  is used (that's a Newtonsoft risk, not System.Text.Json).
- **XSS via Director Notes**: Director notes are stored and returned as plain
  text — they are not rendered as HTML on the backend. The frontend must handle
  output encoding (React auto-escapes by default).

**Mitigation:** No immediate SQL or JSON injection risks. Frontend components
correctly use React JSX interpolation (auto-escaped) for director notes and
dialogue text. No `dangerouslySetInnerHTML` usage detected.

---

### A04: Insecure Design
**Severity:** Medium
**Evidence:** The `POST /episodes/{id}/script` endpoint does not enforce rate
limiting at the application level — a malicious user could rapidly enqueue many
script generation jobs, consuming GPU resources and credits.

**Mitigation:** Rate limiting is applied at the API gateway level via the
`RateLimiting` middleware configured in `Program.cs` (5 tiers established in
Phase 1). The `POST` script generation endpoint falls under the default tier.
Consider adding a specific rate limit for script generation (e.g., 3 requests
per 10 minutes per episode) to prevent abuse.

---

### A05: Security Misconfiguration
**Severity:** Low
**Evidence:** The `ScriptController` correctly returns ProblemDetails-compatible
error responses with error codes and messages. No stack traces or internal
exception details are leaked in error responses. The controller uses structured
error codes (`NOT_FOUND`, `CHARACTERS_NOT_READY`, `INVALID_CHARACTERS`, `NO_SCRIPT`).

**Mitigation:** No issues found. Error handling follows the established pattern.

---

### A06: Vulnerable and Outdated Components
**Severity:** Low
**Evidence:** Phase 5 does not introduce new NuGet packages or npm dependencies
beyond those already present in the project. `System.Text.Json` is the built-in
.NET 8 JSON library with no known vulnerabilities.

**Mitigation:** Continue monitoring NuGet and npm advisories. Run `dotnet list
package --vulnerable` and `npm audit` in CI pipeline.

---

### A07: Identification and Authentication Failures
**Severity:** Low
**Evidence:** JWT Bearer authentication is enforced via the `[Authorize]`
attribute on all Script endpoints. The `RequireTeamMember` policy validates team
membership. No anonymous access is possible.

**Mitigation:** No issues found for Phase 5 endpoints. Token rotation and
expiration are handled at the infrastructure level (Phase 1).

---

### A08: Software and Data Integrity Failures
**Severity:** Medium
**Evidence:** When `SaveManualEdits()` is called, the handler deserializes the
incoming `ScreenplayDto` and re-serializes it to JSON before storing. This
ensures the stored JSON always conforms to the expected schema. However, there
is no schema validation step — a malformed `ScreenplayDto` with empty scenes or
negative timing values would be accepted.

**Mitigation:** Add FluentValidation rules to `SaveScriptCommand` to enforce:
- At least one scene in the screenplay.
- `SceneNumber` > 0 and sequential.
- `StartTime` >= 0 and `EndTime` > `StartTime` for all dialogue lines.
- Non-empty `Character` and `Text` fields.
Currently, the validator only checks `EpisodeId.NotEmpty()` and
`Screenplay.NotNull()`. Consider extending it with nested child validators.

---

### A09: Security Logging and Monitoring Failures
**Severity:** Low
**Evidence:** MediatR pipeline behaviours include `LoggingBehaviour` which logs
all command/query executions with correlation IDs. Script generation and save
operations are logged. No passwords, tokens, or screenplay content are logged
at the Info level.

**Mitigation:** Ensure verbose/debug logging does not dump the full `RawJson`
payload (which could be large). Consider logging only the screenplay title and
scene count at Info level, reserving full JSON for Debug level.

---

### A10: Server-Side Request Forgery (SSRF)
**Severity:** N/A
**Evidence:** Phase 5 does not make any outbound HTTP calls from the backend.
Script generation is handled asynchronously via Service Bus messages. No user-
supplied URLs are fetched by the server.

**Mitigation:** No SSRF risk in Phase 5.

---

## Summary

| Finding | Severity | Status |
|---------|----------|--------|
| A01 Access Control | Low | Covered by policy + episode ownership |
| A02 Cryptographic Failures | Low | No sensitive data in scripts |
| A03 Injection | Low | Parameterized EF Core, System.Text.Json |
| A04 Insecure Design | Medium | Rate limit recommended for script generation |
| A05 Security Misconfiguration | Low | No stack traces leaked |
| A06 Outdated Components | Low | No new dependencies |
| A07 Auth Failures | Low | JWT + RequireTeamMember policy |
| A08 Data Integrity | Medium | Extend FluentValidation for screenplay |
| A09 Logging | Low | Avoid logging full RawJson at Info |
| A10 SSRF | N/A | No outbound HTTP calls |

**Overall Assessment:** Phase 5 Script Workshop has a strong security posture.
Two medium-severity findings (A04 rate limiting, A08 input validation) should
be addressed. No critical or high-severity vulnerabilities detected.
