using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Commands.UpsertBrandKit;

public sealed record UpsertBrandKitCommand(
    Guid              TeamId,
    string            PrimaryColor,
    string            SecondaryColor,
    WatermarkPosition WatermarkPosition,
    decimal           WatermarkOpacity,
    string?           LogoUrl       = null,
    string?           LogoBlobPath  = null) : IRequest<Result<BrandKitDto>>;

public sealed class UpsertBrandKitValidator : AbstractValidator<UpsertBrandKitCommand>
{
    public UpsertBrandKitValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.PrimaryColor).NotEmpty().MaximumLength(20);
        RuleFor(x => x.SecondaryColor).NotEmpty().MaximumLength(20);
        RuleFor(x => x.WatermarkOpacity).InclusiveBetween(0m, 1m);
    }
}

public sealed class UpsertBrandKitHandler(IBrandKitRepository brandKits)
    : IRequestHandler<UpsertBrandKitCommand, Result<BrandKitDto>>
{
    public async Task<Result<BrandKitDto>> Handle(UpsertBrandKitCommand cmd, CancellationToken ct)
    {
        var existing = await brandKits.GetByTeamIdAsync(cmd.TeamId, ct);

        BrandKit kit;
        if (existing is null)
        {
            kit = BrandKit.Create(cmd.TeamId, cmd.PrimaryColor, cmd.SecondaryColor,
                cmd.WatermarkPosition, cmd.WatermarkOpacity, cmd.LogoUrl, cmd.LogoBlobPath);
            await brandKits.AddAsync(kit, ct);
        }
        else
        {
            existing.Update(cmd.PrimaryColor, cmd.SecondaryColor,
                cmd.WatermarkPosition, cmd.WatermarkOpacity, cmd.LogoUrl, cmd.LogoBlobPath);
            await brandKits.UpdateAsync(existing, ct);
            kit = existing;
        }

        return Result<BrandKitDto>.Success(MapToDto(kit));
    }

    internal static BrandKitDto MapToDto(BrandKit k)
        => new(k.Id, k.TeamId, k.LogoUrl, k.LogoBlobPath,
               k.PrimaryColor, k.SecondaryColor, k.WatermarkPosition,
               k.WatermarkOpacity, k.CreatedAt, k.UpdatedAt);
}
