using System;
using EventBroker.Core;

namespace EventBroker.Client
{
    public interface IEventsSource : IDisposable
    {
        IObservable<TEvent> EventsOfType<TEvent>(ConsumptionType consumptionType) where TEvent : IEvent;
        void Start();
    }
}
