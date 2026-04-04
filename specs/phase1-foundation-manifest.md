# Phase 1 Foundation File Manifest

## SharedKernel
- backend/src/AnimStudio.SharedKernel/Entity.cs — Base entity class.
- backend/src/AnimStudio.SharedKernel/AggregateRoot.cs — Base aggregate root class.
- backend/src/AnimStudio.SharedKernel/IDomainEvent.cs — Marker interface for domain events.
- backend/src/AnimStudio.SharedKernel/IModuleRegistration.cs — Interface for module registration.
- backend/src/AnimStudio.SharedKernel/Result.cs — Result<T> discriminated union.
- backend/src/AnimStudio.SharedKernel/PaginatedList.cs — Paginated list implementation.
- backend/src/AnimStudio.SharedKernel/ICacheKey.cs — Cacheable query interface.
- backend/src/AnimStudio.SharedKernel/OutboxMessage.cs — Shared OutboxMessage entity class.
- backend/src/AnimStudio.SharedKernel/EpisodeSagaState.cs — Shared EpisodeSagaState entity class.

## IdentityModule Domain
- backend/src/AnimStudio.IdentityModule/Domain/User.cs — User entity.
- backend/src/AnimStudio.IdentityModule/Domain/Team.cs — Team entity.
- backend/src/AnimStudio.IdentityModule/Domain/TeamMember.cs — Team member entity.
- backend/src/AnimStudio.IdentityModule/Domain/Plan.cs — Subscription plan entity.
- backend/src/AnimStudio.IdentityModule/Domain/Subscription.cs — Subscription entity.
- backend/src/AnimStudio.IdentityModule/Domain/TeamRole.cs — Team role enumeration.
- backend/src/AnimStudio.IdentityModule/Domain/SubscriptionStatus.cs — Subscription status enumeration.
- backend/src/AnimStudio.IdentityModule/Domain/DomainEvents/UserRegistered.cs — Event.
- backend/src/AnimStudio.IdentityModule/Domain/DomainEvents/TeamCreated.cs — Event.
- backend/src/AnimStudio.IdentityModule/Domain/DomainEvents/TeamMemberInvited.cs — Event.
- backend/src/AnimStudio.IdentityModule/Domain/DomainEvents/TeamMemberJoined.cs — Event.
- backend/src/AnimStudio.IdentityModule/Domain/DomainEvents/SubscriptionActivated.cs — Event.
- backend/src/AnimStudio.IdentityModule/Domain/DomainExceptions/DomainException.cs — Base exception class.
- backend/src/AnimStudio.IdentityModule/Domain/DomainExceptions/TeamMemberLimitExceededException.cs — Exception.
- backend/src/AnimStudio.IdentityModule/Domain/DomainExceptions/EpisodeLimitExceededException.cs — Exception.
- backend/src/AnimStudio.IdentityModule/Domain/DomainExceptions/FeatureNotAvailableException.cs — Exception.

## IdentityModule Application
- backend/src/AnimStudio.IdentityModule/Application/ValidationBehaviour.cs — FluentValidation pipeline behavior.
- backend/src/AnimStudio.IdentityModule/Application/LoggingBehaviour.cs — Serilog for performance logging.
- backend/src/AnimStudio.IdentityModule/Application/CorrelationBehaviour.cs — Correlation ID injection pipeline behavior.
- backend/src/AnimStudio.IdentityModule/Application/CachingBehaviour.cs — Redis cache pipeline behavior.
- backend/src/AnimStudio.IdentityModule/Application/TransactionBehaviour.cs — EF Core Transaction Outbox pipeline behavior.
- backend/src/AnimStudio.IdentityModule/Application/Commands/RegisterUserCommand.cs — Register action.
- backend/src/AnimStudio.IdentityModule/Application/Commands/CreateNewTeamCommand.cs
...
