using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Commands.CloneVoice;

/// <summary>
/// Initiates a voice cloning process from an audio sample.
/// Studio subscription tier only.
/// </summary>
public sealed record CloneVoiceCommand(
    Guid CharacterId,
    string? AudioSampleUrl = null) : IRequest<Result<VoiceCloneResponse>>;

public sealed class CloneVoiceValidator : AbstractValidator<CloneVoiceCommand>
{
    public CloneVoiceValidator()
    {
        RuleFor(x => x.CharacterId).NotEmpty();
    }
}

public sealed class CloneVoiceHandler(IVoiceCloneService cloneService)
    : IRequestHandler<CloneVoiceCommand, Result<VoiceCloneResponse>>
{
    public async Task<Result<VoiceCloneResponse>> Handle(
        CloneVoiceCommand cmd, CancellationToken ct)
    {
        var (voiceCloneUrl, status) = await cloneService.CloneVoiceAsync(
            cmd.CharacterId, cmd.AudioSampleUrl, ct);

        return Result<VoiceCloneResponse>.Success(
            new VoiceCloneResponse(voiceCloneUrl, status));
    }
}
