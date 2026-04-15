using System.Text.Json;
using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Domain.Entities;

namespace AnimStudio.ContentModule.Application;

/// <summary>Domain entity → DTO mapping helpers, avoiding AutoMapper dependency.</summary>
internal static class ContentMappings
{
    public static ProjectDto ToDto(this Project p) => new(
        p.Id, p.TeamId, p.Name, p.Description, p.ThumbnailUrl, p.CreatedAt, p.UpdatedAt);

    public static EpisodeDto ToDto(this Episode e) => new(
        e.Id, e.ProjectId, e.Name, e.Idea, e.Style,
        e.Status.ToString(), e.TemplateId, e.DirectorNotes,
        e.CreatedAt, e.UpdatedAt, e.RenderedAt);

    public static JobDto ToDto(this Job j) => new(
        j.Id, j.EpisodeId,
        j.Type.ToString(), j.Status.ToString(),
        j.Payload, j.Result, j.ErrorMessage,
        j.QueuedAt, j.StartedAt, j.CompletedAt, j.AttemptNumber);

    public static TemplateDto ToDto(this EpisodeTemplate t)
    {
        var plot = JsonSerializer.Deserialize<JsonElement>(t.PlotStructure);
        return new TemplateDto(
            t.Id, t.Title, t.Genre.ToString(), t.Description, plot,
            t.DefaultStyle.ToString(), t.PreviewVideoUrl, t.ThumbnailUrl,
            t.IsActive, t.SortOrder);
    }

    public static StylePresetDto ToDto(this StylePreset s) => new(
        s.Id, s.Style.ToString(), s.DisplayName, s.Description,
        s.SampleImageUrl, s.FluxStylePromptSuffix, s.IsActive);
}
