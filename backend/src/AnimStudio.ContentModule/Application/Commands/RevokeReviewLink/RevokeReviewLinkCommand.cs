using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Commands.RevokeReviewLink;

public sealed record RevokeReviewLinkCommand(Guid ReviewLinkId, Guid UserId) : IRequest<Result>;

public sealed class RevokeReviewLinkValidator : AbstractValidator<RevokeReviewLinkCommand>
{
    public RevokeReviewLinkValidator()
    {
        RuleFor(x => x.ReviewLinkId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class RevokeReviewLinkHandler(IReviewLinkRepository reviewLinks)
    : IRequestHandler<RevokeReviewLinkCommand, Result>
{
    public async Task<Result> Handle(RevokeReviewLinkCommand cmd, CancellationToken ct)
    {
        var link = await reviewLinks.GetByIdAsync(cmd.ReviewLinkId, ct);
        if (link is null)
            return Result.Failure("Review link not found.", "NOT_FOUND");

        if (link.CreatedByUserId != cmd.UserId)
            return Result.Failure("Only the creator can revoke this review link.", "NOT_CREATOR");

        link.Revoke();
        await reviewLinks.UpdateAsync(link, ct);
        return Result.Success();
    }
}
