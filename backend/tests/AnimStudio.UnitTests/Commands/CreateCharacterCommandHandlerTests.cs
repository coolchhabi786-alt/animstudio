using AnimStudio.ContentModule.Application.Commands.CreateCharacter;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;
using Moq;
using Xunit;

namespace AnimStudio.UnitTests.Commands;

public sealed class CreateCharacterCommandHandlerTests
{
    private readonly Mock<ICharacterRepository> _repoMock;
    private readonly Mock<ICurrentUserService> _userMock;
    private readonly CreateCharacterCommandHandler _handler;

    public CreateCharacterCommandHandlerTests()
    {
        _repoMock = new Mock<ICharacterRepository>();
        _userMock = new Mock<ICurrentUserService>();
        _userMock.Setup(x => x.GetCurrentTeamId()).Returns(Guid.NewGuid());
        _userMock.Setup(x => x.GetCurrentUserId()).Returns(Guid.NewGuid());
        _handler = new CreateCharacterCommandHandler(_repoMock.Object, _userMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesCharacterAndQueuesTraining()
    {
        // Arrange
        var cmd = new CreateCharacterCommand(
            Name: "Whiskerbolt",
            Description: "An electrifying orange tabby",
            StyleDna: "anime, vibrant, big eyes");

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.JobId);
        Assert.Equal(50, result.Value.EstimatedCreditsCost);

        _repoMock.Verify(
            r => r.AddAsync(It.Is<Character>(c =>
                c.Name == "Whiskerbolt" &&
                c.TrainingStatus == TrainingStatus.TrainingQueued),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyName_ThrowsValidationException()
    {
        // Validation is enforced by FluentValidation pipeline behaviour.
        // Unit test the domain factory directly when name is empty.
        Assert.Throws<ArgumentException>(() =>
            Character.Create(Guid.NewGuid(), string.Empty, null, null, Guid.NewGuid()));
    }

    [Fact]
    public async Task Handle_NameTooLong_ThrowsValidationException()
    {
        var longName = new string('A', 201);
        var cmd = new CreateCharacterCommand(longName, null, null);

        // FluentValidation pipeline would catch this. Verify the validator directly.
        var validator = new CreateCharacterCommandValidator();
        var validation = await validator.ValidateAsync(cmd);

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, e => e.PropertyName == nameof(cmd.Name));
    }
}
