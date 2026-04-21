using AnimStudio.ContentModule.Domain.Entities;
using AnimStudio.ContentModule.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.ContentModule.Infrastructure.Persistence;

/// <summary>
/// Content module's own EF Core context — owns the content.* schema tables.
/// </summary>
public sealed class ContentDbContext : DbContext
{
    public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Episode> Episodes => Set<Episode>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<EpisodeTemplate> EpisodeTemplates => Set<EpisodeTemplate>();
    public DbSet<StylePreset> StylePresets => Set<StylePreset>();

    // ── Phase 4 — Character Studio ─────────────────────────────────────────
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<EpisodeCharacter> EpisodeCharacters => Set<EpisodeCharacter>();

    // ── Phase 5 — Script Workshop ──────────────────────────────────────────
    public DbSet<Script> Scripts => Set<Script>();

    // ── Phase 6 — Storyboard Studio ────────────────────────────────────────
    public DbSet<Storyboard> Storyboards => Set<Storyboard>();
    public DbSet<StoryboardShot> StoryboardShots => Set<StoryboardShot>();

    // ── Phase 7 — Voice Studio ─────────────────────────────────────────────
    public DbSet<VoiceAssignment> VoiceAssignments => Set<VoiceAssignment>();

    // ── Phase 8 — Animation Studio ─────────────────────────────────────────
    public DbSet<AnimationJob> AnimationJobs => Set<AnimationJob>();
    public DbSet<AnimationClip> AnimationClips => Set<AnimationClip>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Project ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Project>(b =>
        {
            b.ToTable("Projects", "content");
            b.HasKey(p => p.Id);
            b.Property(p => p.Id).ValueGeneratedNever();
            b.Property(p => p.Name).IsRequired().HasMaxLength(200);
            b.Property(p => p.Description).HasMaxLength(2000);
            b.Property(p => p.ThumbnailUrl).HasMaxLength(2000);
            b.Property(p => p.RowVersion).IsRowVersion();
            // Global query filter: soft-delete
            b.HasQueryFilter(p => !p.IsDeleted);
            b.HasIndex(p => p.TeamId);
        });

