using AnimStudio.IdentityModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimStudio.IdentityModule.Infrastructure.Persistence.Configurations;

internal sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams", "identity");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.LogoUrl).HasMaxLength(2048);
        builder.Property(t => t.OwnerId).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();

        builder.Property(t => t.RowVersion).IsRowVersion().IsConcurrencyToken();

        builder.HasMany(t => t.Members)
               .WithOne(m => m.Team)
               .HasForeignKey(m => m.TeamId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Subscription)
               .WithOne(s => s.Team)
               .HasForeignKey<Subscription>(s => s.TeamId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
