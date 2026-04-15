using Xunit;
using Moq;
using FluentAssertions;
using AnimStudio.ContentModule.Application.Commands.GenerateScript;
using AnimStudio.ContentModule.Application.Commands.SaveScript;
using AnimStudio.ContentModule.Application.Commands.RegenerateScript;
using AnimStudio.ContentModule.Application.DTOs;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;

namespace AnimStudio.UnitTests.Commands;

public class GenerateScriptCommandHandlerTests
{
    private readonly Mock<IEpisodeRepository> _episodes = new();
    private readonly Mock<IJobRepository> _jobs = new();
    private readonly Mock<ISagaStateRepository> _sagas = new();
    private readonly Mock<ICharacterRepository> _characters = new();
    private readonly GenerateScriptHandler _handler;

    public GenerateScriptCommandHandlerTests()
    {
        _handler = new GenerateScriptHandler(_episodes.Object, _jobs.Object, _sagas.Object, _characters.Object);
    }

    [Fact]
    public async Task Handle_EpisodeNotFound_ReturnsFailure()
    {
        // Arrange
        var episodeId = Guid.NewGuid();
        _episodes.Setup(r => r.GetByIdAsync(episodeId, default)).ReturnsAsync((Episode?)null);
        var cmd = new GenerateScriptCommand(episodeId, null);

        // Act
        var result = await _handler.Handle(cmd, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_NoReadyCharacters_ReturnsCharactersNotReadyFailure()
    {
        // Arrange
        var episodeId = Guid.NewGuid();
        _episodes.Setup(r => r.GetByIdAsync(episodeId, default))
            .ReturnsAsync(BuildEpisode(episodeId));

        // Character in Training state — not Ready
        var trainingChar = BuildCharacter(TrainingStatus.Training);
        _characters.Setup(r => r.GetByEpisodeIdAsync(episodeId, default))
            .ReturnsAsync(new List<Character> { trainingChar });

        _jobs.Setup(r => r.GetByEpisodeIdAsync(episodeId, default))
             .ReturnsAsync(new List<Job>());

        var cmd = new GenerateScriptCommand(episodeId, null);

        // Act
        var result = await _handler.Handle(cmd, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("CHARACTERS_NOT_READY");
    }

    [Fact]
    public async Task Handle_WithReadyCharacter_EnqueuesJobAndReturnsAccepted()
    {
        // Arrange
        var episodeId = Guid.NewGuid();
        _episodes.Setup(r => r.GetByIdAsync(episodeId, default))
            .ReturnsAsync(BuildEpisode(episodeId));

        _characters.Setup(r => r.GetByEpisodeIdAsync(episodeId, default))
            .ReturnsAsync(new List<Character> { BuildCharacter(TrainingStatus.Ready) });

        _jobs.Setup(r => r.GetByEpisodeIdAsync(episodeId, default))
             .ReturnsAsync(new List<Job>());

        _jobs.Setup(r => r.AddAsync(It.IsAny<Job>(), default)).Returns(Task.CompletedTask);
        _episodes.Setup(r => r.UpdateAsync(It.IsAny<Episode>(), default)).Returns(Task.CompletedTask);

        var cmd = new GenerateScriptCommand(episodeId, "Make it funny");

        // Act
        var result = await _handler.Handle(cmd, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Type.Should().Be("Script");
        _jobs.Verify(r => r.AddAsync(It.IsAny<Job>(), default), Times.Once);
        _episodes.Verify(r => r.UpdateAsync(It.IsAny<Episode>(), default), Times.Once);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Episode BuildEpisode(Guid id)
        => Episode.Create(Guid.NewGuid(), "Test Episode", "A test idea", "Anime");

    private static Character BuildCharacter(TrainingStatus status)
    {
        var c = Character.Create(Guid.NewGuid(), "Test Char", "Description", "Style DNA");
        if (status == TrainingStatus.Ready)
        {
            c.UpdateTrainingStatus(TrainingStatus.Ready, 100,
                "https://example.com/lora.safetensors", "char_test");
        }
        return c;
    }
}

public class SaveScriptCommandHandlerTests
{
    private readonly Mock<IScriptRepository> _scripts = new();
    private readonly Mock<ICharacterRepository> _characters = new();
    private readonly Mock<IEpisodeRepository> _episodes = new();
    private readonly SaveScriptHandler _handler;

    public SaveScriptCommandHandlerTests()
    {
        _handler = new SaveScriptHandler(_scripts.Object, _characters.Object, _episodes.Object);
    }

    [Fact]
    public async Task Handle_EpisodeNotFound_ReturnsFailure()
    {
        var episodeId = Guid.NewGuid();
        _episodes.Setup(r => r.GetByIdAsync(episodeId, default)).ReturnsAsync((Episode?)null);

        var cmd = new SaveScriptCommand(episodeId, new ScreenplayDto("Title", []));
        var result = await _handler.Handle(cmd, default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_InvalidCharacterInDialogue_ReturnsInvalidCharactersFailure()
    {
        var episodeId = Guid.NewGuid();
        _episodes.Setup(r => r.GetByIdAsync(episodeId, default))
            .ReturnsAsync(Episode.Create(Guid.NewGuid(), "EP", "idea", "Anime"));
        _characters.Setup(r => r.GetByEpisodeIdAsync(episodeId, default))
            .ReturnsAsync(new List<Character>()); // empty roster

        var screenplay = new ScreenplayDto("My Script", [
            new SceneDto(1, "Opening shot", "Happy", [
                new DialogueLineDto("UnknownCharacter", "Hello world", 0.0, 2.5)
            ])
        ]);

        var result = await _handler.Handle(new SaveScriptCommand(episodeId, screenplay), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_CHARACTERS");
        result.Error.Should().Contain("UnknownCharacter");
    }

    [Fact]
    public async Task Handle_ValidScript_CreatesScriptAndMarksManuallyEdited()
    {
        var episodeId = Guid.NewGuid();
        _episodes.Setup(r => r.GetByIdAsync(episodeId, default))
            .ReturnsAsync(Episode.Create(Guid.NewGuid(), "EP", "idea", "Anime"));

        var character = Character.Create(Guid.NewGuid(), "Mr. Whiskers", "A cat", "Anime cat style");
        _characters.Setup(r => r.GetByEpisodeIdAsync(episodeId, default))
            .ReturnsAsync(new List<Character> { character });

        _scripts.Setup(r => r.GetByEpisodeIdAsync(episodeId, default)).ReturnsAsync((Script?)null);
        _scripts.Setup(r => r.AddAsync(It.IsAny<Script>(), default)).Returns(Task.CompletedTask);

        var screenplay = new ScreenplayDto("My Script", [
            new SceneDto(1, "Opening", "Happy", [
                new DialogueLineDto("Mr. Whiskers", "Meow!", 0.0, 1.5)
            ])
        ]);

        var result = await _handler.Handle(new SaveScriptCommand(episodeId, screenplay), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsManuallyEdited.Should().BeTrue();
        _scripts.Verify(r => r.AddAsync(It.IsAny<Script>(), default), Times.Once);
    }
}
