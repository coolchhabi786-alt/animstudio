using Xunit;
using Moq;
using FluentAssertions;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Application.Queries.GetScript;
using AnimStudio.ContentModule.Domain.Entities;
using System.Text.Json;

namespace AnimStudio.UnitTests.Queries;

public class GetScriptQueryHandlerTests
{
    private readonly Mock<IScriptRepository> _scripts = new();
    private readonly Mock<IEpisodeRepository> _episodes = new();
    private readonly GetScriptHandler _handler;

    public GetScriptQueryHandlerTests()
    {
        _handler = new GetScriptHandler(_scripts.Object, _episodes.Object);
    }

    [Fact]
    public async Task Handle_EpisodeNotFound_ReturnsNotFoundFailure()
    {
        var episodeId = Guid.NewGuid();
        _episodes.Setup(r => r.GetByIdAsync(episodeId, default)).ReturnsAsync((Episode?)null);

        var result = await _handler.Handle(new GetScriptQuery(episodeId), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_NoScriptYet_ReturnsSuccessWithNullValue()
    {
        var episodeId = Guid.NewGuid();
        _episodes.Setup(r => r.GetByIdAsync(episodeId, default))
            .ReturnsAsync(Episode.Create(Guid.NewGuid(), "EP", "idea", "Anime"));
        _scripts.Setup(r => r.GetByEpisodeIdAsync(episodeId, default)).ReturnsAsync((Script?)null);

        var result = await _handler.Handle(new GetScriptQuery(episodeId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ScriptExists_ReturnsDeserializedScreenplay()
    {
        var episodeId = Guid.NewGuid();
        _episodes.Setup(r => r.GetByIdAsync(episodeId, default))
            .ReturnsAsync(Episode.Create(Guid.NewGuid(), "EP", "idea", "Anime"));

        var rawJson = JsonSerializer.Serialize(new
        {
            title = "Test Script",
            scenes = new[]
            {
                new
                {
                    sceneNumber = 1,
                    visualDescription = "Opening",
                    emotionalTone = "Happy",
                    dialogue = new[]
                    {
                        new { character = "Mr. Whiskers", text = "Meow!", startTime = 0.0, endTime = 1.5 }
                    }
                }
            }
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var script = Script.Create(episodeId, "Test Script", rawJson);
        _scripts.Setup(r => r.GetByEpisodeIdAsync(episodeId, default)).ReturnsAsync(script);

        var result = await _handler.Handle(new GetScriptQuery(episodeId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be("Test Script");
        result.Value.Screenplay.Scenes.Should().HaveCount(1);
        result.Value.Screenplay.Scenes[0].Dialogue.Should().HaveCount(1);
        result.Value.Screenplay.Scenes[0].Dialogue[0].Character.Should().Be("Mr. Whiskers");
    }
}
