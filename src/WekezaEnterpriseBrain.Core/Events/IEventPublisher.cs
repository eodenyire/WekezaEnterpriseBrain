namespace WekezaEnterpriseBrain.Core.Events;

/// <summary>
/// Interface for publishing domain events
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : DomainEvent;
    Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> events) where TEvent : DomainEvent;
}

/// <summary>
/// Interface for consuming domain events
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : DomainEvent
{
    Task HandleAsync(TEvent domainEvent);
}

/// <summary>
/// Interface for event bus
/// </summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : DomainEvent;
    void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : DomainEvent;
    void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : DomainEvent;
}
