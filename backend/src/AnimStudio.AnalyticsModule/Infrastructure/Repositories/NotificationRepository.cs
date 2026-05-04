using AnimStudio.AnalyticsModule.Application.Interfaces;
using AnimStudio.AnalyticsModule.Domain.Entities;
using AnimStudio.AnalyticsModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AnimStudio.AnalyticsModule.Infrastructure.Repositories;

public sealed class NotificationRepository(AnalyticsDbContext db) : INotificationRepository
{
    public async Task AddAsync(Notification notification, CancellationToken ct = default)
    {
        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);
    }

    public Task<List<Notification>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
        => db.Notifications
             .Where(n => n.UserId == userId && !n.IsDeleted)
             .OrderByDescending(n => n.CreatedAt)
             .Skip((page - 1) * pageSize)
             .Take(pageSize)
             .ToListAsync(ct);

    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Notifications.FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted, ct);

    public async Task UpdateAsync(Notification notification, CancellationToken ct = default)
    {
        db.Notifications.Update(notification);
        await db.SaveChangesAsync(ct);
    }

    public async Task MarkAllReadAsync(Guid userId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        await db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, now)
                .SetProperty(n => n.UpdatedAt, now),
            ct);
    }

    public Task<int> CountUnreadAsync(Guid userId, CancellationToken ct = default)
        => db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted, ct);
}
