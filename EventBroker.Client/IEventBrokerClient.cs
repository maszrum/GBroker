using System;
using EventBroker.Client.Exceptions;

namespace EventBroker.Client
{
    public interface IEventBrokerClient : IDisposable
    {
        IEventsProducer Producer { get; }
        IEventsConsumer Consumer { get; }
        IExceptionsCatcher ExceptionsCatcher { get; }
        string ServiceIdentificator { get; }
    }
}
