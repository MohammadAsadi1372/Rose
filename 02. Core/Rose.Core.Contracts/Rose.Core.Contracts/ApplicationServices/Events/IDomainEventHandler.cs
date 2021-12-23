using Rose.Core.Domain.Events;

namespace Rose.Core.Contracts.ApplicationServices.Events;
public interface IDomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task Handle(TDomainEvent Event);
}

