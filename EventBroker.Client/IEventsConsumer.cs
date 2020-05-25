using System;
using EventBroker.Core;

namespace EventBroker.Client
{
    public interface IEventsConsumer : IDisposable
    {
        void ConfigureSubscription<TEvent>(ConsumptionType consumptionType);
        IObservable<TEvent> EventsOfType<TEvent>() where TEvent : IEvent;
        IObservable<TEvent> EventsOfType<TEvent>(ConsumptionType consumptionType) where TEvent : IEvent;
    }
}
