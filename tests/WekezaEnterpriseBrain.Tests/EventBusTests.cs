using Xunit;
using WekezaEnterpriseBrain.Core.Events;
using WekezaEnterpriseBrain.Infrastructure.EventBus;

namespace WekezaEnterpriseBrain.Tests;

public class EventBusTests
{
    private class TestEvent : DomainEvent
    {
        public string Message { get; set; } = string.Empty;
    }

    private class TestEventHandler : IEventHandler<TestEvent>
    {
        public int HandledCount { get; private set; }
        public TestEvent? LastEvent { get; private set; }

        public Task HandleAsync(TestEvent domainEvent)
        {
            HandledCount++;
            LastEvent = domainEvent;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task PublishEvent_WithSubscriber_ShouldInvokeHandler()
    {
        // Arrange
        var eventBus = new InMemoryEventBus();
        var handler = new TestEventHandler();
        eventBus.Subscribe(handler);

        var testEvent = new TestEvent
        {
            EventType = "TestEvent",
            Message = "Hello, World!"
        };

        // Act
        await eventBus.PublishAsync(testEvent);

        // Assert
        Assert.Equal(1, handler.HandledCount);
        Assert.NotNull(handler.LastEvent);
        Assert.Equal("Hello, World!", handler.LastEvent.Message);
    }

    [Fact]
    public async Task PublishEvent_WithMultipleSubscribers_ShouldInvokeAllHandlers()
    {
        // Arrange
        var eventBus = new InMemoryEventBus();
        var handler1 = new TestEventHandler();
        var handler2 = new TestEventHandler();
        eventBus.Subscribe(handler1);
        eventBus.Subscribe(handler2);

        var testEvent = new TestEvent { EventType = "TestEvent", Message = "Test" };

        // Act
        await eventBus.PublishAsync(testEvent);

        // Assert
        Assert.Equal(1, handler1.HandledCount);
        Assert.Equal(1, handler2.HandledCount);
    }

    [Fact]
    public async Task UnsubscribeHandler_ShouldNotReceiveEvents()
    {
        // Arrange
        var eventBus = new InMemoryEventBus();
        var handler = new TestEventHandler();
        eventBus.Subscribe(handler);
        eventBus.Unsubscribe(handler);

        var testEvent = new TestEvent { EventType = "TestEvent" };

        // Act
        await eventBus.PublishAsync(testEvent);

        // Assert
        Assert.Equal(0, handler.HandledCount);
    }

    [Fact]
    public async Task PublishBatch_ShouldHandleAllEvents()
    {
        // Arrange
        var eventBus = new InMemoryEventBus();
        var handler = new TestEventHandler();
        eventBus.Subscribe(handler);

        var events = new[]
        {
            new TestEvent { EventType = "Test1", Message = "1" },
            new TestEvent { EventType = "Test2", Message = "2" },
            new TestEvent { EventType = "Test3", Message = "3" }
        };

        // Act
        await eventBus.PublishBatchAsync(events);

        // Assert
        Assert.Equal(3, handler.HandledCount);
    }
}
