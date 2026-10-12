namespace GameBacklog.Api.Events
{
    public class LoggingEventPublisher(ILogger<LoggingEventPublisher> logger) : IEventPublisher
    {
        public Task PublishAsync<TEvent>(TEvent @event) where TEvent : class
        {
            logger.LogInformation("Event {EventType}: {@Event}", typeof(TEvent).Name, @event);
            return Task.CompletedTask;
        }
    }
}
