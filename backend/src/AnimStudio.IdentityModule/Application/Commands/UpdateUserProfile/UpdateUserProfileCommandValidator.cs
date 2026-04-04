using FluentValidation;

namespace AnimStudio.IdentityModule.Application.Commands.UpdateUserProfile;

internal sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MinimumLength(2).WithMessage("Display name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Display name must not exceed 100 characters.");
        RuleFor(x => x.AvatarUrl)
            .MaximumLength(2048).WithMessage("Avatar URL must not exceed 2048 characters.")
            .Must(url => url is null || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Avatar URL must be a valid absolute URI.")
            .When(x => x.AvatarUrl is not null);
    }
}
