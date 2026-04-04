using System;

namespace AnimStudio.SharedKernel
{
    /// <summary>
    /// Represents a base entity with common properties.
    /// </summary>
    /// <typeparam name="TId">The type of the entity ID.</typeparam>
    public abstract class Entity<TId>
    {
        /// <summary>
        /// Gets or sets the entity ID.
        /// </summary>
        public TId Id { get; set; } = default!;

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets or sets the last update timestamp.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Indicates whether the entity is marked as deleted.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Gets or sets the deletion timestamp if marked as deleted.
        /// </summary>
        public DateTimeOffset? DeletedAt { get; set; } = null;

        /// <summary>
        /// Gets or sets the user ID who deleted the entity, if applicable.
        /// </summary>
        public Guid? DeletedByUserId { get; set; } = null;

        /// <summary>
        /// Gets or sets the concurrency token used for optimistic concurrency checks.
        /// </summary>
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}