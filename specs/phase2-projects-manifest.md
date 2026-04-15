# Phase 2: File Manifest

## [SharedKernel]
backend/src/AnimStudio.SharedKernel/Events/DomainEvent.cs       - Base domain event class
backend/src/AnimStudio.SharedKernel/Enums/PipelineStage.cs      - PipelineStage enum definition

## [IdentityModule.Domain]
No new files in this phase

## [IdentityModule.Application]
No new files in this phase

## [IdentityModule.Infrastructure]
No new files in this phase

## [API]
backend/src/AnimStudio.API/Controllers/ProjectsController.cs    - API for Project operations
backend/src/AnimStudio.API/Controllers/EpisodesController.cs    - API for Episode operations
backend/src/AnimStudio.API/Controllers/JobsController.cs        - API for individual Job operations
backend/src/AnimStudio.API/SignalR/ProgressHub.cs               - SignalR hub for real-time updates

## [ContentModule-stub]
backend/src/AnimStudio.ContentModule/Domain/Aggregates/Project.cs  - Project aggregate definition
backend/src/AnimStudio.ContentModule/Domain/Aggregates/Episode.cs  - Episode aggregate definition
backend/src/AnimStudio.ContentModule/Domain/Entities/Job.cs        - Job entity class
backend/src/AnimStudio.ContentModule/Domain/Entities/EpisodeSagaState.cs    - Saga State entity
backend/src/AnimStudio.ContentModule/Application/Commands/CreateProjectCommand.cs  - CreateProject command
backend/src/AnimStudio.ContentModule/Application/Commands/UpdateProjectCommand.cs  - UpdateProject command
backend/src/AnimStudio.ContentModule/Application/Commands/CreateEpisodeCommand.cs  - CreateEpisode command
backend/src/AnimStudio.ContentModule/Application/Commands/DispatchEpisodeJobCommand.cs - DispatchEpisodeJob command
backend/src/AnimStudio.ContentModule/Application/Commands/HandleJobCompletionCommand.cs - HandleJobCompletion command
backend/src/AnimStudio.ContentModule/Application/Queries/GetProjectQuery.cs       - GetProject query
backend/src/AnimStudio.ContentModule/Application/Queries/GetProjectsQuery.cs      - GetProjects paginated list query
backend/src/AnimStudio.ContentModule/Application/Queries/GetEpisodeQuery.cs       - GetEpisode query
backend/src/AnimStudio.ContentModule/Application/Queries/GetEpisodesQuery.cs      - GetEpisodes query
backend/src/AnimStudio.ContentModule/Application/Queries/GetJobQuery.cs           - GetJob query
backend/src/AnimStudio.ContentModule/Application/Queries/GetSagaStateQuery.cs     - GetSagaState query
backend/src/AnimStudio.ContentModule/Infrastructure/ContentDbContext.cs          - Entity Framework Core DB context
backend/src/AnimStudio.ContentModule/Infrastructure/Repositories/ProjectRepository.cs  - Project repository
backend/src/AnimStudio.ContentModule/Infrastructure/Repositories/EpisodeRepository.cs  - Episode repository
backend/src/AnimStudio.ContentModule/Infrastructure/Repositories/JobRepository.cs      - Job repository
backend/src/AnimStudio.ContentModule/Infrastructure/Repositories/SagaStateRepository.cs  - SagaState repository
backend/src/AnimStudio.ContentModule/Infrastructure/HostedServices/CompletionMessageProcessor.cs - Message processor for job completions

## [DeliveryModule-stub]
No new files in this phase

## [AnalyticsModule-stub]
No new files in this phase

## [Frontend-Config]
frontend/package.json                                         - Project dependencies
frontend/next.config.js                                       - Next.js configuration
frontend/src/layout.tsx                                       - Application layout with top-level navigation
frontend/src/config/auth.ts                                   - Authentication configuration
frontend/src/config/signalr.ts                                - SignalR configuration
frontend/src/types/global.ts                                  - Type definitions for shared objects
frontend/src/lib/fetch.ts                                     - Fetch helper for API calls
frontend/src/lib/auth.ts                                      - Authentication helpers

## [Frontend-Pages]
frontend/src/pages/dashboard/page.tsx                        - Main dashboard page for projects
frontend/src/pages/projects/[id]/page.tsx                    - Individual project page with episode list
frontend/src/pages/projects/[id]/episodes/[episodeId]/page.tsx - Episode detail page with wizard steps
frontend/src/components/episode/progress-stepper.tsx         - Stepper component for pipeline stages
frontend/src/components/episode/job-progress-toast.tsx       - Progress toast for jobs (SignalR-driven)
frontend/src/hooks/use-signalr.ts                            - Hook for SignalR lifecycle
frontend/src/hooks/use-episode-progress.ts                   - Hook for episode progress subscription
frontend/src/hooks/use-projects.ts                           - TanStack Query hook for project CRUD
frontend/src/hooks/use-episodes.ts                           - TanStack Query hook for episode CRUD
frontend/src/hooks/use-saga-state.ts                        - TanStack Query hook for saga state polling

## [Infra]
infra/modules/sql.bicep                                      - Database provisioning for AnimStudio content schema
infra/modules/servicebus.bicep                               - Service Bus namespace and queue provisioning
infra/workflows/outbox-publisher-job.yaml                    - Outbox-to-Service Bus workflow
infra/workflows/deadletter-retry-job.yaml                    - Deadletter processor for failed completions
infra/dockerfiles/api.Dockerfile                             - Backend API service Dockerfile