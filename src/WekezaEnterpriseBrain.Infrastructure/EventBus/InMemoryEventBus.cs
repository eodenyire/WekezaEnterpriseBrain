using WekezaEnterpriseBrain.Core.Events;

namespace WekezaEnterpriseBrain.Infrastructure.EventBus;

/// <summary>
/// In-memory event bus for POC (in production, use Kafka or RabbitMQ)
/// </summary>
public class InMemoryEventBus : IEventBus, IEventPublisher
{
    private readonly Dictionary<Type, List<object>> _handlers = new();
    private readonly object _lock = new();

    public Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : DomainEvent
    {
        List<object> handlers;
        
        lock (_lock)
        {
            if (!_handlers.TryGetValue(typeof(TEvent), out handlers!))
            {
                // No handlers registered for this event type
                return Task.CompletedTask;
            }
            // Create a copy to avoid collection modification issues
            handlers = new List<object>(handlers);
        }

        // Execute handlers asynchronously
        var tasks = handlers
            .Cast<IEventHandler<TEvent>>()
            .Select(handler => handler.HandleAsync(domainEvent));

        return Task.WhenAll(tasks);
    }

    public Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> events) where TEvent : DomainEvent
    {
        var tasks = events.Select(e => PublishAsync(e));
        return Task.WhenAll(tasks);
    }

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : DomainEvent
    {
        lock (_lock)
        {
            if (!_handlers.ContainsKey(typeof(TEvent)))
            {
                _handlers[typeof(TEvent)] = new List<object>();
            }
            _handlers[typeof(TEvent)].Add(handler);
        }
    }

    public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : DomainEvent
    {
        lock (_lock)
        {
            if (_handlers.TryGetValue(typeof(TEvent), out var handlers))
            {
                handlers.Remove(handler);
            }
        }
    }

    public int GetHandlerCount<TEvent>() where TEvent : DomainEvent
    {
        lock (_lock)
        {
            return _handlers.TryGetValue(typeof(TEvent), out var handlers) ? handlers.Count : 0;
        }
    }
}
