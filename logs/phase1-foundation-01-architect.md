## Final Answer

### ENTITY SUMMARY:
1. **User:**  
   - Id, Email, DisplayName, ExternalId, CreatedAt, LastLoginAt  
   - Navigation: TeamMemberships.

2. **Team:**  
   - Id, Name, OwnerId, CreatedAt  
   - Navigation: Members, Subscription.

3. **TeamMember:**  
   - TeamId, UserId, Role, JoinedAt, InviteToken, InviteExpiresAt, InviteAcceptedAt  
   - Navigation: Team, User.

4. **Plan:**  
   - Id, Name, StripePriceId, EpisodesPerMonth, MaxCharacters, MaxTeamMembers, Price, IsActive, IsDefault.

5. **Subscription:**  
   - Id, TeamId, PlanId, StripeSubscriptionId, StripeCustomerId, Status, CurrentPeriodStart, CurrentPeriodEnd, UsageEpisodesThisMonth, UsageResetAt  
   - Navigation: Team, Plan.

### API ENDPOINTS:
1. `GET /api/v1/auth/me` - Retrieve current authenticated user.  
2. `PUT /api/v1/users/profile` - Update user's profile (DisplayName).  
3. `POST /api/v1/teams` - Create a new team (Name, OwnerId).  
4. `GET /api/v1/teams/{id}` - Get details of a specific team.  
5. `GET /api/v1/teams/{id}/members` - List members of a team.  
6. `POST /api/v1/teams/{id}/invites` - Invite users to the team.  
7. `POST /api/v1/teams/invites/{token}/accept` - Accept a team invite by token.  
8. `GET /api/v1/billing/plans` - List available subscription plans.  
9. `GET /api/v1/billing/subscription` - Get current subscription details for the team.  
10. `POST /api/v1/billing/checkout` - Create a Stripe checkout session for subscription.  
11. `POST /api/v1/billing/portal` - Access the Stripe portal for billing management.  
12. `POST /api/v1/billing/webhook` - Stripe webhook listener (AllowAnonymous).

### SHAREDKERNEL FILES:
- `backend/src/AnimStudio.SharedKernel/Entity.cs`
- `backend/src/AnimStudio.SharedKernel/AggregateRoot.cs`
- `backend/src/AnimStudio.SharedKernel/IDomainEvent.cs`
- `backend/src/AnimStudio.SharedKernel/IModuleRegistration.cs`
- `backend/src/AnimStudio.SharedKernel/Result.cs`
- `backend/src/AnimStudio.SharedKernel/PaginatedList.cs`
- `backend/src/AnimStudio.SharedKernel/ICacheKey.cs`
- `backend/src/AnimStudio.SharedKernel/OutboxMessage.cs`
- `backend/src/AnimStudio.SharedKernel/EpisodeSagaState.cs`

### IDENTITYMODULE DOMAIN FILES:
- `backend/src/AnimStudio.IdentityModule/Domain/User.cs`
- `backend/src/AnimStudio.IdentityModule/Domain/Team.cs`
- `backend/src/AnimStudio.IdentityModule/Domain/TeamMember.cs`
- `backend/src/AnimStudio.IdentityModule/Domain/Plan.cs`
- `backend/src/AnimStudio.IdentityModule/Domain/Subscription.cs`
- `backend/src/AnimStudio.IdentityModule/Domain/TeamRole.cs`
- `backend/src/AnimStudio.IdentityModule/Domain/SubscriptionStatus.cs`
- `backend/src/AnimStudio.IdentityModule/Domain/DomainEvents/UserRegistered.cs`
- `backend/src/AnimStudio.IdentityModule/Domain/DomainEvents/TeamCreated.cs`
- `backend/src/AnimStudio.IdentityModule/Domain/DomainEvents/TeamMemberInvited.cs`
- `backend/src/AnimStudio.IdentityModule/Domain/DomainEvents/TeamMemberJoined.cs`
- `backend/src/AnimStudio.IdentityModule/Domain/DomainEvents/SubscriptionActivated.cs`
- `backend/src/AnimStudio.IdentityModule/Domain/DomainExceptions/DomainException.cs`
- `backend/src/AnimStudio.IdentityModule/Domain/DomainExceptions/TeamMemberLimitExceededException.cs`
- `backend/src/AnimStudio.IdentityModule/Domain/DomainExceptions/EpisodeLimitExceededException.cs`
- `backend/src/AnimStudio.IdentityModule/Domain/DomainExceptions/FeatureNotAvailableException.cs`

