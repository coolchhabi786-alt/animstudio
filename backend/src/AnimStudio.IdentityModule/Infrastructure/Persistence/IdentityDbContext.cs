using AnimStudio.IdentityModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.IdentityModule.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for the Identity module. Uses the <c>identity</c> schema.
/// All tables in this module are prefixed with the schema to avoid collisions with other modules.
/// </summary>
public sealed class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Plan> Plans => Set<Plan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration<T> classes from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

        // Seed default plans
        SeedPlans(modelBuilder);
    }

    private static void SeedPlans(ModelBuilder modelBuilder)
    {
        var starterPlanId = new Guid("11111111-1111-1111-1111-111111111111");
        var proPlanId = new Guid("22222222-2222-2222-2222-222222222222");
        var studioPlanId = new Guid("33333333-3333-3333-3333-333333333333");

        modelBuilder.Entity<Plan>().HasData(
            new Plan
            {
                Id = starterPlanId,
                Name = "Starter",
                StripePriceId = "price_starter",
                EpisodesPerMonth = 3,
                MaxCharacters = 5,
                MaxTeamMembers = 2,
                Price = 0m,
                IsActive = true,
                IsDefault = true,
                CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                UpdatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                RowVersion = Array.Empty<byte>(),
            },
            new Plan
            {
                Id = proPlanId,
                Name = "Pro",
                StripePriceId = "price_pro_monthly",
                EpisodesPerMonth = 20,
                MaxCharacters = 25,
                MaxTeamMembers = 5,
                Price = 49m,
                IsActive = true,
                IsDefault = false,
                CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                UpdatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                RowVersion = Array.Empty<byte>(),
            },
            new Plan
            {
                Id = studioPlanId,
                Name = "Studio",
                StripePriceId = "price_studio_monthly",
                EpisodesPerMonth = 100,
                MaxCharacters = 100,
                MaxTeamMembers = 20,
                Price = 199m,
                IsActive = true,
                IsDefault = false,
                CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                UpdatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                RowVersion = Array.Empty<byte>(),
            });
    }
}
