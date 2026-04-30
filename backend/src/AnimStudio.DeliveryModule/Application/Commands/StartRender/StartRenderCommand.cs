using AnimStudio.DeliveryModule.Application.DTOs;
using AnimStudio.DeliveryModule.Application.Interfaces;
using AnimStudio.DeliveryModule.Domain.Entities;
using AnimStudio.DeliveryModule.Domain.Enums;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.DeliveryModule.Application.Commands.StartRender;

public sealed record StartRenderCommand(
    Guid EpisodeId,
    RenderAspectRatio AspectRatio,
    Guid RequestedByUserId) : IRequest<Result<RenderDto>>;

public sealed class StartRenderValidator : AbstractValidator<StartRenderCommand>
{
    public StartRenderValidator()
    {
        RuleFor(x => x.EpisodeId).NotEmpty();
        RuleFor(x => x.RequestedByUserId).NotEmpty();
        RuleFor(x => x.AspectRatio).IsInEnum();
    }
}

public sealed class StartRenderHandler(IRenderRepository renders)
    : IRequestHandler<StartRenderCommand, Result<RenderDto>>
{
    public async Task<Result<RenderDto>> Handle(
        StartRenderCommand cmd, CancellationToken ct)
    {
        var render = Render.Create(cmd.EpisodeId, cmd.AspectRatio);
        await renders.AddAsync(render, ct);
        await renders.SaveChangesAsync(ct);

        return Result<RenderDto>.Success(MapDto(render));
    }

    internal static RenderDto MapDto(Render r) => new(
        r.Id,
        r.EpisodeId,
        r.AspectRatio,
        r.Status,
        r.FinalVideoUrl,
        r.CdnUrl,
        r.CaptionsSrtUrl,
        r.DurationSeconds,
        r.ErrorMessage,
        r.CreatedAt,
        r.CompletedAt);
}
