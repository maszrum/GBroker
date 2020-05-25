using System;
using EventBroker.Core;

namespace EventBroker.Grpc.Server.Sessions
{
    public class SubscribedEvent
    {
        public SubscribedEvent(string eventName, ConsumptionType consumptionType)
        {
            EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
            ConsumptionType = consumptionType;
        }

        public string EventName { get; }
        public ConsumptionType ConsumptionType { get; set; }

        public override int GetHashCode()
            => EventName.GetHashCode();

        public static implicit operator string(SubscribedEvent se)
            => se.EventName;
    }
}