        // ── Episode ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Episode>(b =>
        {
            b.ToTable("Episodes", "content");
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).ValueGeneratedNever();
            b.Property(e => e.Name).IsRequired().HasMaxLength(200);
            b.Property(e => e.Style).HasMaxLength(500);
            b.Property(e => e.DirectorNotes).HasMaxLength(5000);
            b.Property(e => e.CharacterIds).HasColumnType("nvarchar(max)");
            // Store EpisodeStatus as string for readability in the DB
            b.Property(e => e.Status).HasConversion<string>().IsRequired().HasMaxLength(30);
            b.Property(e => e.RowVersion).IsRowVersion(); // Optimistic concurrency
            b.HasQueryFilter(e => !e.IsDeleted);
            b.HasIndex(e => e.ProjectId);
        });

        // ── Job ────────────────────────────────────────────────────────────────
        modelBuilder.Entity<Job>(b =>
        {
            b.ToTable("Jobs", "content");
            b.HasKey(j => j.Id);
            b.Property(j => j.Id).ValueGeneratedNever();
            b.Property(j => j.Type).HasConversion<string>().IsRequired().HasMaxLength(30);
            b.Property(j => j.Status).HasConversion<string>().IsRequired().HasMaxLength(20);
            b.Property(j => j.Payload).HasColumnType("nvarchar(max)");
            b.Property(j => j.Result).HasColumnType("nvarchar(max)");
            b.Property(j => j.ErrorMessage).HasMaxLength(4000);
            b.HasIndex(j => j.EpisodeId);
            b.HasIndex(j => j.Status);
        });

        // ── EpisodeTemplate ────────────────────────────────────────────────────
        modelBuilder.Entity<EpisodeTemplate>(b =>
        {
            b.ToTable("EpisodeTemplates", "content");
            b.HasKey(t => t.Id);
            b.Property(t => t.Id).ValueGeneratedNever();
            b.Property(t => t.Title).IsRequired().HasMaxLength(200);
            b.Property(t => t.Genre).HasConversion<string>().IsRequired().HasMaxLength(30);
            b.Property(t => t.Description).IsRequired().HasMaxLength(1000);
            b.Property(t => t.PlotStructure).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(t => t.DefaultStyle).HasConversion<string>().IsRequired().HasMaxLength(30);
            b.Property(t => t.PreviewVideoUrl).HasMaxLength(2000);
            b.Property(t => t.ThumbnailUrl).HasMaxLength(2000);
            b.HasIndex(t => t.Genre);
            b.HasIndex(t => t.SortOrder);
            // No soft-delete — use IsActive = false to retire
        });

        // ── StylePreset ────────────────────────────────────────────────────────
        modelBuilder.Entity<StylePreset>(b =>
        {
            b.ToTable("StylePresets", "content");
            b.HasKey(s => s.Id);
            b.Property(s => s.Id).ValueGeneratedNever();
            b.Property(s => s.Style).HasConversion<string>().IsRequired().HasMaxLength(30);
            b.Property(s => s.DisplayName).IsRequired().HasMaxLength(100);
            b.Property(s => s.Description).IsRequired().HasMaxLength(500);
            b.Property(s => s.SampleImageUrl).HasMaxLength(2000);
            b.Property(s => s.FluxStylePromptSuffix).IsRequired().HasMaxLength(500);
            b.HasIndex(s => s.Style).IsUnique();
        });

        // ── Character (Phase 4) ────────────────────────────────────────────────
        modelBuilder.Entity<Character>(b =>
        {
            b.ToTable("Characters", "content");
            b.HasKey(c => c.Id);
            b.Property(c => c.Id).ValueGeneratedNever();
            b.Property(c => c.TeamId).IsRequired();
            b.Property(c => c.Name).IsRequired().HasMaxLength(200);
            b.Property(c => c.Description).HasMaxLength(2000);
            b.Property(c => c.StyleDna).HasMaxLength(4000);
            b.Property(c => c.ImageUrl).HasMaxLength(2048);
            b.Property(c => c.LoraWeightsUrl).HasMaxLength(2048);
            b.Property(c => c.TriggerWord).HasMaxLength(100);
            b.Property(c => c.TrainingStatus)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(30);
            b.Property(c => c.TrainingProgressPercent).HasDefaultValue(0);
            b.Property(c => c.CreditsCost).HasDefaultValue(50);
            b.Property(c => c.RowVersion).IsRowVersion();
            b.HasQueryFilter(c => !c.IsDeleted);
            b.HasIndex(c => c.TeamId);
        });

        // ── EpisodeCharacter (Phase 4) ─────────────────────────────────────────
        modelBuilder.Entity<EpisodeCharacter>(b =>
        {
            b.ToTable("EpisodeCharacters", "content");
            b.HasKey(ec => new { ec.EpisodeId, ec.CharacterId });
            b.HasOne(ec => ec.Episode)
                .WithMany()
                .HasForeignKey(ec => ec.EpisodeId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(ec => ec.Character)
                .WithMany(c => c.EpisodeCharacters)
                .HasForeignKey(ec => ec.CharacterId)
                .OnDelete(DeleteBehavior.Restrict); // prevent cascading delete on character
        });

        // ── Script (Phase 5) ───────────────────────────────────────────────────
        modelBuilder.Entity<Script>(b =>
        {
            b.ToTable("Scripts", "content");
            b.HasKey(s => s.Id);
            b.Property(s => s.Id).ValueGeneratedNever();
            b.Property(s => s.EpisodeId).IsRequired();
            b.Property(s => s.Title).IsRequired().HasMaxLength(500);
            b.Property(s => s.RawJson).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(s => s.IsManuallyEdited).HasDefaultValue(false);
            b.Property(s => s.DirectorNotes).HasMaxLength(5000);
            b.Property(s => s.RowVersion).IsRowVersion();
            b.HasIndex(s => s.EpisodeId).IsUnique(); // one script per episode
        });

        // ── Storyboard (Phase 6) ───────────────────────────────────────────────
        modelBuilder.Entity<Storyboard>(b =>
        {
            b.ToTable("Storyboards", "content");
            b.HasKey(s => s.Id);
            b.Property(s => s.Id).ValueGeneratedNever();
            b.Property(s => s.EpisodeId).IsRequired();
            b.Property(s => s.ScreenplayTitle).IsRequired().HasMaxLength(500);
            b.Property(s => s.RawJson).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(s => s.DirectorNotes).HasMaxLength(5000);
            b.Property(s => s.RowVersion).IsRowVersion();
            b.HasQueryFilter(s => !s.IsDeleted);
            b.HasIndex(s => s.EpisodeId).IsUnique(); // one storyboard per episode

            // Aggregate ownership — shots load via backing field.
            // Map the public navigation property and instruct EF to use the
            // private backing field for property access. Using the expression
            // avoids EF inferring the field twice which caused the conflict.
            b.HasMany(s => s.Shots)
                .WithOne()
                .HasForeignKey(nameof(StoryboardShot.StoryboardId))
                .OnDelete(DeleteBehavior.Cascade);

            b.Metadata
                .FindNavigation(nameof(Storyboard.Shots))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        // ── StoryboardShot (Phase 6) ───────────────────────────────────────────
        modelBuilder.Entity<StoryboardShot>(b =>
        {
            b.ToTable("StoryboardShots", "content");
            b.HasKey(s => s.Id);
            b.Property(s => s.Id).ValueGeneratedNever();
            b.Property(s => s.StoryboardId).IsRequired();
            b.Property(s => s.SceneNumber).IsRequired();
            b.Property(s => s.ShotIndex).IsRequired();
            b.Property(s => s.Description).IsRequired().HasMaxLength(2000);
            b.Property(s => s.ImageUrl).HasMaxLength(2048);
            b.Property(s => s.StyleOverride).HasMaxLength(500);
            b.Property(s => s.RegenerationCount).HasDefaultValue(0);
            b.Property(s => s.RowVersion).IsRowVersion();
            b.HasQueryFilter(s => !s.IsDeleted);
            b.HasIndex(s => s.StoryboardId);
            b.HasIndex(s => new { s.StoryboardId, s.SceneNumber, s.ShotIndex }).IsUnique();
        });

        // ── VoiceAssignment (Phase 7) ──────────────────────────────────────────
        modelBuilder.Entity<VoiceAssignment>(b =>
        {
            b.ToTable("VoiceAssignments", "content");
            b.HasKey(v => v.Id);
            b.Property(v => v.Id).ValueGeneratedNever();
            b.Property(v => v.EpisodeId).IsRequired();
            b.Property(v => v.CharacterId).IsRequired();
            b.Property(v => v.VoiceName).IsRequired().HasMaxLength(100);
            b.Property(v => v.Language).IsRequired().HasMaxLength(10).HasDefaultValue("en-US");
            b.Property(v => v.VoiceCloneUrl).HasMaxLength(2048);
            b.Property(v => v.RowVersion).IsRowVersion();
            b.HasQueryFilter(v => !v.IsDeleted);
            b.HasIndex(v => v.EpisodeId);
            // Unique index is filtered so soft-deleted rows don't block re-assignment
            b.HasIndex(v => new { v.EpisodeId, v.CharacterId }).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        // ── AnimationJob (Phase 8) ─────────────────────────────────────────────
        modelBuilder.Entity<AnimationJob>(b =>
        {
            b.ToTable("AnimationJobs", "content");
            b.HasKey(j => j.Id);
            b.Property(j => j.Id).ValueGeneratedNever();
            b.Property(j => j.EpisodeId).IsRequired();
            b.Property(j => j.Backend).HasConversion<string>().IsRequired().HasMaxLength(20);
            b.Property(j => j.Status).HasConversion<string>().IsRequired().HasMaxLength(30);
            b.Property(j => j.EstimatedCostUsd).HasPrecision(10, 4);
            b.Property(j => j.ActualCostUsd).HasPrecision(10, 4);
            b.Property(j => j.RowVersion).IsRowVersion();
            b.HasQueryFilter(j => !j.IsDeleted);
            b.HasIndex(j => j.EpisodeId);
            b.HasIndex(j => j.Status);
        });

        // ── AnimationClip (Phase 8) ────────────────────────────────────────────
        modelBuilder.Entity<AnimationClip>(b =>
        {
            b.ToTable("AnimationClips", "content");
            b.HasKey(c => c.Id);
            b.Property(c => c.Id).ValueGeneratedNever();
            b.Property(c => c.EpisodeId).IsRequired();
            b.Property(c => c.SceneNumber).IsRequired();
            b.Property(c => c.ShotIndex).IsRequired();
            b.Property(c => c.ClipUrl).HasMaxLength(2048);
            b.Property(c => c.Status).HasConversion<string>().IsRequired().HasMaxLength(20);
            b.Property(c => c.RowVersion).IsRowVersion();
            b.HasQueryFilter(c => !c.IsDeleted);
            b.HasIndex(c => c.EpisodeId);
            b.HasIndex(c => new { c.EpisodeId, c.SceneNumber, c.ShotIndex }).IsUnique();
            b.HasOne<StoryboardShot>()
                .WithMany()
                .HasForeignKey(c => c.StoryboardShotId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