### IDENTITYMODULE APPLICATION FILES:
- `backend/src/AnimStudio.IdentityModule/Application/ValidationBehaviour.cs`
- `backend/src/AnimStudio.IdentityModule/Application/LoggingBehaviour.cs`
- `backend/src/AnimStudio.IdentityModule/Application/CorrelationBehaviour.cs`
- `backend/src/AnimStudio.IdentityModule/Application/CachingBehaviour.cs`
- `backend/src/AnimStudio.IdentityModule/Application/TransactionBehaviour.cs`
- `backend/src/AnimStudio.IdentityModule/Application/Commands/RegisterUserCommand.cs`
- `backend/src/AnimStudio.IdentityModule/Application/Commands/CreateNewTeamCommand.cs`

### IDENTITYMODULE INFRASTRUCTURE FILES:
- `backend/src/AnimStudio.IdentityModule/Infrastructure/IdentityDbContext.cs`
- `backend/src/AnimStudio.IdentityModule/Infrastructure/Repository.cs`
- `backend/src/AnimStudio.IdentityModule/Infrastructure/StripeService.cs`
- `backend/src/AnimStudio.IdentityModule/Infrastructure/EmailService.cs`
- `backend/src/AnimStudio.IdentityModule/Infrastructure/CurrentUserService.cs`
- `backend/src/AnimStudio.IdentityModule/Infrastructure/CacheService.cs`

### API FILES:
- `backend/src/AnimStudio.API/Program.cs`
- `backend/src/AnimStudio.API/Controllers/AuthController.cs`
- `backend/src/AnimStudio.API/Controllers/UsersController.cs`
- `backend/src/AnimStudio.API/Controllers/TeamsController.cs`
- `backend/src/AnimStudio.API/Controllers/BillingController.cs`
- `backend/src/AnimStudio.API/Middleware/CorrelationIdMiddleware.cs`
- `backend/src/AnimStudio.API/Middleware/SubscriptionGateMiddleware.cs`
- `backend/src/AnimStudio.API/Middleware/GlobalExceptionHandler.cs`

### STUB MODULE FILES:
#### ContentModule:
- `backend/src/AnimStudio.ContentModule/Domain/Project.cs` (stub)
- `backend/src/AnimStudio.ContentModule/Domain/Episode.cs` (stub)
- `backend/src/AnimStudio.ContentModule/Application/Handlers/SubscriptionActivatedHandler.cs` (stub)
- `backend/src/AnimStudio.ContentModule/Infrastructure/ContentDbContext.cs` (stub)

#### DeliveryModule:
- `backend/src/AnimStudio.DeliveryModule/DeliveryModuleRegistration.cs`
- `backend/src/AnimStudio.DeliveryModule/DeliveryDbContext.cs`

#### AnalyticsModule:
- `backend/src/AnimStudio.AnalyticsModule/AnalyticsModuleRegistration.cs`
- `backend/src/AnimStudio.AnalyticsModule/AnalyticsDbContext.cs`

### FRONTEND CONFIG FILES:
- `frontend/package.json`
- `frontend/src/middleware.ts`
- `frontend/src/lib/api-client.ts`
- `frontend/src/lib/auth.ts`
- `frontend/src/lib/correlation.ts`
- `frontend/src/lib/stripe-client.ts`
- `frontend/src/types/api.ts`
- `frontend/src/stores/auth-store.ts`

### FRONTEND PAGE FILES:
- `frontend/src/app/(auth)/login/page.tsx`
- `frontend/src/app/(auth)/signup/page.tsx`
- `frontend/src/app/(auth)/accept-invite/page.tsx`
- `frontend/src/app/(studio)/layout.tsx`
- `frontend/src/app/(studio)/dashboard/page.tsx`
- `frontend/src/app/(studio)/settings/profile/page.tsx`
- `frontend/src/app/(studio)/settings/team/page.tsx`
- `frontend/src/app/(studio)/settings/billing/page.tsx`
- `frontend/src/components/layout/sidebar.tsx`
- `frontend/src/components/layout/top-bar.tsx`
- `frontend/src/components/billing/plan-card.tsx`
- `frontend/src/components/billing/usage-meter.tsx`
- `frontend/src/components/team/invite-form.tsx`
- `frontend/src/components/team/member-row.tsx`

### INFRA FILES:
- `infra/modules/containerapp-env.bicep`
- `infra/modules/containerapp.bicep`
- `infra/modules/redis.bicep`
- `infra/modules/frontdoor.bicep`
- `infra/modules/sql.bicep`
- `infra/modules/hangfire-sql.bicep`
- `infra/modules/servicebus.bicep`
- `infra/modules/signalr.bicep`
- `infra/modules/storage.bicep`
- `infra/modules/keyvault.bicep`
- `infra/modules/acr.bicep`
- `infra/parameters/dev.json`
- `infra/parameters/prod.json`
- `.github/workflows/infra.yml`
- `.github/workflows/deploy-api.yml`
- `.github/workflows/deploy-web.yml`