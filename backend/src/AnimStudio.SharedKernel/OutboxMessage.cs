using System;

namespace AnimStudio.SharedKernel
{
    /// <summary>
    /// Represents a message stored in the outbox for processing.
    /// </summary>
    public class OutboxMessage
    {
        /// <summary>
        /// Gets or sets the unique identifier for the message.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the serialized payload of the event.
        /// </summary>
        public string Payload { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status of the message.
        /// </summary>
        public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.Pending;

        /// <summary>
        /// Gets or sets the timestamp of when the event occurred.
        /// </summary>
        public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets or sets the timestamp of when the message was processed.
        /// </summary>
        public DateTimeOffset? ProcessedAt { get; set; } = null;

        /// <summary>
        /// Gets or sets the retry count for processing the message.
        /// </summary>
        public int RetryCount { get; set; } = 0;
    }

    /// <summary>
    /// Enum representing the status of an outbox message.
    /// </summary>
    public enum OutboxMessageStatus
    {
        Pending,
        Delivered,
        Failed
    }
}