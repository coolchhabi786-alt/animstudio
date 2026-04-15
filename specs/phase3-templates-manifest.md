# Phase 3 — Template & Style Library — File Manifest

## [ContentModule.Domain]
- `backend/src/AnimStudio.ContentModule/Domain/Enums/GenreEnum.cs`
- `backend/src/AnimStudio.ContentModule/Domain/Enums/StyleEnum.cs`
- `backend/src/AnimStudio.ContentModule/Domain/Entities/EpisodeTemplate.cs`
- `backend/src/AnimStudio.ContentModule/Domain/Entities/StylePreset.cs`

## [ContentModule.Application]
- `backend/src/AnimStudio.ContentModule/Application/DTOs/TemplateDtos.cs`
- `backend/src/AnimStudio.ContentModule/Application/Interfaces/ITemplateRepositories.cs`
- `backend/src/AnimStudio.ContentModule/Application/Queries/TemplateQueries.cs`

## [ContentModule.Infrastructure]
- updated: `backend/src/AnimStudio.ContentModule/Infrastructure/Persistence/ContentDbContext.cs`
- `backend/src/AnimStudio.ContentModule/Infrastructure/Repositories/TemplateRepositories.cs`
- `backend/src/AnimStudio.ContentModule/Migrations/20260405000001_Phase3Templates.cs`
- `backend/src/AnimStudio.ContentModule/Migrations/20260405000001_Phase3Templates.Designer.cs`

## [AnimStudio.API]
- `backend/src/AnimStudio.API/Controllers/TemplatesController.cs`
- updated: `backend/src/AnimStudio.ContentModule/ContentModuleRegistration.cs`

## [Frontend.Hooks]
- `frontend/src/hooks/use-templates.ts`

## [Frontend.Types]
- updated: `frontend/src/types/index.ts`

## [Frontend.Components]
- `frontend/src/components/template/template-card.tsx`
- `frontend/src/components/template/style-picker.tsx`
- `frontend/src/components/template/template-gallery.tsx`

## [Frontend.Pages]
- `frontend/src/app/(dashboard)/episodes/new/page.tsx`

## [Specs]
- `specs/openapi/phase3-templates-api.yaml`
- `specs/database/phase3-templates-entities.cs`
- `specs/phase3-templates-manifest.md`
- `specs/phase3-templates-architecture.md`
