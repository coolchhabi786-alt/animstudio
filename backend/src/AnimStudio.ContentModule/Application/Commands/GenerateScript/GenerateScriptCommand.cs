using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;
using System.Text.Json;

namespace AnimStudio.ContentModule.Application.Commands.GenerateScript;

/// <summary>Enqueues a Script writing job for an episode. Returns 202 + JobDto.</summary>
public sealed record GenerateScriptCommand(
    Guid EpisodeId,
    string? DirectorNotes = null,
    List<Guid>? ExistingCharacterIds = null,
    bool AllowNewCharacters = true,
    int? NewCharacterCount = null,
    List<string>? NewCharacterNames = null)
    : IRequest<Result<JobDto>>;

public sealed class GenerateScriptValidator : AbstractValidator<GenerateScriptCommand>
{
    public GenerateScriptValidator()
    {
        RuleFor(x => x.EpisodeId).NotEmpty();
    }
}

public sealed class GenerateScriptHandler(
    IEpisodeRepository episodes,
    IJobRepository jobs,
    ICharacterRepository characters)
    : IRequestHandler<GenerateScriptCommand, Result<JobDto>>
{
    public async Task<Result<JobDto>> Handle(GenerateScriptCommand cmd, CancellationToken ct)
    {
        var episode = await episodes.GetByIdAsync(cmd.EpisodeId, ct);
        if (episode is null)
            return Result<JobDto>.Failure("Episode not found.", "NOT_FOUND");

        if (string.IsNullOrWhiteSpace(episode.Idea))
            return Result<JobDto>.Failure(
                "Set a story idea before generating a script.",
                "IDEA_REQUIRED");

        var existingJobs = await jobs.GetByEpisodeIdAsync(cmd.EpisodeId, ct);
        var attempt = existingJobs.Count(j => j.Type == JobType.Script) + 1;

        // Load and auto-attach existing Ready characters selected by the user
        var existingCharacterData = new List<object>();
        if (cmd.ExistingCharacterIds is { Count: > 0 })
        {
            var existing = await characters.GetByIdsAsync(cmd.ExistingCharacterIds, ct);
            foreach (var c in existing)
            {
                var alreadyLinked = await characters.GetEpisodeCharacterAsync(cmd.EpisodeId, c.Id, ct);
                if (alreadyLinked is null)
                    await characters.AttachToEpisodeAsync(
                        new EpisodeCharacter { EpisodeId = cmd.EpisodeId, CharacterId = c.Id }, ct);

                existingCharacterData.Add(new
                {
                    name = c.Name,
                    description = c.Description,
                    styleDna = c.StyleDna,
                });
            }
        }

        // Extract sceneCount from CharacterPreferences JSON if present
        int? sceneCount = null;
        if (!string.IsNullOrWhiteSpace(episode.CharacterPreferences))
        {
            try
            {
                var prefs = JsonSerializer.Deserialize<JsonElement>(episode.CharacterPreferences);
                if (prefs.TryGetProperty("sceneCount", out var sc) && sc.ValueKind == JsonValueKind.Number)
                    sceneCount = sc.GetInt32();
            }
            catch (JsonException) { }
        }

        var payload = JsonSerializer.Serialize(new
        {
            episodeId = cmd.EpisodeId,
            jobType = JobType.Script.ToString(),
            attempt,
            directorNotes = cmd.DirectorNotes,
            idea = episode.Idea,
            characterPreferences = episode.CharacterPreferences,
            allowNewCharacters = cmd.AllowNewCharacters,
            existingCharacters = existingCharacterData,
            newCharacterCount = cmd.NewCharacterCount,
            newCharacterNames = cmd.NewCharacterNames ?? new List<string>(),
            sceneCount,
        });

        var job = Job.Create(cmd.EpisodeId, JobType.Script, payload, attempt);
        await jobs.AddAsync(job, ct);

        episode.Advance(EpisodeStatus.Script);
        await episodes.UpdateAsync(episode, ct);

        return Result<JobDto>.Success(new JobDto(
            job.Id,
            job.EpisodeId,
            job.Type.ToString(),
            job.Status.ToString(),
            job.Payload,
            null, null,
            job.QueuedAt,
            null, null,
            job.AttemptNumber));
    }
}
