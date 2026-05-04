using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace AnimStudio.ContentModule.Application.Queries.GetReviewLink;

public sealed record GetReviewLinkQuery(string Token, string? Password = null)
    : IRequest<Result<ReviewLinkDetailDto>>;

public sealed class GetReviewLinkHandler(
    IReviewLinkRepository    reviewLinks,
    IReviewCommentRepository reviewComments,
    IRenderInfoProvider      renderInfoProvider,
    IConfiguration           configuration)
    : IRequestHandler<GetReviewLinkQuery, Result<ReviewLinkDetailDto>>
{
    public async Task<Result<ReviewLinkDetailDto>> Handle(GetReviewLinkQuery query, CancellationToken ct)
    {
        var link = await reviewLinks.GetByTokenAsync(query.Token, ct);
        if (link is null || !link.IsValid())
            return Result<ReviewLinkDetailDto>.Failure("Review link not found or has expired.", "INVALID_TOKEN");

        if (link.PasswordHash is not null)
        {
            if (string.IsNullOrWhiteSpace(query.Password))
                return Result<ReviewLinkDetailDto>.Failure("This link is password-protected.", "PASSWORD_REQUIRED");

            if (!BCrypt.Net.BCrypt.Verify(query.Password, link.PasswordHash))
                return Result<ReviewLinkDetailDto>.Failure("Incorrect password.", "WRONG_PASSWORD");
        }

        link.IncrementViewCount();
        await reviewLinks.UpdateAsync(link, ct);

        var comments   = await reviewComments.GetByReviewLinkIdAsync(link.Id, ct);
        var renderInfo = await renderInfoProvider.GetByIdAsync(link.RenderId, ct);

        var frontendBaseUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
        var shareUrl        = $"{frontendBaseUrl}/review/{link.Token}";

        return Result<ReviewLinkDetailDto>.Success(new ReviewLinkDetailDto(
            link.Id,
            link.Token,
            shareUrl,
            link.EpisodeId,
            link.RenderId,
            link.ExpiresAt,
            link.IsRevoked,
            link.ViewCount,
            link.CreatedAt,
            renderInfo is null ? null : new RenderInfo(renderInfo.VideoUrl, renderInfo.DurationSeconds),
            comments.Select(c => new ReviewCommentDto(
                c.Id, c.AuthorName, c.Text, c.TimestampSeconds, c.IsResolved, c.CreatedAt))
                .ToList()
                .AsReadOnly()));
    }
}
