using MediatR;

namespace AnimStudio.SharedKernel
{
    /// <summary>
    /// Marker interface for domain events, implementing MediatR.INotification.
    /// </summary>
    public interface IDomainEvent : INotification
    {
    }
}