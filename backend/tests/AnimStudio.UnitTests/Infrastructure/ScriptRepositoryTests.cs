using Xunit;
using FluentAssertions;
using AnimStudio.ContentModule.Infrastructure.Repositories;
using AnimStudio.ContentModule.Infrastructure.Persistence;
using AnimStudio.ContentModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.UnitTests.Infrastructure;

public class ScriptRepositoryTests : IDisposable
{
    private readonly ContentDbContext _dbContext;
    private readonly ScriptRepository _repository;

    public ScriptRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: $"ScriptRepoTests_{Guid.NewGuid()}")
            .Options;

        _dbContext = new ContentDbContext(options);
        _repository = new ScriptRepository(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task GivenScript_WhenAddAsyncIsCalled_ThenSavesToDatabase()
    {
        // Arrange
        var episodeId = Guid.NewGuid();
        var script = Script.Create(episodeId, "Test Script", "{\"title\":\"Test\",\"scenes\":[]}");

        // Act
        await _repository.AddAsync(script);

        // Assert
        var saved = await _dbContext.Scripts.FirstOrDefaultAsync(s => s.EpisodeId == episodeId);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("Test Script");
        saved.IsManuallyEdited.Should().BeFalse();
    }

    [Fact]
    public async Task GivenExistingScript_WhenGetByEpisodeIdAsync_ThenReturnsScript()
    {
        // Arrange
        var episodeId = Guid.NewGuid();
        var script = Script.Create(episodeId, "Found Script", "{}");
        await _dbContext.Scripts.AddAsync(script);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEpisodeIdAsync(episodeId);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Found Script");
        result.EpisodeId.Should().Be(episodeId);
    }

    [Fact]
    public async Task GivenNoScript_WhenGetByEpisodeIdAsync_ThenReturnsNull()
    {
        // Act
        var result = await _repository.GetByEpisodeIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GivenExistingScript_WhenGetByIdAsync_ThenReturnsScript()
    {
        // Arrange
        var script = Script.Create(Guid.NewGuid(), "By ID", "{}");
        await _dbContext.Scripts.AddAsync(script);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(script.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(script.Id);
        result.Title.Should().Be("By ID");
    }

    [Fact]
    public async Task GivenNonExistingId_WhenGetByIdAsync_ThenReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GivenExistingScript_WhenUpdateAsync_ThenPersistsChanges()
    {
        // Arrange
        var episodeId = Guid.NewGuid();
        var script = Script.Create(episodeId, "Original", "{\"title\":\"Original\",\"scenes\":[]}");
        await _dbContext.Scripts.AddAsync(script);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act — load, modify, update
        var loaded = await _repository.GetByEpisodeIdAsync(episodeId);
        loaded.Should().NotBeNull();
        loaded!.SaveManualEdits("{\"title\":\"Edited\",\"scenes\":[]}");
        await _repository.UpdateAsync(loaded);

        // Assert
        _dbContext.ChangeTracker.Clear();
        var updated = await _dbContext.Scripts.FirstOrDefaultAsync(s => s.EpisodeId == episodeId);
        updated.Should().NotBeNull();
        updated!.IsManuallyEdited.Should().BeTrue();
        updated.RawJson.Should().Contain("Edited");
    }

    [Fact]
    public async Task GivenScript_WhenUpdateFromJob_ThenResetsManuallyEdited()
    {
        // Arrange
        var episodeId = Guid.NewGuid();
        var script = Script.Create(episodeId, "Initial", "{}");
        script.SaveManualEdits("{\"title\":\"Edited\",\"scenes\":[]}");
        await _dbContext.Scripts.AddAsync(script);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var loaded = await _repository.GetByEpisodeIdAsync(episodeId);
        loaded!.UpdateFromJob("{\"title\":\"Regenerated\",\"scenes\":[]}", "Regenerated");
        await _repository.UpdateAsync(loaded);

        // Assert
        _dbContext.ChangeTracker.Clear();
        var result = await _dbContext.Scripts.FirstOrDefaultAsync(s => s.EpisodeId == episodeId);
        result.Should().NotBeNull();
        result!.IsManuallyEdited.Should().BeFalse();
        result.Title.Should().Be("Regenerated");
    }

    [Fact]
    public async Task GivenScript_WhenSetDirectorNotes_ThenPersistsNotes()
    {
        // Arrange
        var episodeId = Guid.NewGuid();
        var script = Script.Create(episodeId, "With Notes", "{}");
        await _dbContext.Scripts.AddAsync(script);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var loaded = await _repository.GetByEpisodeIdAsync(episodeId);
        loaded!.SetDirectorNotes("Add more humor to scene 2");
        await _repository.UpdateAsync(loaded);

        // Assert
        _dbContext.ChangeTracker.Clear();
        var result = await _dbContext.Scripts.FirstOrDefaultAsync(s => s.EpisodeId == episodeId);
        result.Should().NotBeNull();
        result!.DirectorNotes.Should().Be("Add more humor to scene 2");
    }
}
