using AnimStudio.SharedKernel.Enums;

namespace AnimStudio.SharedKernel
{
    /// <summary>
    /// Saga state for an episode's pipeline run — lives in shared.SagaStates.
    /// Tracks which pipeline stage is currently running, retry counts, and
    /// whether a compensating action is in progress.
    /// </summary>
    public class EpisodeSagaState
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>The episode this saga tracks.</summary>
        public Guid EpisodeId { get; set; }

        /// <summary>Current stage of the pipeline (stored as string for readability).</summary>
        public PipelineStage CurrentStage { get; set; } = PipelineStage.Idle;

        public int RetryCount { get; set; }

        public string? LastError { get; set; }

        public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>True when a compensating/rollback flow is active.</summary>
        public bool IsCompensating { get; set; }
    }
}