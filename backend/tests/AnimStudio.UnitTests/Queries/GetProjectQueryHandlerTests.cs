using Xunit;
using Moq;
using FluentAssertions;
using AutoFixture;
using AnimStudio.ContentModule.Application.Queries;
using AnimStudio.ContentModule.Application.Interfaces;
using AnimStudio.ContentModule.Domain.Aggregates;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AnimStudio.UnitTests.Queries
{
    public class GetProjectQueryHandlerTests
    {
        private readonly Mock<IProjectRepository> _repositoryMock;
        private readonly GetProjectQueryHandler _handler;
        private readonly Fixture _fixture;

        public GetProjectQueryHandlerTests()
        {
            _repositoryMock = new Mock<IProjectRepository>();
            _fixture = new Fixture();

            _handler = new GetProjectQueryHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task GivenValidQuery_WhenHandleIsCalled_ThenReturnsProject()
        {
            // Arrange
            var validQuery = _fixture.Create<GetProjectQuery>();
            var project = _fixture.Create<Project>();

            _repositoryMock.Setup(repo => repo.GetByIdAsync(validQuery.Id)).ReturnsAsync(project);

            // Act
            var result = await _handler.Handle(validQuery, default);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(project);
        }

        [Fact]
        public async Task GivenNonExistingProject_WhenHandleIsCalled_ThenReturnsNull()
        {
            // Arrange
            var validQuery = _fixture.Create<GetProjectQuery>();

            _repositoryMock.Setup(repo => repo.GetByIdAsync(validQuery.Id)).ReturnsAsync((Project)null);

            // Act
            var result = await _handler.Handle(validQuery, default);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GivenInvalidQuery_WhenHandleIsCalled_ThenThrowsException()
        {
            // Arrange
            GetProjectQuery invalidQuery = null;

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await _handler.Handle(invalidQuery, default)
            );

            // Assert
            exception.Should().NotBeNull();
        }
    }
}