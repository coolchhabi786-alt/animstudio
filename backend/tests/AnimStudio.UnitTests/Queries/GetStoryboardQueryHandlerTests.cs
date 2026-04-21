using Xunit;
using Moq;
using FluentAssertions;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Application.Queries.GetStoryboard;
using AnimStudio.ContentModule.Domain.Entities;

namespace AnimStudio.UnitTests.Queries;

public class GetStoryboardQueryHandlerTests
{
    private readonly Mock<IStoryboardRepository> _storyboards = new();
    private readonly Mock<IEpisodeRepository> _episodes = new();
    private readonly GetStoryboardHandler _handler;

    public GetStoryboardQueryHandlerTests()
    {
        _handler = new GetStoryboardHandler(_storyboards.Object, _episodes.Object);
    }

    [Fact]
    public async Task Handle_EpisodeNotFound_ReturnsNotFoundFailure()
    {
        var episodeId = Guid.NewGuid();
        _episodes.Setup(r => r.GetByIdAsync(episodeId, default)).ReturnsAsync((Episode?)null);

        var result = await _handler.Handle(new GetStoryboardQuery(episodeId), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_NoStoryboardYet_ReturnsSuccessWithNullValue()
    {
        var episodeId = Guid.NewGuid();
        _episodes.Setup(r => r.GetByIdAsync(episodeId, default))
            .ReturnsAsync(Episode.Create(Guid.NewGuid(), "Ep", "idea", "Anime"));
        _storyboards.Setup(r => r.GetByEpisodeIdAsync(episodeId, default))
            .ReturnsAsync((Storyboard?)null);

        var result = await _handler.Handle(new GetStoryboardQuery(episodeId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_StoryboardExists_ReturnsDtoWithShotsOrderedBySceneThenShot()
    {
        var episodeId = Guid.NewGuid();
        _episodes.Setup(r => r.GetByIdAsync(episodeId, default))
            .ReturnsAsync(Episode.Create(Guid.NewGuid(), "Ep", "idea", "Pixar3D"));

        var storyboard = Storyboard.Create(episodeId, "My Screenplay", "{}");
        storyboard.SeedShots(new[]
        {
            (2, 1, "Scene 2 Shot 1"),
            (1, 2, "Scene 1 Shot 2"),
            (1, 1, "Scene 1 Shot 1"),
        });
        _storyboards.Setup(r => r.GetByEpisodeIdAsync(episodeId, default))
            .ReturnsAsync(storyboard);

        var result = await _handler.Handle(new GetStoryboardQuery(episodeId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.EpisodeId.Should().Be(episodeId);
        result.Value.ScreenplayTitle.Should().Be("My Screenplay");
        result.Value.Shots.Should().HaveCount(3);

        // Verify ordering: scene 1 shot 1 → scene 1 shot 2 → scene 2 shot 1
        result.Value.Shots[0].SceneNumber.Should().Be(1);
        result.Value.Shots[0].ShotIndex.Should().Be(1);
        result.Value.Shots[0].Description.Should().Be("Scene 1 Shot 1");

        result.Value.Shots[1].SceneNumber.Should().Be(1);
        result.Value.Shots[1].ShotIndex.Should().Be(2);

        result.Value.Shots[2].SceneNumber.Should().Be(2);
        result.Value.Shots[2].ShotIndex.Should().Be(1);
    }

    [Fact]
    public async Task Handle_StoryboardWithStyleOverride_MapsStyleOverrideToDto()
    {
        var episodeId = Guid.NewGuid();
        _episodes.Setup(r => r.GetByIdAsync(episodeId, default))
            .ReturnsAsync(Episode.Create(Guid.NewGuid(), "Ep", "idea", "Anime"));

        var storyboard = Storyboard.Create(episodeId, "Animated Title", "{}");
        storyboard.SeedShots(new[] { (1, 1, "Opening shot") });
        var shot = storyboard.Shots.First();
        storyboard.SetShotStyleOverride(shot.Id, "watercolor");

        _storyboards.Setup(r => r.GetByEpisodeIdAsync(episodeId, default))
            .ReturnsAsync(storyboard);

        var result = await _handler.Handle(new GetStoryboardQuery(episodeId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Shots.Single().StyleOverride.Should().Be("watercolor");
    }
}
