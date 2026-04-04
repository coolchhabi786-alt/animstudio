using AnimStudio.IdentityModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnimStudio.IdentityModule.Infrastructure.Persistence.Configurations;

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions", "identity");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.TeamId).IsRequired();
        builder.Property(s => s.PlanId).IsRequired();
        builder.Property(s => s.StripeSubscriptionId).HasMaxLength(256);
        builder.HasIndex(s => s.StripeSubscriptionId).IsUnique().HasFilter("[StripeSubscriptionId] IS NOT NULL");
        builder.Property(s => s.StripeCustomerId).HasMaxLength(256);

        builder.Property(s => s.Status)
               .HasConversion<string>()
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(s => s.CurrentPeriodStart).IsRequired();
        builder.Property(s => s.CurrentPeriodEnd).IsRequired();
        builder.Property(s => s.TrialEndsAt);
        builder.Property(s => s.CancelAtPeriodEnd).IsRequired();
        builder.Property(s => s.UsageEpisodesThisMonth).IsRequired();
        builder.Property(s => s.UsageResetAt).IsRequired();

        builder.Property(s => s.RowVersion).IsRowVersion().IsConcurrencyToken();

        builder.HasOne(s => s.Plan)
               .WithMany()
               .HasForeignKey(s => s.PlanId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
