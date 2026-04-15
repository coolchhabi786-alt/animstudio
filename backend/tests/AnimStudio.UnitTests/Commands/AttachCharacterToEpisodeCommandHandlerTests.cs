using AnimStudio.ContentModule.Application.Commands.AttachCharacter;
using AnimStudio.ContentModule.Application.Queries.GetCharacter;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using Moq;
using Xunit;

namespace AnimStudio.UnitTests.Commands;

public sealed class AttachCharacterToEpisodeCommandHandlerTests
{
    private readonly ContentDbContext _db;
    private readonly AttachCharacterCommandHandler _handler;

    public AttachCharacterToEpisodeCommandHandlerTests()
    {
        _db = TestDbContextFactory.CreateContentDb();
        _handler = new AttachCharacterCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_CharacterReady_AttachesSuccessfully()
    {
        // Arrange
        var episodeId = Guid.NewGuid();
        var character = Character.Create(
            Guid.NewGuid(), "Hero Cat", null, null, Guid.NewGuid());

        // Advance to Ready state
        character.AdvanceTraining(
            Domain.Enums.TrainingStatus.Training, 50, null, null, null);
        character.AdvanceTraining(
            Domain.Enums.TrainingStatus.Ready, 100, "https://img.test/hero.png",
            "https://lora.test/hero.safetensors", "hero_cat_v1");

        await _db.Characters.AddAsync(character);
        await _db.SaveChangesAsync();

        var cmd = new AttachCharacterCommand(episodeId, character.Id);

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var link = _db.EpisodeCharacters
            .FirstOrDefault(ec => ec.EpisodeId == episodeId && ec.CharacterId == character.Id);
        Assert.NotNull(link);
    }

    [Fact]
    public async Task Handle_CharacterNotReady_ReturnsBadRequest()
    {
        // Arrange
        var character = Character.Create(
            Guid.NewGuid(), "Draft Cat", null, null, Guid.NewGuid());
        await _db.Characters.AddAsync(character);
        await _db.SaveChangesAsync();

        var cmd = new AttachCharacterCommand(Guid.NewGuid(), character.Id);

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("not Ready", result.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_DuplicateAttach_ReturnsOkIdempotent()
    {
        // Arrange
        var episodeId = Guid.NewGuid();
        var character = Character.Create(
            Guid.NewGuid(), "Dupe Cat", null, null, Guid.NewGuid());
        character.AdvanceTraining(Domain.Enums.TrainingStatus.Ready, 100,
            "https://img.test/dupe.png", "https://lora.test/dupe.safetensors", "dupe_v1");

        var existing = new EpisodeCharacter { EpisodeId = episodeId, CharacterId = character.Id };
        await _db.Characters.AddAsync(character);
        await _db.EpisodeCharacters.AddAsync(existing);
        await _db.SaveChangesAsync();

        var cmd = new AttachCharacterCommand(episodeId, character.Id);

        // Act — should not throw, just return success
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var linkCount = _db.EpisodeCharacters
            .Count(ec => ec.EpisodeId == episodeId && ec.CharacterId == character.Id);
        Assert.Equal(1, linkCount);
    }
}
