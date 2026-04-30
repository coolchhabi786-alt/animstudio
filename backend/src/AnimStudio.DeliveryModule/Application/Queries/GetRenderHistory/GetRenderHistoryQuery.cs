using AnimStudio.DeliveryModule.Application.Commands.StartRender;
using AnimStudio.DeliveryModule.Application.DTOs;
using AnimStudio.DeliveryModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.DeliveryModule.Application.Queries.GetRenderHistory;

public sealed record GetRenderHistoryQuery(Guid EpisodeId) : IRequest<Result<List<RenderDto>>>;

public sealed class GetRenderHistoryHandler(IRenderRepository renders)
    : IRequestHandler<GetRenderHistoryQuery, Result<List<RenderDto>>>
{
    public async Task<Result<List<RenderDto>>> Handle(
        GetRenderHistoryQuery query, CancellationToken ct)
    {
        var list = await renders.GetByEpisodeAsync(query.EpisodeId, ct);
        return Result<List<RenderDto>>.Success(list.ConvertAll(StartRenderHandler.MapDto));
    }
}
