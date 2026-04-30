using AnimStudio.DeliveryModule.Domain.Entities;
using AnimStudio.DeliveryModule.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.DeliveryModule.Infrastructure.Persistence;

/// <summary>
/// Delivery module's own EF Core context — owns the delivery.* schema tables.
/// </summary>
public sealed class DeliveryDbContext : DbContext
{
    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options) { }

    public DbSet<Render> Renders => Set<Render>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Render>(b =>
        {
            b.ToTable("Renders", "delivery");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.EpisodeId);
            b.HasIndex(x => x.Status);
            b.HasQueryFilter(x => !x.IsDeleted);

            b.Property(x => x.RowVersion).IsRowVersion();

            b.Property(x => x.AspectRatio)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)");

            b.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)");

            b.Property(x => x.FinalVideoUrl).HasMaxLength(2048);
            b.Property(x => x.CdnUrl).HasMaxLength(2048);
            b.Property(x => x.CaptionsSrtUrl).HasMaxLength(2048);
            b.Property(x => x.ErrorMessage).HasMaxLength(1000);
        });
    }
}
