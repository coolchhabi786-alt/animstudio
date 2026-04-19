using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.ContentModule.Application.Queries.GetAnimationClipSignedUrl;

/// <summary>Issues a signed Blob URL (60-second TTL) for a single rendered clip.</summary>
public sealed record GetAnimationClipSignedUrlQuery(Guid EpisodeId, Guid ClipId)
    : IRequest<Result<SignedClipUrlDto>>;

public sealed class GetAnimationClipSignedUrlHandler(
    IAnimationClipRepository clips,
    IClipUrlSigner signer)
    : IRequestHandler<GetAnimationClipSignedUrlQuery, Result<SignedClipUrlDto>>
{
    public async Task<Result<SignedClipUrlDto>> Handle(
        GetAnimationClipSignedUrlQuery query, CancellationToken ct)
    {
        var clip = await clips.GetByIdAsync(query.ClipId, ct);
        if (clip is null || clip.EpisodeId != query.EpisodeId)
            return Result<SignedClipUrlDto>.Failure("Clip not found.", "NOT_FOUND");

        if (clip.Status != ClipStatus.Ready || string.IsNullOrWhiteSpace(clip.ClipUrl))
            return Result<SignedClipUrlDto>.Failure(
                "Clip is not yet ready for playback.", "CLIP_NOT_READY");

        var (url, expiresAt) = signer.Sign(clip.ClipUrl);
        return Result<SignedClipUrlDto>.Success(new SignedClipUrlDto(clip.Id, url, expiresAt));
    }
}
