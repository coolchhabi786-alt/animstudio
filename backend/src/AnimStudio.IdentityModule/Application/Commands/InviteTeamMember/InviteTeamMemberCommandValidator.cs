using FluentValidation;

namespace AnimStudio.IdentityModule.Application.Commands.InviteTeamMember;

internal sealed class InviteTeamMemberCommandValidator : AbstractValidator<InviteTeamMemberCommand>
{
    private static readonly string[] ValidRoles = ["Admin", "Member"];

    public InviteTeamMemberCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty().WithMessage("Team ID is required.");
        RuleFor(x => x.InvitedByUserId).NotEmpty().WithMessage("Inviting user ID is required.");
        RuleFor(x => x.InviteeEmail)
            .NotEmpty().WithMessage("Invitee email is required.")
            .EmailAddress().WithMessage("Invitee email is not valid.")
            .MaximumLength(320).WithMessage("Email must not exceed 320 characters.");
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(r => ValidRoles.Contains(r, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Role must be 'Admin' or 'Member'.");
    }
}
