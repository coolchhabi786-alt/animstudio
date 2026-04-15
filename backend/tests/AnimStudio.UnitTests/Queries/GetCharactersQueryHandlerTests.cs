using AnimStudio.ContentModule.Application.Queries.GetCharacters;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;
using Moq;
using Xunit;

namespace AnimStudio.UnitTests.Queries;

public sealed class GetCharactersQueryHandlerTests
{
    private readonly Mock<ICharacterRepository> _repoMock;
    private readonly Mock<ICurrentUserService> _userMock;
    private readonly GetCharactersQueryHandler _handler;
    private readonly Guid _teamId = Guid.NewGuid();

    public GetCharactersQueryHandlerTests()
    {
        _repoMock = new Mock<ICharacterRepository>();
        _userMock = new Mock<ICurrentUserService>();
        _userMock.Setup(x => x.GetCurrentTeamId()).Returns(_teamId);
        _handler = new GetCharactersQueryHandler(_repoMock.Object, _userMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsOnlyTeamCharacters()
    {
        // Arrange
        var ownedCharacter = Character.Create(_teamId, "Team Cat", null, null, Guid.NewGuid());
        var otherCharacter = Character.Create(Guid.NewGuid(), "Other Cat", null, null, Guid.NewGuid());

        _repoMock
            .Setup(r => r.GetByTeamIdAsync(_teamId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { ownedCharacter }, 1));

        var query = new GetCharactersQuery(Page: 1, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.All(result.Value.Items, c => Assert.Equal(_teamId, c.TeamId));
    }

    [Fact]
    public async Task Handle_EmptyLibrary_ReturnsEmptyPagedResult()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetByTeamIdAsync(_teamId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<Character>(), 0));

        var query = new GetCharactersQuery(Page: 1, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public async Task Handle_ReadyCharactersIncludeLoraUrl()
    {
        // Arrange
        var character = Character.Create(_teamId, "LoRA Cat", null, "anime", Guid.NewGuid());
        character.AdvanceTraining(TrainingStatus.Ready, 100,
            "https://img.test/lora.png",
            "https://lora.test/lora.safetensors",
            "lora_cat_v1");

        _repoMock
            .Setup(r => r.GetByTeamIdAsync(_teamId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { character }, 1));

        var query = new GetCharactersQuery(Page: 1, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.Value.Items[0];
        Assert.Equal(TrainingStatus.Ready.ToString(), dto.TrainingStatus);
        Assert.Equal("lora_cat_v1", dto.TriggerWord);
    }
}
