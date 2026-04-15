using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;
using System.Text.Json;

namespace AnimStudio.ContentModule.Application.Commands.SaveScript;

/// <summary>
/// Saves user-edited screenplay content. Marks the script as manually edited.
/// Validates that all characters referenced in dialogue exist in the episode roster.
/// </summary>
public sealed record SaveScriptCommand(Guid EpisodeId, ScreenplayDto Screenplay) : IRequest<Result<ScriptDto>>;

public sealed class SaveScriptValidator : AbstractValidator<SaveScriptCommand>
{
    public SaveScriptValidator()
    {
        RuleFor(x => x.EpisodeId).NotEmpty();
        RuleFor(x => x.Screenplay).NotNull();
        RuleFor(x => x.Screenplay.Title).NotEmpty().MaximumLength(500)
            .When(x => x.Screenplay is not null);
        RuleFor(x => x.Screenplay.Scenes).NotEmpty()
            .When(x => x.Screenplay is not null);
    }
}

public sealed class SaveScriptHandler(
    IScriptRepository scripts,
    ICharacterRepository characters,
    IEpisodeRepository episodes)
    : IRequestHandler<SaveScriptCommand, Result<ScriptDto>>
{
    public async Task<Result<ScriptDto>> Handle(SaveScriptCommand cmd, CancellationToken ct)
    {
        var episode = await episodes.GetByIdAsync(cmd.EpisodeId, ct);
        if (episode is null)
            return Result<ScriptDto>.Failure("Episode not found.", "NOT_FOUND");

        // Validate all characters referenced in dialogue exist in episode roster
        var roster = await characters.GetByEpisodeIdAsync(cmd.EpisodeId, ct);
        var rosterNames = roster.Select(c => c.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var invalidChars = cmd.Screenplay.Scenes
            .SelectMany(s => s.Dialogue)
            .Select(d => d.Character)
            .Where(name => !rosterNames.Contains(name))
            .Distinct()
            .ToList();

        if (invalidChars.Count > 0)
            return Result<ScriptDto>.Failure(
                $"Unknown character(s) in script: {string.Join(", ", invalidChars)}. All characters must exist in the episode roster.",
                "INVALID_CHARACTERS");

        var script = await scripts.GetByEpisodeIdAsync(cmd.EpisodeId, ct);
        var rawJson = JsonSerializer.Serialize(cmd.Screenplay);

        if (script is null)
        {
            script = Script.Create(cmd.EpisodeId, cmd.Screenplay.Title, rawJson);
            script.SaveManualEdits(rawJson); // mark as manually edited
            await scripts.AddAsync(script, ct);
        }
        else
        {
            script.SaveManualEdits(rawJson);
            await scripts.UpdateAsync(script, ct);
        }

        return Result<ScriptDto>.Success(MapToDto(script, cmd.Screenplay));
    }

    private static ScriptDto MapToDto(Script script, ScreenplayDto screenplay) =>
        new(script.Id, script.EpisodeId, script.Title, screenplay,
            script.IsManuallyEdited, script.DirectorNotes,
            script.CreatedAt, script.UpdatedAt);
}
