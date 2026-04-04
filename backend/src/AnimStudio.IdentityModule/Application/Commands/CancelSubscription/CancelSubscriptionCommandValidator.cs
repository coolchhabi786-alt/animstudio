using FluentValidation;

namespace AnimStudio.IdentityModule.Application.Commands.CancelSubscription;

internal sealed class CancelSubscriptionCommandValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty().WithMessage("Team ID is required.");
    }
}
