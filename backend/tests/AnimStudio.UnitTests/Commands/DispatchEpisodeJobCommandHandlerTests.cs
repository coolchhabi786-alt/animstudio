using Xunit;
using Moq;
using FluentAssertions;
using AutoFixture;
using AnimStudio.ContentModule.Application.Commands;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Entities;
using System.Threading.Tasks;

namespace AnimStudio.UnitTests.Commands
{
    public class DispatchEpisodeJobCommandHandlerTests
    {
        private readonly Mock<IJobRepository> _repositoryMock;
        private readonly DispatchEpisodeJobCommandHandler _handler;
        private readonly Fixture _fixture;

        public DispatchEpisodeJobCommandHandlerTests()
        {
            _repositoryMock = new Mock<IJobRepository>();
            _fixture = new Fixture();

            _handler = new DispatchEpisodeJobCommandHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task GivenValidCommand_WhenHandleIsCalled_ThenDispatchesJobSuccessfully()
        {
            // Arrange
            var validCommand = _fixture.Create<DispatchEpisodeJobCommand>();

            // Act
            var result = await _handler.Handle(validCommand, default);

            // Assert
            result.Should().NotBeNull();
            _repositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Job>()), Times.Once);
        }

        [Fact]
        public async Task GivenInvalidCommand_WhenHandleIsCalled_ThenThrowsException()
        {
            // Arrange
            DispatchEpisodeJobCommand invalidCommand = null;

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await _handler.Handle(invalidCommand, default)
            );

            // Assert
            exception.Should().NotBeNull();
        }
    }
}