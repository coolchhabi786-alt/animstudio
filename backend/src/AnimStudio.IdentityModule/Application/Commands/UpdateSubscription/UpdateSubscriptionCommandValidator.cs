using FluentValidation;

namespace AnimStudio.IdentityModule.Application.Commands.UpdateSubscription;

internal sealed class UpdateSubscriptionCommandValidator : AbstractValidator<UpdateSubscriptionCommand>
{
    private static readonly string[] ValidStatuses =
        ["Active", "PastDue", "Cancelled", "Trialing", "Incomplete", "Unpaid"];

    public UpdateSubscriptionCommandValidator()
    {
        RuleFor(x => x.StripeCustomerId).NotEmpty().WithMessage("Stripe customer ID is required.");
        RuleFor(x => x.StripeSubscriptionId).NotEmpty().WithMessage("Stripe subscription ID is required.");
        RuleFor(x => x.StripePriceId).NotEmpty().WithMessage("Stripe price ID is required.");
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => ValidStatuses.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid subscription status.");
        RuleFor(x => x.CurrentPeriodEnd)
            .GreaterThan(DateTimeOffset.UtcNow.AddDays(-1))
            .WithMessage("CurrentPeriodEnd must be a future or recent date.");
    }
}
