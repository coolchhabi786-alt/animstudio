using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.SharedKernel;
using FluentValidation;
using MediatR;

namespace AnimStudio.ContentModule.Application.Commands.PreviewVoice;

/// <summary>
/// Generates a TTS audio preview for the given text and voice.
/// Returns a signed Blob URL with 60-second expiry.
/// </summary>
public sealed record PreviewVoiceCommand(
    string Text,
    string VoiceName,
    string Language = "en-US") : IRequest<Result<VoicePreviewResponse>>;

public sealed class PreviewVoiceValidator : AbstractValidator<PreviewVoiceCommand>
{
    public PreviewVoiceValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
        RuleFor(x => x.VoiceName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Language).NotEmpty().MaximumLength(10);
    }
}

public sealed class PreviewVoiceHandler(IVoicePreviewService previewService)
    : IRequestHandler<PreviewVoiceCommand, Result<VoicePreviewResponse>>
{
    public async Task<Result<VoicePreviewResponse>> Handle(
        PreviewVoiceCommand cmd, CancellationToken ct)
    {
        var (audioUrl, expiresAt) = await previewService.GeneratePreviewAsync(
            cmd.Text, cmd.VoiceName, cmd.Language, ct);

        return Result<VoicePreviewResponse>.Success(
            new VoicePreviewResponse(audioUrl, expiresAt));
    }
}
