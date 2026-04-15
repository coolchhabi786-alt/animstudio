using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.ContentModule.Infrastructure.Repositories;

/// <summary>
/// Read-only repository for <see cref="EpisodeTemplate"/>.
/// Only active templates (<c>IsActive = true</c>) are returned.
/// </summary>
public sealed class EpisodeTemplateRepository(ContentDbContext db) : IEpisodeTemplateRepository
{
    public async Task<List<EpisodeTemplate>> GetAllAsync(string? genre, CancellationToken ct)
    {
        var query = db.EpisodeTemplates
            .Where(t => t.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(genre) &&
            Enum.TryParse<Genre>(genre, ignoreCase: true, out var genreEnum))
        {
            query = query.Where(t => t.Genre == genreEnum);
        }

        return await query
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Title)
            .ToListAsync(ct);
    }

    public Task<EpisodeTemplate?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.EpisodeTemplates
            .Where(t => t.IsActive)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
}

/// <summary>
/// Read-only repository for <see cref="StylePreset"/>.
/// Only active presets (<c>IsActive = true</c>) are returned.
/// </summary>
public sealed class StylePresetRepository(ContentDbContext db) : IStylePresetRepository
{
    public Task<List<StylePreset>> GetAllAsync(CancellationToken ct)
        => db.StylePresets
            .Where(s => s.IsActive)
            .OrderBy(s => s.Style)
            .ToListAsync(ct);
}
