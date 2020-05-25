using System;
using System.Collections.Generic;
using System.Linq;
using EventBroker.Core;
using EventBroker.Grpc.Client.Core;

namespace EventBroker.Grpc.Client.Source
{
    internal class EventsSubscriber
    {
        private readonly IGrpcClient _grpc;
        private readonly HashSet<SubscribedEvent> _events = new HashSet<SubscribedEvent>();

        public EventsSubscriber(IGrpcClient grpc)
        {
            _grpc = grpc ?? throw new ArgumentNullException(nameof(grpc));
        }

        public void Subscribe(string eventName, ConsumptionType consumptionType)
        {
            var existing = _events
                .SingleOrDefault(e => e == eventName);

            if (existing == null)
            {
                var subscribedEvent = new SubscribedEvent(eventName, consumptionType);
                _events.Add(subscribedEvent);

                try
                {
                    _grpc.Subscribe(eventName, consumptionType);
                }
#pragma warning disable CA1031
                catch { }
#pragma warning restore CA1031
            }
            else if (existing.ConsumptionType != consumptionType)
            {
                throw new InvalidOperationException(
                    $"event {eventName} has already been subscribed with another consumption type, cannot change");
            }
        }

        public void Unsubscribe(string eventName)
        {
            var subscription = _events
                .SingleOrDefault(s => s == eventName);

            if (subscription != null)
            {
                _events.Remove(subscription);

                try
                {
                    _grpc.Unsubscribe(eventName);
                }
#pragma warning disable CA1031
                catch
                {
                }
#pragma warning restore CA1031
            }
        }

        public void RefreshSubscriptions()
        {
            if (_events.Count > 0)
            {
                var tuples = _events
                    .Select(e => (e.EventName, e.ConsumptionType));

                _grpc.SubscribeMany(tuples);
            }
        }
    }
}
