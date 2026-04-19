// Phase 7 — Voice Studio Entity Definitions (EF Core)
// Schema: content.*

using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// Built-in TTS voice names (OpenAI TTS voices).
/// </summary>
public enum BuiltInVoice
{
    Alloy   = 0,
    Echo    = 1,
    Fable   = 2,
    Onyx    = 3,
    Nova    = 4,
    Shimmer = 5,
}

/// <summary>
/// Maps a character to a voice within an episode.
/// One voice assignment per (EpisodeId, CharacterId) pair.
/// VoiceCloneUrl is nullable — populated only for Studio-tier custom voice clones.
/// </summary>
public sealed class VoiceAssignment : AggregateRoot<Guid>
{
    // Properties:
    //   Id              Guid        PK, ValueGeneratedNever
    //   EpisodeId       Guid        FK → Episodes, required
    //   CharacterId     Guid        FK → Characters, required
    //   VoiceName       string      Required, MaxLength(100) — built-in name or custom clone name
    //   Language        string      Required, MaxLength(10), default "en-US"
    //   VoiceCloneUrl   string?     Nullable, MaxLength(2048) — signed Blob URL (Studio tier only)
    //   RowVersion      byte[]      Optimistic concurrency token
    //
    // Indexes:
    //   IX_VoiceAssignments_EpisodeId_CharacterId  UNIQUE
    //   IX_VoiceAssignments_EpisodeId
    //
    // Table: content.VoiceAssignments
}
