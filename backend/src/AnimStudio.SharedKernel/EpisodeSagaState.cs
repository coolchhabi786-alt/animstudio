using System;

namespace AnimStudio.SharedKernel
{
    /// <summary>
    /// Represents the state of an episode saga.
    /// </summary>
    public class EpisodeSagaState
    {
        /// <summary>
        /// Gets or sets the unique identifier for the saga state.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the current status of the saga.
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Gets or sets the serialized data representing the saga's progress.
        /// </summary>
        public string Data { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp of the last update.
        /// </summary>
        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    }
}