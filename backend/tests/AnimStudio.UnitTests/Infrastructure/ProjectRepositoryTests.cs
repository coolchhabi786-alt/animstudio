using Xunit;
using FluentAssertions;
using AnimStudio.ContentModule.Infrastructure.Repositories;
using AnimStudio.ContentModule.Domain.Aggregates;
using AnimStudio.ContentModule.Infrastructure;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AnimStudio.UnitTests.Infrastructure
{
    public class ProjectRepositoryTests
    {
        private readonly DbContextOptions<ContentDbContext> _dbContextOptions;
        private readonly ContentDbContext _dbContext;
        private readonly ProjectRepository _repository;

        public ProjectRepositoryTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<ContentDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _dbContext = new ContentDbContext(_dbContextOptions);
            _repository = new ProjectRepository(_dbContext);
        }

        [Fact]
        public async Task GivenProject_WhenAddAsyncIsCalled_ThenSavesToDatabase()
        {
            // Arrange
            var project = new Project
            {
                TeamId = 1,
                Name = "Test Project",
                Description = "Test Description"
            };

            // Act
            await _repository.AddAsync(project);
            await _dbContext.SaveChangesAsync();

            // Assert
            var savedProject = _dbContext.Projects.FirstOrDefault(p => p.Name == "Test Project");
            savedProject.Should().NotBeNull();
        }

        [Fact]
        public async Task GivenProjectsInDatabase_WhenGetPaginatedAsyncIsCalled_ThenReturnsCorrectPage()
        {
            // Arrange
            _dbContext.Projects.AddRange(new[]
            {
                new Project { TeamId = 1, Name = "Project 1" },
                new Project { TeamId = 1, Name = "Project 2" },
                new Project { TeamId = 1, Name = "Project 3" },
            });

            await _dbContext.SaveChangesAsync();

            // Act
            var paginatedProjects = await _repository.GetPaginatedAsync(1, 2);

            // Assert
            paginatedProjects.Should().NotBeNull();
            paginatedProjects.Should().HaveCount(2);
            paginatedProjects.First().Name.Should().Be("Project 1");
        }

        [Fact]
        public async Task GivenNonExistingProject_WhenGetByIdAsyncIsCalled_ThenReturnsNull()
        {
            // Act
            var project = await _repository.GetByIdAsync(999);

            // Assert
            project.Should().BeNull();
        }
    }
}