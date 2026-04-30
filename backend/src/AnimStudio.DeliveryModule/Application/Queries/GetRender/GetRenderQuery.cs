using AnimStudio.DeliveryModule.Application.Commands.StartRender;
using AnimStudio.DeliveryModule.Application.DTOs;
using AnimStudio.DeliveryModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.DeliveryModule.Application.Queries.GetRender;

public sealed record GetRenderQuery(Guid RenderId) : IRequest<Result<RenderDto>>;

public sealed class GetRenderHandler(IRenderRepository renders)
    : IRequestHandler<GetRenderQuery, Result<RenderDto>>
{
    public async Task<Result<RenderDto>> Handle(GetRenderQuery query, CancellationToken ct)
    {
        var render = await renders.GetByIdAsync(query.RenderId, ct);
        if (render is null)
            return Result<RenderDto>.Failure("Render not found.", "NOT_FOUND");

        return Result<RenderDto>.Success(StartRenderHandler.MapDto(render));
    }
}
