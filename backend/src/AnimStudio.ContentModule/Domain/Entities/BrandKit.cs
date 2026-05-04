using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// Team-scoped branding settings applied during post-production render.
/// One BrandKit per team (enforced by unique index on TeamId).
/// </summary>
public sealed class BrandKit : AggregateRoot<Guid>
{
    public Guid TeamId { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? LogoBlobPath { get; private set; }
    public string PrimaryColor { get; private set; } = "#000000";
    public string SecondaryColor { get; private set; } = "#FFFFFF";
    public WatermarkPosition WatermarkPosition { get; private set; } = WatermarkPosition.BottomRight;

    /// <summary>0.0 (transparent) to 1.0 (fully opaque).</summary>
    public decimal WatermarkOpacity { get; private set; } = 0.5m;

    private BrandKit() { }

    public static BrandKit Create(
        Guid teamId,
        string primaryColor,
        string secondaryColor,
        WatermarkPosition watermarkPosition = WatermarkPosition.BottomRight,
        decimal watermarkOpacity = 0.5m,
        string? logoUrl = null,
        string? logoBlobPath = null)
    {
        if (teamId == Guid.Empty)
            throw new ArgumentException("Team ID is required.", nameof(teamId));

        return new BrandKit
        {
            Id                = Guid.NewGuid(),
            TeamId            = teamId,
            PrimaryColor      = primaryColor,
            SecondaryColor    = secondaryColor,
            WatermarkPosition = watermarkPosition,
            WatermarkOpacity  = Math.Clamp(watermarkOpacity, 0m, 1m),
            LogoUrl           = logoUrl,
            LogoBlobPath      = logoBlobPath,
            CreatedAt         = DateTimeOffset.UtcNow,
            UpdatedAt         = DateTimeOffset.UtcNow,
        };
    }

    public void Update(
        string primaryColor,
        string secondaryColor,
        WatermarkPosition watermarkPosition,
        decimal watermarkOpacity,
        string? logoUrl,
        string? logoBlobPath)
    {
        PrimaryColor      = primaryColor;
        SecondaryColor    = secondaryColor;
        WatermarkPosition = watermarkPosition;
        WatermarkOpacity  = Math.Clamp(watermarkOpacity, 0m, 1m);
        LogoUrl           = logoUrl;
        LogoBlobPath      = logoBlobPath;
        UpdatedAt         = DateTimeOffset.UtcNow;
    }
}
