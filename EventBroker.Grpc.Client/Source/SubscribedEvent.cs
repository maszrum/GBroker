using System;
using EventBroker.Core;

namespace EventBroker.Grpc.Client.Source
{
    internal class SubscribedEvent
    {
        public SubscribedEvent(string eventName, ConsumptionType consumptionType)
        {
            EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
            ConsumptionType = consumptionType;
        }

        public string EventName { get; }
        public ConsumptionType ConsumptionType { get; }

        public override int GetHashCode()
            => EventName.GetHashCode();

        public static implicit operator string(SubscribedEvent se)
            => se.EventName;
    }
}
