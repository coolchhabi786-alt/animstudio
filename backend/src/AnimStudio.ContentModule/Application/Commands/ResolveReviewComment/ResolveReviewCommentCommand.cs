using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Commands.ResolveReviewComment;

public sealed record ResolveReviewCommentCommand(Guid CommentId, Guid UserId) : IRequest<Result>;

public sealed class ResolveReviewCommentValidator : AbstractValidator<ResolveReviewCommentCommand>
{
    public ResolveReviewCommentValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class ResolveReviewCommentHandler(
    IReviewCommentRepository reviewComments,
    IReviewLinkRepository    reviewLinks)
    : IRequestHandler<ResolveReviewCommentCommand, Result>
{
    public async Task<Result> Handle(ResolveReviewCommentCommand cmd, CancellationToken ct)
    {
        var comment = await reviewComments.GetByIdAsync(cmd.CommentId, ct);
        if (comment is null)
            return Result.Failure("Comment not found.", "NOT_FOUND");

        var link = await reviewLinks.GetByIdAsync(comment.ReviewLinkId, ct);
        if (link is null || link.CreatedByUserId != cmd.UserId)
            return Result.Failure("Only the review link creator can resolve comments.", "NOT_CREATOR");

        comment.Resolve(cmd.UserId);
        await reviewComments.UpdateAsync(comment, ct);
        return Result.Success();
    }
}
