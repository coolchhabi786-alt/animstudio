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
    public class CreateProjectCommandHandlerTests
    {
        private readonly Mock<IProjectRepository> _repositoryMock;
        private readonly CreateProjectCommandHandler _handler;
        private readonly Fixture _fixture;

        public CreateProjectCommandHandlerTests()
        {
            _repositoryMock = new Mock<IProjectRepository>();
            _fixture = new Fixture();

            _handler = new CreateProjectCommandHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task GivenValidCommand_WhenHandleIsCalled_ThenCreatesProjectSuccessfully()
        {
            // Arrange
            var validCommand = _fixture.Create<CreateProjectCommand>();

            // Act
            var result = await _handler.Handle(validCommand, default);

            // Assert
            result.Should().NotBeNull();
            _repositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Project>()), Times.Once);
        }

        [Fact]
        public async Task GivenInvalidCommand_WhenHandleIsCalled_ThenThrowsException()
        {
            // Arrange
            CreateProjectCommand invalidCommand = null;

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await _handler.Handle(invalidCommand, default)
            );

            // Assert
            exception.Should().NotBeNull();
        }
    }
}