using FluentValidation;

namespace AnimStudio.IdentityModule.Application.Commands.RegisterUser;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.ExternalId)
            .NotEmpty().WithMessage("External identity provider ID is required.")
            .MaximumLength(256).WithMessage("External ID must not exceed 256 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Email address is not in a valid format.")
            .MaximumLength(320).WithMessage("Email must not exceed 320 characters.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MinimumLength(2).WithMessage("Display name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Display name must not exceed 100 characters.");

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(2048).WithMessage("Avatar URL must not exceed 2048 characters.")
            .Must(uri => uri is null || Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Avatar URL must be a valid absolute URI.")
            .When(x => x.AvatarUrl is not null);
    }
}
