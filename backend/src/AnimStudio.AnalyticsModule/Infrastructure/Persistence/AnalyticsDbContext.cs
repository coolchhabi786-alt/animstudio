using AnimStudio.AnalyticsModule.Domain.Entities;
using AnimStudio.AnalyticsModule.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.AnalyticsModule.Infrastructure.Persistence;

public sealed class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : DbContext(options)
{
    public DbSet<VideoView>    VideoViews    => Set<VideoView>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("analytics");

        // ── VideoView ──────────────────────────────────────────────────────────
        modelBuilder.Entity<VideoView>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.Source)
             .HasConversion<string>()
             .HasMaxLength(30)
             .IsRequired();
            e.Property(v => v.ViewerIpHash).HasMaxLength(128);
            e.Property(v => v.ViewedAt).IsRequired();

            e.HasIndex(v => v.EpisodeId);
            e.HasIndex(v => v.RenderId);
            e.HasIndex(v => v.ReviewLinkId).HasFilter("[ReviewLinkId] IS NOT NULL");
        });

        // ── Notification ───────────────────────────────────────────────────────
        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Type)
             .HasConversion<string>()
             .HasMaxLength(30)
             .IsRequired();
            e.Property(n => n.Title).HasMaxLength(200).IsRequired();
            e.Property(n => n.Body).HasMaxLength(2000).IsRequired();
            e.Property(n => n.RelatedEntityType).HasMaxLength(100);
            e.Property(n => n.RowVersion).IsRowVersion();

            e.HasIndex(n => n.UserId);
            e.HasIndex(n => new { n.UserId, n.IsRead });
        });

        base.OnModelCreating(modelBuilder);
    }
}
