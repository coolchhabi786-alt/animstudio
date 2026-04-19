// Phase 8 — Animation Studio entity schema outline (non-compiling reference)
// Canonical location of the compiled types:
//   AnimStudio.ContentModule.Domain.Entities.AnimationJob
//   AnimStudio.ContentModule.Domain.Entities.AnimationClip

namespace AnimStudio.ContentModule.Domain.Entities;

// Tables live in the "content" schema.

public sealed class AnimationJob // : AggregateRoot<Guid>, ISoftDelete
{
    public Guid Id { get; }                       // PK
    public Guid EpisodeId { get; }                // FK -> content.Episodes (required, indexed)
    public AnimationBackend Backend { get; }      // Kling | Local
    public decimal EstimatedCostUsd { get; }      // decimal(10,4)
    public decimal? ActualCostUsd { get; }        // decimal(10,4) nullable
    public Guid? ApprovedByUserId { get; }        // audit
    public DateTime? ApprovedAt { get; }
    public AnimationStatus Status { get; }        // PendingApproval | Approved | Running | Completed | Failed | Cancelled
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }
    public byte[] RowVersion { get; }             // concurrency
    public bool IsDeleted { get; }                // soft-delete
}

public sealed class AnimationClip // : AggregateRoot<Guid>, ISoftDelete
{
    public Guid Id { get; }                       // PK
    public Guid EpisodeId { get; }                // FK -> content.Episodes
    public int SceneNumber { get; }               // 1-based
    public int ShotIndex { get; }                 // 0-based
    public Guid? StoryboardShotId { get; }        // FK -> content.StoryboardShots (optional)
    public string? ClipUrl { get; }               // Blob path, signed at request time
    public double? DurationSeconds { get; }
    public ClipStatus Status { get; }             // Pending | Rendering | Ready | Failed
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }
    public byte[] RowVersion { get; }
    public bool IsDeleted { get; }
}

public enum AnimationBackend { Kling = 0, Local = 1 }

public enum AnimationStatus
{
    PendingApproval = 0,
    Approved        = 1,
    Running         = 2,
    Completed       = 3,
    Failed          = 4,
    Cancelled       = 5,
}

public enum ClipStatus
{
    Pending   = 0,
    Rendering = 1,
    Ready     = 2,
    Failed    = 3,
}

// EF Fluent-API highlights (ContentDbContext.OnModelCreating):
//   - ToTable("AnimationJobs", "content")   / ToTable("AnimationClips", "content")
//   - HasQueryFilter(x => !x.IsDeleted)
//   - HasIndex(x => x.EpisodeId)
//   - AnimationClip: HasIndex(x => new { EpisodeId, SceneNumber, ShotIndex }).IsUnique()
//   - AnimationClip: HasOne<StoryboardShot>().WithMany().HasForeignKey(x => x.StoryboardShotId).OnDelete(SetNull)
//   - Property(x => x.EstimatedCostUsd).HasPrecision(10, 4)
//   - Property(x => x.RowVersion).IsRowVersion()
