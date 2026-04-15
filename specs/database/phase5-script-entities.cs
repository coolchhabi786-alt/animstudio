// ============================================================
// Phase 5 — Script Workshop
// EF Core C# entity definitions with data annotations,
// navigation properties, and XML doc comments.
// ============================================================

using AnimStudio.SharedKernel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimStudio.ContentModule.Domain.Entities;

// ── Script ───────────────────────────────────────────────────────────────────

/// <summary>
/// A screenplay attached to a single episode. Contains the full screenplay
/// as a JSON blob (<see cref="RawJson"/>) produced by the Python ScriptwritingCrew,
/// or manually edited by the user. One script per episode (unique constraint on
/// <see cref="EpisodeId"/>).
/// </summary>
public sealed class Script : AggregateRoot<Guid>
{
    /// <summary>The episode this script belongs to. Unique — one script per episode.</summary>
    [Required]
    public Guid EpisodeId { get; private set; }

    /// <summary>Screenplay title. Max 500 chars.</summary>
    [Required, MaxLength(500)]
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// JSON-serialised Screenplay model produced by the Python ScriptwritingCrew.
    /// Schema: { title: string, scenes: [{ scene_number: int, visual_description: string,
    ///           emotional_tone: string, dialogue: [{ character: string, text: string,
    ///           start_time: float, end_time: float }] }] }
    /// Stored as nvarchar(max) to accommodate arbitrarily long screenplays.
    /// </summary>
    [Required, Column(TypeName = "nvarchar(max)")]
    public string RawJson { get; private set; } = "{}";

    /// <summary>
    /// True when the user has saved manual edits via PUT /episodes/{id}/script.
    /// Reset to false when an AI regeneration completes.
    /// </summary>
    public bool IsManuallyEdited { get; private set; }

    /// <summary>
    /// Optional director notes passed to the AI engine during regeneration.
    /// Max 5000 chars. Null if no notes provided.
    /// </summary>
    [MaxLength(5000)]
    public string? DirectorNotes { get; private set; }

    // ── Navigation ─────────────────────────────────────────────────────────
    /// <summary>Navigation property to the parent Episode.</summary>
    public Episode Episode { get; private set; } = null!;
}

// ── EF Core table configuration (applied in ContentDbContext.OnModelCreating) ──
//
//   Table name:     content.Scripts
//   Primary key:    Id (Guid)
//   Unique index:   IX_Scripts_EpisodeId (enforces one script per episode)
//   Columns:
//     Id                 uniqueidentifier   NOT NULL  PK
//     EpisodeId          uniqueidentifier   NOT NULL  FK → content.Episodes(Id)
//     Title              nvarchar(500)      NOT NULL
//     RawJson            nvarchar(max)      NOT NULL
//     IsManuallyEdited   bit                NOT NULL  DEFAULT 0
//     DirectorNotes      nvarchar(5000)     NULL
//     CreatedAt          datetimeoffset     NOT NULL
//     UpdatedAt          datetimeoffset     NOT NULL
//     IsDeleted          bit                NOT NULL  DEFAULT 0
//     DeletedAt          datetimeoffset     NULL
//     DeletedByUserId    uniqueidentifier   NULL
//     RowVersion         rowversion         NOT NULL
