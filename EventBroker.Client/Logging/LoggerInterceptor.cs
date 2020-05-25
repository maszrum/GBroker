using System;
using System.Threading.Tasks;
using EventBroker.Client.Interceptor;
using EventBroker.Core;
using Microsoft.Extensions.Logging;

namespace EventBroker.Client.Logging
{
    internal class LoggerInterceptor : IEventInterceptor
    {
        private readonly ILogger<IEventBrokerClient> _logger;

        public LoggerInterceptor(ILogger<IEventBrokerClient> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public TEvent InterceptIncoming<TEvent>(TEvent ev, Type sourceType) where TEvent : IEvent
        {
            LogIncoming<TEvent>(sourceType);
            return ev;
        }

        public TEvent InterceptOutgoing<TEvent>(TEvent ev) where TEvent : IEvent
        {
            LogOutgoing<TEvent>();
            return ev;
        }

        public Task<TEvent> InterceptOutgoingAsync<TEvent>(TEvent ev) where TEvent : IEvent
        {
            LogOutgoing<TEvent>();
            return Task.FromResult(ev);
        }

        public void LogException(Exception exception)
        {
            _logger.LogError(
                exception, "Exception was thrown in event broker client on {Time}", DateTime.UtcNow);
        }

        private void LogIncoming<TEvent>(Type sourceType)
        {
            _logger.LogInformation(
                "Received event of type {EventType} from source {SourceType}",
                typeof(TEvent).Name, sourceType.FullName);
        }

        private void LogOutgoing<TEvent>()
        {
            _logger.LogInformation(
                "Sending event of type {EventType}", typeof(TEvent).Name);
        }
    }
}
