using AnimStudio.ContentModule.Domain.Enums;

namespace AnimStudio.ContentModule.Application.DTOs;

public sealed record BrandKitDto(
    Guid              Id,
    Guid              TeamId,
    string?           LogoUrl,
    string?           LogoBlobPath,
    string            PrimaryColor,
    string            SecondaryColor,
    WatermarkPosition WatermarkPosition,
    decimal           WatermarkOpacity,
    DateTimeOffset    CreatedAt,
    DateTimeOffset    UpdatedAt);
