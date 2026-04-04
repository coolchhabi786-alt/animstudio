using AnimStudio.IdentityModule.Domain.Entities;
using AnimStudio.IdentityModule.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AnimStudio.UnitTests.Infrastructure
{
    public class RepositoryTests
    {
        private static IdentityDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new IdentityDbContext(options);
        }

        [Fact]
        public async Task AddUser_And_Retrieve_Succeeds()
        {
            await using var ctx = CreateInMemoryContext();
            var user = User.Create(Guid.NewGuid(), "ext-1", "repo@example.com", "Repo User");
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
            var found = await ctx.Users.FindAsync(user.Id);
            found.Should().NotBeNull();
            found!.Email.Should().Be("repo@example.com");
        }

        [Fact]
        public async Task AddTeam_And_Retrieve_Succeeds()
        {
            await using var ctx = CreateInMemoryContext();
            var team = Team.Create(Guid.NewGuid(), "Test Team", Guid.NewGuid());
            ctx.Teams.Add(team);
            await ctx.SaveChangesAsync();
            var found = await ctx.Teams.FindAsync(team.Id);
            found.Should().NotBeNull();
            found!.Name.Should().Be("Test Team");
        }
    }
}
