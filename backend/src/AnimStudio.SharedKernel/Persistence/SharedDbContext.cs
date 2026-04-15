using AnimStudio.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.SharedKernel.Persistence;

/// <summary>
/// Shared DbContext that owns cross-module persistence concerns:
/// the transactional outbox and Saga state tables.
/// Uses the <c>shared</c> schema.
/// </summary>
public sealed class SharedDbContext : DbContext
{
    public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options) { }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<EpisodeSagaState> EpisodeSagaStates => Set<EpisodeSagaState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("OutboxMessages", "shared");
            b.HasKey(m => m.Id);
            b.Property(m => m.Id).ValueGeneratedNever();
            b.Property(m => m.EventType).IsRequired().HasMaxLength(256);
            b.Property(m => m.Payload).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(m => m.Status).HasConversion<string>().IsRequired().HasMaxLength(20);
            b.HasIndex(m => new { m.Status, m.OccurredAt });
        });

        modelBuilder.Entity<EpisodeSagaState>(b =>
        {
            b.ToTable("SagaStates", "shared");
            b.HasKey(s => s.Id);
            b.Property(s => s.Id).ValueGeneratedNever();
            b.Property(s => s.CurrentStage).HasConversion<string>().IsRequired().HasMaxLength(50);
            b.Property(s => s.LastError).HasMaxLength(2000);
            b.HasIndex(s => s.EpisodeId).IsUnique();
        });
    }
}
