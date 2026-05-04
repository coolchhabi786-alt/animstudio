using AnimStudio.AnalyticsModule.Domain.Enums;
using AnimStudio.API.Hosted;
using AnimStudio.ContentModule.Application.Commands.AddReviewComment;
using AnimStudio.ContentModule.Application.Commands.CreateReviewLink;
using AnimStudio.ContentModule.Application.Commands.ResolveReviewComment;
using AnimStudio.ContentModule.Application.Commands.RevokeReviewLink;
using AnimStudio.ContentModule.Application.Commands.UpsertBrandKit;
using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Application.Queries.GetBrandKit;
using AnimStudio.ContentModule.Application.Queries.GetReviewComments;
using AnimStudio.ContentModule.Application.Queries.GetReviewLink;
using AnimStudio.ContentModule.Domain.Enums;
using Asp.Versioning;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AnimStudio.API.Controllers;

/// <summary>
/// Review links, timestamped comments, and team brand-kit management.
/// Public (AllowAnonymous) endpoints require only the 16-char token — no auth.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "RequireTeamMember")]
public sealed class ReviewController(
    ISender              mediator,
    IFileStorageService  fileStorage,
    IBackgroundJobClient backgroundJobs) : ControllerBase
{
    // ── POST /api/v1/renders/{renderId}/review-links ────────────────────────

    [HttpPost("api/v{version:apiVersion}/renders/{renderId:guid}/review-links")]
    [ProducesResponseType(typeof(ReviewLinkDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateReviewLink(
        Guid renderId,
        [FromBody] CreateReviewLinkRequest req,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(
            new CreateReviewLinkCommand(req.EpisodeId, renderId, userId, req.ExpiresAt, req.Password), ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error, code = result.ErrorCode });

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    // ── DELETE /api/v1/review-links/{id} ───────────────────────────────────

    [HttpDelete("api/v{version:apiVersion}/review-links/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeReviewLink(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new RevokeReviewLinkCommand(id, GetCurrentUserId()), ct);

        if (!result.IsSuccess)
            return result.ErrorCode switch
            {
                "NOT_FOUND"   => NotFound(new { error = result.Error, code = result.ErrorCode }),
                "NOT_CREATOR" => StatusCode(StatusCodes.Status403Forbidden,
                                     new { error = result.Error, code = result.ErrorCode }),
                _             => BadRequest(new { error = result.Error, code = result.ErrorCode }),
            };

        return NoContent();
    }

    // ── GET /api/v1/review/{token} ─────────────────────────────────────────

    [AllowAnonymous]
    [HttpGet("api/v{version:apiVersion}/review/{token}")]
    [ProducesResponseType(typeof(ReviewLinkDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReviewLink(
        string token,
        [FromQuery] string? password,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetReviewLinkQuery(token, password), ct);

        if (!result.IsSuccess)
            return result.ErrorCode switch
            {
                "PASSWORD_REQUIRED" or "WRONG_PASSWORD" =>
                    StatusCode(StatusCodes.Status401Unauthorized,
                        new { error = result.Error, code = result.ErrorCode }),
                _ => NotFound(new { error = result.Error, code = result.ErrorCode }),
            };

        var dto         = result.Value!;
        var ipHash      = HttpContext.Connection.RemoteIpAddress?.ToString();
        backgroundJobs.Enqueue<TrackVideoViewHangfireProcessor>(p =>
            p.ProcessAsync(dto.EpisodeId, dto.RenderId, VideoViewSource.ReviewLink, ipHash, dto.Id, CancellationToken.None));

        return Ok(dto);
    }

    // ── GET /api/v1/review/{token}/comments ────────────────────────────────

    [AllowAnonymous]
    [HttpGet("api/v{version:apiVersion}/review/{token}/comments")]
    [ProducesResponseType(typeof(IReadOnlyList<ReviewCommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReviewComments(string token, CancellationToken ct)
    {
        var result = await mediator.Send(new GetReviewCommentsQuery(token), ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error, code = result.ErrorCode });

        return Ok(result.Value);
    }

    // ── POST /api/v1/review/{token}/comments ───────────────────────────────

    [AllowAnonymous]
    [HttpPost("api/v{version:apiVersion}/review/{token}/comments")]
    [ProducesResponseType(typeof(ReviewCommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddReviewComment(
        string token,
        [FromBody] AddReviewCommentRequest req,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new AddReviewCommentCommand(token, req.AuthorName, req.Text, req.TimestampSeconds), ct);

        if (!result.IsSuccess)
            return result.ErrorCode switch
            {
                "INVALID_TOKEN" => NotFound(new { error = result.Error, code = result.ErrorCode }),
                _               => BadRequest(new { error = result.Error, code = result.ErrorCode }),
            };

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    // ── PATCH /api/v1/review/{token}/comments/{commentId}/resolve ──────────

    [HttpPatch("api/v{version:apiVersion}/review/{token}/comments/{commentId:guid}/resolve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveComment(
        string token, Guid commentId, CancellationToken ct)
    {
        var result = await mediator.Send(new ResolveReviewCommentCommand(commentId, GetCurrentUserId()), ct);

        if (!result.IsSuccess)
            return result.ErrorCode switch
            {
                "NOT_FOUND"   => NotFound(new { error = result.Error, code = result.ErrorCode }),
                "NOT_CREATOR" => StatusCode(StatusCodes.Status403Forbidden,
                                     new { error = result.Error, code = result.ErrorCode }),
                _             => BadRequest(new { error = result.Error, code = result.ErrorCode }),
            };

        return NoContent();
    }

    // ── GET /api/v1/teams/{teamId}/brand-kit ───────────────────────────────

    [HttpGet("api/v{version:apiVersion}/teams/{teamId:guid}/brand-kit")]
    [ProducesResponseType(typeof(BrandKitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBrandKit(Guid teamId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetBrandKitQuery(teamId), ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error, code = result.ErrorCode });

        return Ok(result.Value);
    }

    // ── PUT /api/v1/teams/{teamId}/brand-kit ───────────────────────────────

    [HttpPut("api/v{version:apiVersion}/teams/{teamId:guid}/brand-kit")]
    [ProducesResponseType(typeof(BrandKitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertBrandKit(
        Guid teamId,
        [FromForm] UpsertBrandKitRequest req,
        CancellationToken ct)
    {
        string? logoUrl      = null;
        string? logoBlobPath = null;

        if (req.LogoFile is { Length: > 0 })
        {
            logoBlobPath = $"brand-kits/{teamId}/logo{Path.GetExtension(req.LogoFile.FileName)}";
            await using var stream = req.LogoFile.OpenReadStream();
            logoUrl = await fileStorage.SaveFileAsync(stream, logoBlobPath, req.LogoFile.ContentType, ct);
        }

        var result = await mediator.Send(new UpsertBrandKitCommand(
            teamId,
            req.PrimaryColor,
            req.SecondaryColor,
            req.WatermarkPosition,
            req.WatermarkOpacity,
            logoUrl,
            logoBlobPath), ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error, code = result.ErrorCode });

        return Ok(result.Value);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private Guid GetCurrentUserId()
    {
        var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(str, out var id) ? id : Guid.Parse("00000000-0000-0000-0000-000000000001");
    }

    // ── Request models ──────────────────────────────────────────────────────

    public sealed record CreateReviewLinkRequest(
        Guid      EpisodeId,
        DateTime? ExpiresAt = null,
        string?   Password  = null);

    public sealed record AddReviewCommentRequest(
        string AuthorName,
        string Text,
        double TimestampSeconds);

    public sealed class UpsertBrandKitRequest
    {
        public string            PrimaryColor      { get; set; } = "#000000";
        public string            SecondaryColor    { get; set; } = "#FFFFFF";
        public WatermarkPosition WatermarkPosition { get; set; } = WatermarkPosition.BottomRight;
        public decimal           WatermarkOpacity  { get; set; } = 0.5m;
        public IFormFile?        LogoFile          { get; set; }
    }
}
