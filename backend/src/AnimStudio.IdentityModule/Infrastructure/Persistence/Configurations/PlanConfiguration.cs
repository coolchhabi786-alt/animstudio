using AnimStudio.IdentityModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimStudio.IdentityModule.Infrastructure.Persistence.Configurations;

internal sealed class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans", "identity");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => p.Name).IsUnique();

        builder.Property(p => p.StripePriceId).IsRequired().HasMaxLength(256);
        builder.HasIndex(p => p.StripePriceId).IsUnique();

        builder.Property(p => p.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.EpisodesPerMonth).IsRequired();
        builder.Property(p => p.MaxCharacters).IsRequired();
        builder.Property(p => p.MaxTeamMembers).IsRequired();
        builder.Property(p => p.IsActive).IsRequired();
        builder.Property(p => p.IsDefault).IsRequired();

        builder.HasQueryFilter(p => p.IsActive);
    }
}
