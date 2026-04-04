using AnimStudio.IdentityModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimStudio.IdentityModule.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "identity");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.ExternalId).IsRequired().HasMaxLength(256);
        builder.HasIndex(u => u.ExternalId).IsUnique();

        builder.Property(u => u.Email).IsRequired().HasMaxLength(320);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.DisplayName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.AvatarUrl).HasMaxLength(2048);
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.LastLoginAt);

        builder.Property(u => u.RowVersion).IsRowVersion().IsConcurrencyToken();

        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
