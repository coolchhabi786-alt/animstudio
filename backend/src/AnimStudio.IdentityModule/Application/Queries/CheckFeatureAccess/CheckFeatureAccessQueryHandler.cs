using AnimStudio.IdentityModule.Domain.Entities;
using AnimStudio.IdentityModule.Domain.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Queries.CheckFeatureAccess;

internal sealed class CheckFeatureAccessQueryHandler(
    ISubscriptionRepository subscriptionRepository) : IRequestHandler<CheckFeatureAccessQuery, Result<bool>>
{
    // Feature gate constants — kept here so they are centrally documented
    private static readonly Dictionary<string, SubscriptionStatus[]> _featureRequirements = new()
    {
        ["advanced_analytics"]      = [SubscriptionStatus.Active],
        ["custom_characters"]       = [SubscriptionStatus.Active, SubscriptionStatus.Trialing],
        ["api_access"]              = [SubscriptionStatus.Active],
        ["priority_rendering"]      = [SubscriptionStatus.Active],
    };

    public async Task<Result<bool>> Handle(CheckFeatureAccessQuery request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByTeamIdAsync(request.TeamId, cancellationToken);
        if (subscription is null)
            return Result<bool>.Success(false);

        if (!_featureRequirements.TryGetValue(request.Feature, out var requiredStatuses))
        {
            // Unknown feature keys are allowed by default (forward-compat)
            return Result<bool>.Success(true);
        }

        var hasAccess = requiredStatuses.Contains(subscription.Status);
        return Result<bool>.Success(hasAccess);
    }
}
