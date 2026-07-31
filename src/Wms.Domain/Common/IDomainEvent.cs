using MediatR;

namespace Wms.Domain.Common;

public interface IDomainEvent : INotification
{
    Guid Id { get; }
    DateTime OccurredOnUtc { get; }
}

public abstract record DomainEvent(Guid Id, DateTime OccurredOnUtc) : IDomainEvent;