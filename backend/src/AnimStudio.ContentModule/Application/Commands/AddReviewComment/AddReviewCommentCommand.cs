using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Commands.AddReviewComment;

/// <summary>Public command — no auth required. Token identifies the review link.</summary>
public sealed record AddReviewCommentCommand(
    string Token,
    string AuthorName,
    string Text,
    double TimestampSeconds) : IRequest<Result<ReviewCommentDto>>;

public sealed class AddReviewCommentValidator : AbstractValidator<AddReviewCommentCommand>
{
    public AddReviewCommentValidator()
    {
        RuleFor(x => x.Token).NotEmpty().MaximumLength(16);
        RuleFor(x => x.AuthorName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.TimestampSeconds).GreaterThanOrEqualTo(0);
    }
}

public sealed class AddReviewCommentHandler(
    IReviewLinkRepository    reviewLinks,
    IReviewCommentRepository reviewComments)
    : IRequestHandler<AddReviewCommentCommand, Result<ReviewCommentDto>>
{
    public async Task<Result<ReviewCommentDto>> Handle(AddReviewCommentCommand cmd, CancellationToken ct)
    {
        var link = await reviewLinks.GetByTokenAsync(cmd.Token, ct);
        if (link is null || !link.IsValid())
            return Result<ReviewCommentDto>.Failure("Review link not found or has expired.", "INVALID_TOKEN");

        var comment = ReviewComment.Create(link.Id, cmd.AuthorName, cmd.Text, cmd.TimestampSeconds);
        await reviewComments.AddAsync(comment, ct);

        return Result<ReviewCommentDto>.Success(
            new ReviewCommentDto(comment.Id, comment.AuthorName, comment.Text,
                                 comment.TimestampSeconds, comment.IsResolved, comment.CreatedAt));
    }
}
