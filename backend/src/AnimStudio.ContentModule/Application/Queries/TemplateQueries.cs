using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;

namespace AnimStudio.ContentModule.Application.Queries;

// ── GetTemplates ───────────────────────────────────────────────────────────────

/// <summary>
/// Returns all active episode templates.
/// Optional <see cref="Genre"/> filter maps to the genre enum name string (case-insensitive).
/// Cached for 1 hour — templates change only on deployment.
/// </summary>
public sealed record GetTemplatesQuery(string? Genre = null) : IRequest<Result<List<TemplateDto>>>, ICacheKey
{
    public string Key => Genre is null ? "templates:all" : $"templates:genre:{Genre.ToLowerInvariant()}";
    public TimeSpan CacheDuration => TimeSpan.FromHours(1);
}

public sealed class GetTemplatesHandler(IEpisodeTemplateRepository templates)
    : IRequestHandler<GetTemplatesQuery, Result<List<TemplateDto>>>
{
    public async Task<Result<List<TemplateDto>>> Handle(GetTemplatesQuery q, CancellationToken ct)
    {
        var list = await templates.GetAllAsync(q.Genre, ct);
        return Result<List<TemplateDto>>.Success(list.Select(t => t.ToDto()).ToList());
    }
}

// ── GetTemplate ───────────────────────────────────────────────────────────────

/// <summary>Returns a single episode template by ID. Cached for 1 hour.</summary>
public sealed record GetTemplateQuery(Guid Id) : IRequest<Result<TemplateDto>>, ICacheKey
{
    public string Key => $"template:{Id}";
    public TimeSpan CacheDuration => TimeSpan.FromHours(1);
}

public sealed class GetTemplateHandler(IEpisodeTemplateRepository templates)
    : IRequestHandler<GetTemplateQuery, Result<TemplateDto>>
{
    public async Task<Result<TemplateDto>> Handle(GetTemplateQuery q, CancellationToken ct)
    {
        var template = await templates.GetByIdAsync(q.Id, ct);
        return template is null
            ? Result<TemplateDto>.Failure("Template not found", "NOT_FOUND")
            : Result<TemplateDto>.Success(template.ToDto());
    }
}

// ── GetStylePresets ───────────────────────────────────────────────────────────

/// <summary>Returns all active visual style presets. Cached for 1 hour.</summary>
public sealed record GetStylePresetsQuery : IRequest<Result<List<StylePresetDto>>>, ICacheKey
{
    public string Key => "styles:all";
    public TimeSpan CacheDuration => TimeSpan.FromHours(1);
}

public sealed class GetStylePresetsHandler(IStylePresetRepository styles)
    : IRequestHandler<GetStylePresetsQuery, Result<List<StylePresetDto>>>
{
    public async Task<Result<List<StylePresetDto>>> Handle(GetStylePresetsQuery q, CancellationToken ct)
    {
        var list = await styles.GetAllAsync(ct);
        return Result<List<StylePresetDto>>.Success(list.Select(s => s.ToDto()).ToList());
    }
}
