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
    public class GetProjectsQueryHandlerTests
    {
        private readonly Mock<IProjectRepository> _repositoryMock;
        private readonly GetProjectsQueryHandler _handler;
        private readonly Fixture _fixture;

        public GetProjectsQueryHandlerTests()
        {
            _repositoryMock = new Mock<IProjectRepository>();
            _fixture = new Fixture();

            _handler = new GetProjectsQueryHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task GivenValidQuery_WhenHandleIsCalled_ThenReturnsProjectsList()
        {
            // Arrange
            var validQuery = _fixture.Create<GetProjectsQuery>();
            var projects = _fixture.CreateMany<Project>(5);

            _repositoryMock.Setup(repo => repo.GetPaginatedAsync(validQuery.PageNumber, validQuery.PageSize)).ReturnsAsync(projects);

            // Act
            var result = await _handler.Handle(validQuery, default);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
        }

        [Fact]
        public async Task GivenValidQuery_WhenNoProjectsExist_ThenReturnsEmptyList()
        {
            // Arrange
            var validQuery = _fixture.Create<GetProjectsQuery>();

            _repositoryMock.Setup(repo => repo.GetPaginatedAsync(validQuery.PageNumber, validQuery.PageSize)).ReturnsAsync(new List<Project>());

            // Act
            var result = await _handler.Handle(validQuery, default);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GivenInvalidQuery_WhenHandleIsCalled_ThenThrowsException()
        {
            // Arrange
            GetProjectsQuery invalidQuery = null;

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await _handler.Handle(invalidQuery, default)
            );

            // Assert
            exception.Should().NotBeNull();
        }
    }
}