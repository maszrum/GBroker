using System;
using System.Collections.Generic;
using EventBroker.Client.EventFlow;
using EventBroker.Client.Exceptions;
using EventBroker.Client.Interceptor;
using EventBroker.Client.Local;

namespace EventBroker.Client
{
    internal sealed class EventBrokerClient : IEventBrokerClient
    {
        private readonly EventsProducer _producer;
        private readonly EventsConsumer _consumer;
        private readonly ExceptionsCatcher _exceptionsCatcher;

        public EventBrokerClient(
            string serviceIdentificator, 
            ISet<IEventsSink> sinks, 
            ISet<IEventsSource> sources, 
            ISet<IEventInterceptor> interceptors,
            ExceptionsCatcher exceptionsCatcher)
        {
            if (string.IsNullOrWhiteSpace(serviceIdentificator))
            {
                throw new ArgumentNullException(nameof(serviceIdentificator));
            }

            ServiceIdentificator = serviceIdentificator;

            var localSinkSource = new LocalEventsSinkSource(serviceIdentificator);
            sinks.Add(localSinkSource);
            sources.Add(localSinkSource);

            _producer = new EventsProducer(sinks, interceptors);
            _consumer = new EventsConsumer(sources, interceptors);

            _exceptionsCatcher = exceptionsCatcher 
                ?? throw new ArgumentNullException(nameof(exceptionsCatcher));
        }

        public IEventsProducer Producer => _producer;

        public IEventsConsumer Consumer => _consumer;

        public IExceptionsCatcher ExceptionsCatcher => _exceptionsCatcher;

        public string ServiceIdentificator { get; }

        public void Dispose()
        {
            _exceptionsCatcher.Dispose();
            _consumer.Dispose();
        }
    }
}
