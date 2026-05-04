using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace AnimStudio.ContentModule.Application.Commands.CreateReviewLink;

public sealed record CreateReviewLinkCommand(
    Guid      EpisodeId,
    Guid      RenderId,
    Guid      CreatedByUserId,
    DateTime? ExpiresAt  = null,
    string?   Password   = null) : IRequest<Result<ReviewLinkDto>>;

public sealed class CreateReviewLinkValidator : AbstractValidator<CreateReviewLinkCommand>
{
    public CreateReviewLinkValidator()
    {
        RuleFor(x => x.EpisodeId).NotEmpty();
        RuleFor(x => x.RenderId).NotEmpty();
        RuleFor(x => x.CreatedByUserId).NotEmpty();
        RuleFor(x => x.Password).MaximumLength(100).When(x => x.Password is not null);
    }
}

public sealed class CreateReviewLinkHandler(
    IReviewLinkRepository reviewLinks,
    IConfiguration        configuration)
    : IRequestHandler<CreateReviewLinkCommand, Result<ReviewLinkDto>>
{
    public async Task<Result<ReviewLinkDto>> Handle(CreateReviewLinkCommand cmd, CancellationToken ct)
    {
        string? passwordHash = null;
        if (!string.IsNullOrWhiteSpace(cmd.Password))
            passwordHash = BCrypt.Net.BCrypt.HashPassword(cmd.Password);

        var link = ReviewLink.Create(
            cmd.EpisodeId, cmd.RenderId, cmd.CreatedByUserId, cmd.ExpiresAt, passwordHash);

        await reviewLinks.AddAsync(link, ct);

        var frontendBaseUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
        var shareUrl = $"{frontendBaseUrl}/review/{link.Token}";

        return Result<ReviewLinkDto>.Success(MapToDto(link, shareUrl));
    }

    internal static ReviewLinkDto MapToDto(ReviewLink link, string shareUrl)
        => new(link.Id, link.Token, shareUrl, link.EpisodeId,
               link.ExpiresAt, link.IsRevoked, link.ViewCount, link.CreatedAt);
}
