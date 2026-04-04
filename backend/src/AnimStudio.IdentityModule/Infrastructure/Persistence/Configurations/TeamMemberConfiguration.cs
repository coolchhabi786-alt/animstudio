using AnimStudio.IdentityModule.Domain.Entities;
using AnimStudio.IdentityModule.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimStudio.IdentityModule.Infrastructure.Persistence.Configurations;

internal sealed class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("TeamMembers", "identity");

        builder.HasKey(m => new { m.TeamId, m.UserId });

        builder.Property(m => m.Role)
               .HasConversion<string>()
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(m => m.JoinedAt).IsRequired();
        builder.Property(m => m.InviteToken).HasMaxLength(256);
        builder.HasIndex(m => m.InviteToken).IsUnique().HasFilter("[InviteToken] IS NOT NULL");

        builder.Property(m => m.InviteExpiresAt);
        builder.Property(m => m.InviteAcceptedAt);

        builder.HasOne(m => m.User)
               .WithMany()
               .HasForeignKey(m => m.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
