using Xunit;
using Moq;
using FluentAssertions;
using AnimStudio.ContentModule.Application.Commands.GenerateStoryboard;
using AnimStudio.ContentModule.Application.Commands.RegenerateShot;
using AnimStudio.ContentModule.Application.Commands.UpdateShotStyle;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;

namespace AnimStudio.UnitTests.Commands;

public class GenerateStoryboardCommandHandlerTests
{
    private readonly Mock<IEpisodeRepository> _episodes = new();
    private readonly Mock<IScriptRepository> _scripts = new();
    private readonly Mock<IStoryboardRepository> _storyboards = new();
    private readonly Mock<IJobRepository> _jobs = new();
    private readonly GenerateStoryboardHandler _handler;

    public GenerateStoryboardCommandHandlerTests()
    {
        _handler = new GenerateStoryboardHandler(
            _episodes.Object, _scripts.Object, _storyboards.Object, _jobs.Object);
    }

    [Fact]
    public async Task Handle_EpisodeNotFound_ReturnsNotFound()
    {
        var cmd = new GenerateStoryboardCommand(Guid.NewGuid(), null);
        _episodes.Setup(r => r.GetByIdAsync(cmd.EpisodeId, default))
            .ReturnsAsync((Episode?)null);

        var result = await _handler.Handle(cmd, default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_NoScript_ReturnsScriptNotReady()
    {
        var episodeId = Guid.NewGuid();
        _episodes.Setup(r => r.GetByIdAsync(episodeId, default))
            .ReturnsAsync(BuildEpisode(episodeId));
        _scripts.Setup(r => r.GetByEpisodeIdAsync(episodeId, default))
            .ReturnsAsync((Script?)null);

        var result = await _handler.Handle(
            new GenerateStoryboardCommand(episodeId, null), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("SCRIPT_NOT_READY");
    }

    [Fact]
    public async Task Handle_WithScript_EnqueuesStoryboardPlanJob()
    {
        var episodeId = Guid.NewGuid();
        var episode = BuildEpisode(episodeId);
        _episodes.Setup(r => r.GetByIdAsync(episodeId, default)).ReturnsAsync(episode);
        _scripts.Setup(r => r.GetByEpisodeIdAsync(episodeId, default))
            .ReturnsAsync(Script.Create(episodeId, "My Screenplay", "{}"));
        _storyboards.Setup(r => r.GetByEpisodeIdAsync(episodeId, default))
            .ReturnsAsync((Storyboard?)null);
        _jobs.Setup(r => r.GetByEpisodeIdAsync(episodeId, default))
            .ReturnsAsync(new List<Job>());

        var result = await _handler.Handle(
            new GenerateStoryboardCommand(episodeId, "tighter pacing"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Type.Should().Be(JobType.StoryboardPlan.ToString());
        _jobs.Verify(r => r.AddAsync(
            It.Is<Job>(j => j.Type == JobType.StoryboardPlan && j.EpisodeId == episodeId),
            default), Times.Once);
    }

    private static Episode BuildEpisode(Guid _)
        => Episode.Create(Guid.NewGuid(), "Ep", "idea", "Pixar3D");
}

public class RegenerateShotCommandHandlerTests
{
    private readonly Mock<IStoryboardRepository> _storyboards = new();
    private readonly Mock<IJobRepository> _jobs = new();
    private readonly RegenerateShotHandler _handler;

    public RegenerateShotCommandHandlerTests()
    {
        _handler = new RegenerateShotHandler(_storyboards.Object, _jobs.Object);
    }

    [Fact]
    public async Task Handle_ShotNotFound_ReturnsNotFound()
    {
        var shotId = Guid.NewGuid();
        _storyboards.Setup(r => r.GetByShotIdAsync(shotId, default))
            .ReturnsAsync((Storyboard?)null);

        var result = await _handler.Handle(new RegenerateShotCommand(shotId, null), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ValidShot_IncrementsCountAndQueuesJob()
    {
        var episodeId = Guid.NewGuid();
        var storyboard = Storyboard.Create(episodeId, "Title", "{}");
        storyboard.SeedShots(new[] { (1, 1, "Wide establishing shot") });
        var shot = storyboard.Shots.First();

        _storyboards.Setup(r => r.GetByShotIdAsync(shot.Id, default)).ReturnsAsync(storyboard);
        _jobs.Setup(r => r.GetByEpisodeIdAsync(episodeId, default))
            .ReturnsAsync(new List<Job>());

        var result = await _handler.Handle(new RegenerateShotCommand(shot.Id, "neon"), default);

        result.IsSuccess.Should().BeTrue();
        shot.RegenerationCount.Should().Be(1);
        shot.StyleOverride.Should().Be("neon");
        _jobs.Verify(r => r.AddAsync(
            It.Is<Job>(j => j.Type == JobType.StoryboardGen),
            default), Times.Once);
    }
}

public class UpdateShotStyleCommandHandlerTests
{
    private readonly Mock<IStoryboardRepository> _storyboards = new();
    private readonly Mock<IJobRepository> _jobs = new();
    private readonly UpdateShotStyleHandler _handler;

    public UpdateShotStyleCommandHandlerTests()
    {
        _handler = new UpdateShotStyleHandler(_storyboards.Object, _jobs.Object);
    }

    [Fact]
    public async Task Handle_UpdatesStyleOverride_AndQueuesGenJob()
    {
        var episodeId = Guid.NewGuid();
        var storyboard = Storyboard.Create(episodeId, "Title", "{}");
        storyboard.SeedShots(new[] { (1, 1, "A shot") });
        var shot = storyboard.Shots.First();

        _storyboards.Setup(r => r.GetByShotIdAsync(shot.Id, default)).ReturnsAsync(storyboard);
        _jobs.Setup(r => r.GetByEpisodeIdAsync(episodeId, default))
            .ReturnsAsync(new List<Job>());

        var result = await _handler.Handle(
            new UpdateShotStyleCommand(shot.Id, "watercolor, pastel palette"),
            default);

        result.IsSuccess.Should().BeTrue();
        shot.StyleOverride.Should().Be("watercolor, pastel palette");
        shot.RegenerationCount.Should().Be(1);
        _jobs.Verify(r => r.AddAsync(
            It.Is<Job>(j => j.Type == JobType.StoryboardGen),
            default), Times.Once);
    }
}
