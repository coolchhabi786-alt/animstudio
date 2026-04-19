using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Queries.GetVoiceAssignments;

/// <summary>
/// Returns all voice assignments for an episode, enriched with character names.
/// </summary>
public sealed record GetVoiceAssignmentsQuery(Guid EpisodeId) : IRequest<Result<List<VoiceAssignmentDto>>>;

public sealed class GetVoiceAssignmentsValidator : AbstractValidator<GetVoiceAssignmentsQuery>
{
    public GetVoiceAssignmentsValidator()
    {
        RuleFor(x => x.EpisodeId).NotEmpty();
    }
}

public sealed class GetVoiceAssignmentsHandler(
    IEpisodeRepository episodes,
    ICharacterRepository characters,
    IVoiceAssignmentRepository voiceAssignments)
    : IRequestHandler<GetVoiceAssignmentsQuery, Result<List<VoiceAssignmentDto>>>
{
    public async Task<Result<List<VoiceAssignmentDto>>> Handle(
        GetVoiceAssignmentsQuery query, CancellationToken ct)
    {
        var episode = await episodes.GetByIdAsync(query.EpisodeId, ct);
        if (episode is null)
            return Result<List<VoiceAssignmentDto>>.Failure("Episode not found.", "NOT_FOUND");

        var assignments = await voiceAssignments.GetByEpisodeIdAsync(query.EpisodeId, ct);

        var dtos = new List<VoiceAssignmentDto>();
        foreach (var a in assignments)
        {
            var character = await characters.GetByIdAsync(a.CharacterId, ct);
            var characterName = character?.Name ?? "Unknown";

            dtos.Add(new VoiceAssignmentDto(
                a.Id, a.EpisodeId, a.CharacterId,
                characterName, a.VoiceName, a.Language,
                a.VoiceCloneUrl, a.UpdatedAt));
        }

        return Result<List<VoiceAssignmentDto>>.Success(dtos);
    }
}
