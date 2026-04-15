using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using MediatR;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnimStudio.ContentModule.Application.Queries.GetScript;

/// <summary>Returns the current script for an episode, or null if none exists yet.</summary>
public sealed record GetScriptQuery(Guid EpisodeId) : IRequest<Result<ScriptDto?>>;

public sealed class GetScriptHandler(
    IScriptRepository scripts,
    IEpisodeRepository episodes)
    : IRequestHandler<GetScriptQuery, Result<ScriptDto?>>
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<Result<ScriptDto?>> Handle(GetScriptQuery query, CancellationToken ct)
    {
        var episode = await episodes.GetByIdAsync(query.EpisodeId, ct);
        if (episode is null)
            return Result<ScriptDto?>.Failure("Episode not found.", "NOT_FOUND");

        var script = await scripts.GetByEpisodeIdAsync(query.EpisodeId, ct);
        if (script is null)
            return Result<ScriptDto?>.Success(null);

        ScreenplayDto? screenplay = null;
        try
        {
            screenplay = JsonSerializer.Deserialize<ScreenplayDto>(script.RawJson, _jsonOptions);
        }
        catch (JsonException)
        {
            // Return a stub screenplay if the JSON is malformed (shouldn't happen)
            screenplay = new ScreenplayDto(script.Title, []);
        }

        screenplay ??= new ScreenplayDto(script.Title, []);

        return Result<ScriptDto?>.Success(new ScriptDto(
            script.Id,
            script.EpisodeId,
            script.Title,
            screenplay,
            script.IsManuallyEdited,
            script.DirectorNotes,
            script.CreatedAt,
            script.UpdatedAt));
    }
}
