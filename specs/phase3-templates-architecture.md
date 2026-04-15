# Phase 3 — Template & Style Library — Architecture Notes

## Azure Service Choices

| Concern | Service | Rationale |
|---|---|---|
| Template/Style storage | Azure SQL (same `content.*` schema) | Co-located with Episode/Project data; FK from Episode.TemplateId |
| Sample render images | Azure Blob Storage → Azure CDN | Static assets served via CDN edge nodes; URL stored in `SampleImageUrl` |
| Preview videos | Azure Blob Storage → Azure CDN | Short MP4 clips (~5 s); thumbnail from first frame |
| Caching | IDistributedCache (Redis) | Templates and styles are read-heavy, rarely mutated; TTL 1 hour |

## Data Model Decisions

### EpisodeTemplate
- Lives in `content.EpisodeTemplates` table, **no soft-delete** — use `IsActive=false` to retire a template.
- `PlotStructure` stored as `nvarchar(max)` JSON. Frontend deserialises into typed `PlotStructureDto`.
- `SortOrder` allows merchandising: promoted templates surface at the top.
- FK from `content.Episodes.TemplateId` is **optional** — episodes created without a template have `TemplateId = NULL`.

### StylePreset
- Lives in `content.StylePresets` table, one row per `Style` enum value.
- `FluxStylePromptSuffix` is appended verbatim to every Flux.1 image generation call.
- Seeded at migrations time; updated by ops tooling, not through the API.
- No soft-delete; use `IsActive=false`.

### Episode.Style (string)  
- Phase 2 stores it as `nvarchar(500)`. Phase 3 validates it against the `Style` enum in `CreateEpisodeValidator`.
- No column migration required — the string value is the enum name (`"Pixar3D"`, `"Anime"`, …).

## Caching Strategy

| Query | Cache Key | TTL |
|---|---|---|
| `GetTemplatesQuery` | `templates:all` / `templates:genre:{genre}` | 1 hour |
| `GetTemplateQuery` | `template:{id}` | 1 hour |
| `GetStylePresetsQuery` | `styles:all` | 1 hour |

Templates and styles change only on deployment/seeding so a 1-hour TTL with explicit invalidation on style updates is appropriate.

## Seed Data

Eight templates (one per genre) and eight style presets (one per `Style` enum value) are inserted via EF Core `HasData` in the Phase 3 migration.

## API Design

```
GET  /api/v1/templates          → list templates (optional ?genre= filter)
GET  /api/v1/templates/{id}     → single template
GET  /api/v1/styles             → list style presets
```

All endpoints require `[Authorize(Policy = "RequireTeamMember")]` (authenticated user in a team).  
Read-only for all roles — no create/update/delete exposed via public API.  
Mutation is ops-only (seed data or future admin panel).

## Frontend Architecture

- `episodes/new/page.tsx` — 2-column episode creation wizard:
  - Left: idea textarea + `<TemplateGallery>` (genre tabs + template cards)
  - Right: `<StylePicker>` swatch grid + selected style details
- `use-templates.ts` — TanStack Query hooks: `useTemplates(genre?)`, `useTemplate(id)`, `useStylePresets()`
- Cache time: `staleTime: 60 * 60 * 1000` (1 hour, mirrors server TTL)
- On template card click → auto-select the template's `defaultStyle` in the style picker
- On "Create Episode" submit → `POST /api/v1/projects/{projectId}/episodes` with `{ name, idea, style, templateId }`

## Security Notes

- No user-supplied values flow into template/style data (read-only lookup).
- `FluxStylePromptSuffix` is only used server-side in the generation pipeline, never echoed to end users verbatim.
- No PII stored in templates or style presets.
