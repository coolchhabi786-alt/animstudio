using FluentValidation;

namespace AnimStudio.IdentityModule.Application.Commands.CreateTeam;

internal sealed class CreateTeamCommandValidator : AbstractValidator<CreateTeamCommand>
{
    public CreateTeamCommandValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Team name is required.")
            .MinimumLength(2).WithMessage("Team name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Team name must not exceed 100 characters.");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(2048).WithMessage("Logo URL must not exceed 2048 characters.")
            .Must(url => url is null || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Logo URL must be a valid absolute URI.")
            .When(x => x.LogoUrl is not null);
    }
}
