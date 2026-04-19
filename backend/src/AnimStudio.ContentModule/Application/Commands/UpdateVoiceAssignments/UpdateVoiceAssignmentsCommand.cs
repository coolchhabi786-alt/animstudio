using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Commands.UpdateVoiceAssignments;

/// <summary>
/// Batch upserts voice assignments for an episode. Creates new assignments or
/// updates existing ones. Returns the full list of assignments after the update.
/// </summary>
public sealed record UpdateVoiceAssignmentsCommand(
    Guid EpisodeId,
    List<VoiceAssignmentRequest> Assignments) : IRequest<Result<List<VoiceAssignmentDto>>>;

public sealed class UpdateVoiceAssignmentsValidator : AbstractValidator<UpdateVoiceAssignmentsCommand>
{
    public UpdateVoiceAssignmentsValidator()
    {
        RuleFor(x => x.EpisodeId).NotEmpty();
        RuleFor(x => x.Assignments).NotEmpty().WithMessage("At least one assignment is required.");
        RuleForEach(x => x.Assignments).ChildRules(a =>
        {
            a.RuleFor(r => r.CharacterId).NotEmpty();
            a.RuleFor(r => r.VoiceName).NotEmpty().MaximumLength(100);
            a.RuleFor(r => r.Language).NotEmpty().MaximumLength(10);
            a.RuleFor(r => r.VoiceCloneUrl).MaximumLength(2048)
                .When(r => r.VoiceCloneUrl is not null);
        });
    }
}

public sealed class UpdateVoiceAssignmentsHandler(
    IEpisodeRepository episodes,
    ICharacterRepository characters,
    IVoiceAssignmentRepository voiceAssignments)
    : IRequestHandler<UpdateVoiceAssignmentsCommand, Result<List<VoiceAssignmentDto>>>
{
    public async Task<Result<List<VoiceAssignmentDto>>> Handle(
        UpdateVoiceAssignmentsCommand cmd, CancellationToken ct)
    {
        var episode = await episodes.GetByIdAsync(cmd.EpisodeId, ct);
        if (episode is null)
            return Result<List<VoiceAssignmentDto>>.Failure("Episode not found.", "NOT_FOUND");

        var results = new List<VoiceAssignmentDto>();

        foreach (var req in cmd.Assignments)
        {
            // Validate that the character exists
            var character = await characters.GetByIdAsync(req.CharacterId, ct);
            if (character is null)
                return Result<List<VoiceAssignmentDto>>.Failure(
                    $"Character {req.CharacterId} not found.", "CHARACTER_NOT_FOUND");

            var existing = await voiceAssignments.GetByEpisodeAndCharacterAsync(
                cmd.EpisodeId, req.CharacterId, ct);

            if (existing is not null)
            {
                existing.Update(req.VoiceName, req.Language, req.VoiceCloneUrl);
                await voiceAssignments.UpdateAsync(existing, ct);

                results.Add(new VoiceAssignmentDto(
                    existing.Id, existing.EpisodeId, existing.CharacterId,
                    character.Name, existing.VoiceName, existing.Language,
                    existing.VoiceCloneUrl, existing.UpdatedAt));
            }
            else
            {
                var assignment = VoiceAssignment.Create(
                    cmd.EpisodeId, req.CharacterId,
                    req.VoiceName, req.Language, req.VoiceCloneUrl);

                await voiceAssignments.AddAsync(assignment, ct);

                results.Add(new VoiceAssignmentDto(
                    assignment.Id, assignment.EpisodeId, assignment.CharacterId,
                    character.Name, assignment.VoiceName, assignment.Language,
                    assignment.VoiceCloneUrl, assignment.UpdatedAt));
            }
        }

        return Result<List<VoiceAssignmentDto>>.Success(results);
    }
}
