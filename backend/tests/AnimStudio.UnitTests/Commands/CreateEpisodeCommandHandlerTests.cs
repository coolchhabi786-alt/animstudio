using Xunit;
using Moq;
using FluentAssertions;
using AutoFixture;
using AnimStudio.ContentModule.Application.Commands;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Aggregates;
using System.Threading.Tasks;

namespace AnimStudio.UnitTests.Commands
{
    public class CreateEpisodeCommandHandlerTests
    {
        private readonly Mock<IEpisodeRepository> _repositoryMock;
        private readonly CreateEpisodeCommandHandler _handler;
        private readonly Fixture _fixture;

        public CreateEpisodeCommandHandlerTests()
        {
            _repositoryMock = new Mock<IEpisodeRepository>();
            _fixture = new Fixture();

            _handler = new CreateEpisodeCommandHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task GivenValidCommand_WhenHandleIsCalled_ThenCreatesEpisodeSuccessfully()
        {
            // Arrange
            var validCommand = _fixture.Create<CreateEpisodeCommand>();

            // Act
            var result = await _handler.Handle(validCommand, default);

            // Assert
            result.Should().NotBeNull();
            _repositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Episode>()), Times.Once);
        }

        [Fact]
        public async Task GivenInvalidCommand_WhenHandleIsCalled_ThenThrowsException()
        {
            // Arrange
            CreateEpisodeCommand invalidCommand = null;

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await _handler.Handle(invalidCommand, default)
            );

            // Assert
            exception.Should().NotBeNull();
        }
    }
}