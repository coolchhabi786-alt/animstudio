using AnimStudio.IdentityModule.Application.DTOs;
using AnimStudio.IdentityModule.Domain.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.IdentityModule.Application.Queries.GetAllPlans;

internal sealed class GetAllPlansQueryHandler(
    ISubscriptionRepository subscriptionRepository) : IRequestHandler<GetAllPlansQuery, Result<IReadOnlyList<PlanDto>>>
{
    public async Task<Result<IReadOnlyList<PlanDto>>> Handle(GetAllPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await subscriptionRepository.GetAllActivePlansAsync(cancellationToken);

        var dtos = plans.Select(p => new PlanDto(
            Id: p.Id,
            Name: p.Name,
            StripePriceId: p.StripePriceId,
            EpisodesPerMonth: p.EpisodesPerMonth,
            MaxCharacters: p.MaxCharacters,
            MaxTeamMembers: p.MaxTeamMembers,
            Price: p.Price,
            IsDefault: p.IsDefault)).ToList();

        return Result<IReadOnlyList<PlanDto>>.Success(dtos);
    }
}
