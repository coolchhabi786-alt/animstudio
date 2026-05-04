using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.ContentModule.Application.Queries.GetBrandKit;

public sealed record GetBrandKitQuery(Guid TeamId) : IRequest<Result<BrandKitDto>>;

public sealed class GetBrandKitHandler(IBrandKitRepository brandKits)
    : IRequestHandler<GetBrandKitQuery, Result<BrandKitDto>>
{
    public async Task<Result<BrandKitDto>> Handle(GetBrandKitQuery query, CancellationToken ct)
    {
        var kit = await brandKits.GetByTeamIdAsync(query.TeamId, ct);
        if (kit is null)
            return Result<BrandKitDto>.Failure("Brand kit not found for this team.", "NOT_FOUND");

        return Result<BrandKitDto>.Success(new BrandKitDto(
            kit.Id, kit.TeamId, kit.LogoUrl, kit.LogoBlobPath,
            kit.PrimaryColor, kit.SecondaryColor, kit.WatermarkPosition,
            kit.WatermarkOpacity, kit.CreatedAt, kit.UpdatedAt));
    }
}
