using FluentValidation;

namespace AnimStudio.IdentityModule.Application.Commands.AcceptTeamInvite;

internal sealed class AcceptTeamInviteCommandValidator : AbstractValidator<AcceptTeamInviteCommand>
{
    public AcceptTeamInviteCommandValidator()
    {
        RuleFor(x => x.InviteToken)
            .NotEmpty().WithMessage("Invite token is required.")
            .MinimumLength(32).WithMessage("Invite token format is invalid.")
            .MaximumLength(256).WithMessage("Invite token format is invalid.");
    }
}
