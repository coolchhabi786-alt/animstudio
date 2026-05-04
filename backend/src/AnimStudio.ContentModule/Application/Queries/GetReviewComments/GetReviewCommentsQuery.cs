using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.ContentModule.Application.Queries.GetReviewComments;

public sealed record GetReviewCommentsQuery(string Token)
    : IRequest<Result<IReadOnlyList<ReviewCommentDto>>>;

public sealed class GetReviewCommentsHandler(
    IReviewLinkRepository    reviewLinks,
    IReviewCommentRepository reviewComments)
    : IRequestHandler<GetReviewCommentsQuery, Result<IReadOnlyList<ReviewCommentDto>>>
{
    public async Task<Result<IReadOnlyList<ReviewCommentDto>>> Handle(
        GetReviewCommentsQuery query, CancellationToken ct)
    {
        var link = await reviewLinks.GetByTokenAsync(query.Token, ct);
        if (link is null || !link.IsValid())
            return Result<IReadOnlyList<ReviewCommentDto>>.Failure(
                "Review link not found or has expired.", "INVALID_TOKEN");

        var comments = await reviewComments.GetByReviewLinkIdAsync(link.Id, ct);

        IReadOnlyList<ReviewCommentDto> dtos = comments
            .Select(c => new ReviewCommentDto(
                c.Id, c.AuthorName, c.Text, c.TimestampSeconds, c.IsResolved, c.CreatedAt))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<ReviewCommentDto>>.Success(dtos);
    }
}
