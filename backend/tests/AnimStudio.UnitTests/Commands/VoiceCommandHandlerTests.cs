using Xunit;
using Moq;
using FluentAssertions;
using AnimStudio.ContentModule.Application.Commands.UpdateVoiceAssignments;
using AnimStudio.ContentModule.Application.Commands.PreviewVoice;
using AnimStudio.ContentModule.Application.Commands.CloneVoice;
using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Application.Queries.GetVoiceAssignments;
using AnimStudio.ContentModule.Domain.Entities;

namespace AnimStudio.UnitTests.Commands;

public class UpdateVoiceAssignmentsCommandHandlerTests
{
    private readonly Mock<IEpisodeRepository> _episodes = new();
    private readonly Mock<ICharacterRepository> _characters = new();
    private readonly Mock<IVoiceAssignmentRepository> _voiceAssignments = new();
    private readonly UpdateVoiceAssignmentsHandler _handler;

    public UpdateVoiceAssignmentsCommandHandlerTests()
    {
        _handler = new UpdateVoiceAssignmentsHandler(
            _episodes.Object, _characters.Object, _voiceAssignments.Object);
    }

    [Fact]
    public async Task Handle_EpisodeNotFound_ReturnsNotFound()
    {
        var cmd = new UpdateVoiceAssignmentsCommand(Guid.NewGuid(), new List<VoiceAssignmentRequest>
        {
            new(Guid.NewGuid(), "Alloy", "en-US", null)
        });
        _episodes.Setup(r => r.GetByIdAsync(cmd.EpisodeId, default))
            .ReturnsAsync((Episode?)null);

        var result = await _handler.Handle(cmd, default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_CharacterNotFound_ReturnsCharacterNotFound()
    {
        var episodeId = Guid.NewGuid();
        var characterId = Guid.NewGuid();
        var episode = Episode.Create(Guid.NewGuid(), "Ep", "idea", "Pixar3D");

        _episodes.Setup(r => r.GetByIdAsync(episodeId, default)).ReturnsAsync(episode);
        _characters.Setup(r => r.GetByIdAsync(characterId, default))
            .ReturnsAsync((Character?)null);

        var cmd = new UpdateVoiceAssignmentsCommand(episodeId, new List<VoiceAssignmentRequest>
        {
            new(characterId, "Alloy", "en-US", null)
        });

        var result = await _handler.Handle(cmd, default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("CHARACTER_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_NewAssignment_CreatesVoiceAssignment()
    {
        var episodeId = Guid.NewGuid();
        var characterId = Guid.NewGuid();
        var episode = Episode.Create(Guid.NewGuid(), "Ep", "idea", "Pixar3D");
        var character = Character.Create(Guid.NewGuid(), "Alice", "A character", null, 50);

        _episodes.Setup(r => r.GetByIdAsync(episodeId, default)).ReturnsAsync(episode);
        _characters.Setup(r => r.GetByIdAsync(characterId, default)).ReturnsAsync(character);
        _voiceAssignments.Setup(r => r.GetByEpisodeAndCharacterAsync(episodeId, characterId, default))
            .ReturnsAsync((VoiceAssignment?)null);

        var cmd = new UpdateVoiceAssignmentsCommand(episodeId, new List<VoiceAssignmentRequest>
        {
            new(characterId, "Nova", "en-US", null)
        });

        var result = await _handler.Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value![0].VoiceName.Should().Be("Nova");
        result.Value![0].CharacterName.Should().Be("Alice");
        _voiceAssignments.Verify(r => r.AddAsync(
            It.Is<VoiceAssignment>(v => v.VoiceName == "Nova" && v.EpisodeId == episodeId),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_OrphanedAssignment_SoftDeletes()
    {
        var episodeId = Guid.NewGuid();
        var keptCharacterId = Guid.NewGuid();
        var orphanCharacterId = Guid.NewGuid();
        var episode = Episode.Create(Guid.NewGuid(), "Ep", "idea", "Pixar3D");
        var keptCharacter = Character.Create(Guid.NewGuid(), "Alice", "A character", null, 50);
        var orphan = VoiceAssignment.Create(episodeId, orphanCharacterId, "Echo", "en-US");
        var kept = VoiceAssignment.Create(episodeId, keptCharacterId, "Alloy", "en-US");

        _episodes.Setup(r => r.GetByIdAsync(episodeId, default)).ReturnsAsync(episode);
        _voiceAssignments.Setup(r => r.GetByEpisodeIdAsync(episodeId, default))
            .ReturnsAsync(new List<VoiceAssignment> { kept, orphan });
        _characters.Setup(r => r.GetByIdAsync(keptCharacterId, default)).ReturnsAsync(keptCharacter);
        _voiceAssignments.Setup(r => r.GetByEpisodeAndCharacterAsync(episodeId, keptCharacterId, default))
            .ReturnsAsync(kept);

        // Only the kept character is in the incoming list
        var cmd = new UpdateVoiceAssignmentsCommand(episodeId, new List<VoiceAssignmentRequest>
        {
            new(keptCharacterId, "Nova", "en-US", null)
        });

        var result = await _handler.Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        // Orphaned assignment must be soft-deleted
        _voiceAssignments.Verify(r => r.SoftDeleteAsync(orphan, default), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingAssignment_UpdatesVoiceAssignment()
    {
        var episodeId = Guid.NewGuid();
        var characterId = Guid.NewGuid();
        var episode = Episode.Create(Guid.NewGuid(), "Ep", "idea", "Pixar3D");
        var character = Character.Create(Guid.NewGuid(), "Bob", "A character", null, 50);
        var existing = VoiceAssignment.Create(episodeId, characterId, "Alloy", "en-US");

        _episodes.Setup(r => r.GetByIdAsync(episodeId, default)).ReturnsAsync(episode);
        _characters.Setup(r => r.GetByIdAsync(characterId, default)).ReturnsAsync(character);
        _voiceAssignments.Setup(r => r.GetByEpisodeAndCharacterAsync(episodeId, characterId, default))
            .ReturnsAsync(existing);

        var cmd = new UpdateVoiceAssignmentsCommand(episodeId, new List<VoiceAssignmentRequest>
        {
            new(characterId, "Echo", "es-ES", null)
        });

        var result = await _handler.Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        result.Value![0].VoiceName.Should().Be("Echo");
        result.Value![0].Language.Should().Be("es-ES");
        existing.VoiceName.Should().Be("Echo");
        _voiceAssignments.Verify(r => r.UpdateAsync(existing, default), Times.Once);
    }
}

public class PreviewVoiceCommandHandlerTests
{
    private readonly Mock<IVoicePreviewService> _previewService = new();
    private readonly PreviewVoiceHandler _handler;

    public PreviewVoiceCommandHandlerTests()
    {
        _handler = new PreviewVoiceHandler(_previewService.Object);
    }

    [Fact]
    public async Task Handle_ReturnsAudioUrl()
    {
        var expectedUrl = "https://blob.local/tts/preview.mp3";
        var expectedExpiry = DateTimeOffset.UtcNow.AddSeconds(60);
        _previewService
            .Setup(s => s.GeneratePreviewAsync("Hello", "Alloy", "en-US", default))
            .ReturnsAsync((expectedUrl, expectedExpiry));

        var result = await _handler.Handle(
            new PreviewVoiceCommand("Hello", "Alloy", "en-US"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AudioUrl.Should().Be(expectedUrl);
        result.Value!.ExpiresAt.Should().Be(expectedExpiry);
    }
}

public class CloneVoiceCommandHandlerTests
{
    private readonly Mock<IVoiceCloneService> _cloneService = new();
    private readonly CloneVoiceHandler _handler;

    public CloneVoiceCommandHandlerTests()
    {
        _handler = new CloneVoiceHandler(_cloneService.Object);
    }

    [Fact]
    public async Task Handle_ReturnsNotAvailableStatus()
    {
        var characterId = Guid.NewGuid();
        _cloneService
            .Setup(s => s.CloneVoiceAsync(characterId, null, default))
            .ReturnsAsync(((string?)null, "NotAvailable"));

        var result = await _handler.Handle(
            new CloneVoiceCommand(characterId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be("NotAvailable");
        result.Value!.VoiceCloneUrl.Should().BeNull();
    }
}

public class GetVoiceAssignmentsQueryHandlerTests
{
    private readonly Mock<IEpisodeRepository> _episodes = new();
    private readonly Mock<ICharacterRepository> _characters = new();
    private readonly Mock<IVoiceAssignmentRepository> _voiceAssignments = new();
    private readonly GetVoiceAssignmentsHandler _handler;

    public GetVoiceAssignmentsQueryHandlerTests()
    {
        _handler = new GetVoiceAssignmentsHandler(
            _episodes.Object, _characters.Object, _voiceAssignments.Object);
    }

    [Fact]
    public async Task Handle_EpisodeNotFound_ReturnsNotFound()
    {
        _episodes.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Episode?)null);

        var result = await _handler.Handle(
            new GetVoiceAssignmentsQuery(Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_WithAssignments_ReturnsEnrichedDtos()
    {
        var episodeId = Guid.NewGuid();
        var characterId = Guid.NewGuid();
        var episode = Episode.Create(Guid.NewGuid(), "Ep", "idea", "Pixar3D");
        var character = Character.Create(Guid.NewGuid(), "Alice", "Desc", null, 50);
        var assignment = VoiceAssignment.Create(episodeId, characterId, "Nova", "en-US");

        _episodes.Setup(r => r.GetByIdAsync(episodeId, default)).ReturnsAsync(episode);
        _voiceAssignments.Setup(r => r.GetByEpisodeIdAsync(episodeId, default))
            .ReturnsAsync(new List<VoiceAssignment> { assignment });
        _characters.Setup(r => r.GetByIdAsync(characterId, default)).ReturnsAsync(character);

        var result = await _handler.Handle(new GetVoiceAssignmentsQuery(episodeId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value![0].CharacterName.Should().Be("Alice");
        result.Value![0].VoiceName.Should().Be("Nova");
    }
}
