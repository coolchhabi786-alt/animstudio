using AnimStudio.ContentModule.Domain.Events;
using AnimStudio.SharedKernel;

namespace AnimStudio.ContentModule.Domain.Entities;

/// <summary>
/// Project aggregate — the top-level container for episodes.
/// </summary>
public sealed class Project : AggregateRoot<Guid>
{
    public Guid TeamId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? ThumbnailUrl { get; private set; }

    // Required by EF Core
    private Project() { }

    public static Project Create(Guid teamId, string name, string description)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            Name = name,
            Description = description,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        project.AddDomainEvent(new ProjectCreatedEvent(project.Id, teamId, name));
        return project;
    }

    public void Update(string name, string description, string? thumbnailUrl)
    {
        Name = name;
        Description = description;
        ThumbnailUrl = thumbnailUrl;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete(Guid deletedByUserId)
    {
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedByUserId = deletedByUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
