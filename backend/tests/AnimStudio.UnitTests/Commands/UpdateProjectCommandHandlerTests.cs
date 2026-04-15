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
    public class UpdateProjectCommandHandlerTests
    {
        private readonly Mock<IProjectRepository> _repositoryMock;
        private readonly UpdateProjectCommandHandler _handler;
        private readonly Fixture _fixture;

        public UpdateProjectCommandHandlerTests()
        {
            _repositoryMock = new Mock<IProjectRepository>();
            _fixture = new Fixture();

            _handler = new UpdateProjectCommandHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task GivenValidCommand_WhenHandleIsCalled_ThenUpdatesProjectSuccessfully()
        {
            // Arrange
            var validCommand = _fixture.Create<UpdateProjectCommand>();
            var project = _fixture.Create<Project>();

            _repositoryMock.Setup(repo => repo.GetByIdAsync(validCommand.Id)).ReturnsAsync(project);

            // Act
            var result = await _handler.Handle(validCommand, default);

            // Assert
            result.Should().NotBeNull();
            _repositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Project>()), Times.Once);
        }

        [Fact]
        public async Task GivenInvalidCommand_WhenHandleIsCalled_ThenThrowsException()
        {
            // Arrange
            UpdateProjectCommand invalidCommand = null;

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await _handler.Handle(invalidCommand, default)
            );

            // Assert
            exception.Should().NotBeNull();
        }

        [Fact]
        public async Task GivenNonExistingProject_WhenHandleIsCalled_ThenThrowsNotFoundException()
        {
            // Arrange
            var validCommand = _fixture.Create<UpdateProjectCommand>();

            _repositoryMock.Setup(repo => repo.GetByIdAsync(validCommand.Id)).ReturnsAsync((Project)null);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () => 
                await _handler.Handle(validCommand, default)
            );

            // Assert
            exception.Should().NotBeNull();
        }
    }
}