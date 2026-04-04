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
            b.ToTable("EpisodeSagaStates", "shared");
            b.HasKey(s => s.Id);
            b.Property(s => s.Id).ValueGeneratedNever();
            b.Property(s => s.Status).IsRequired().HasMaxLength(100);
            b.Property(s => s.Data).HasColumnType("nvarchar(max)");
            b.HasIndex(s => s.Status);
        });
    }
}
