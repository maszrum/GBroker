using System.Collections.Immutable;
using System.Linq;
using EventBroker.Core;

namespace EventBroker.Grpc.Server.Sessions
{
    public class EventSubscriptions
    {
        private ImmutableHashSet<SubscribedEvent> _subscribedEvents = ImmutableHashSet<SubscribedEvent>.Empty;

        public void Register(string eventName, ConsumptionType consumptionType)
        {
            var subscription = GetSubscriptionOrDefault(eventName);
            if (subscription != null)
            {
                subscription.ConsumptionType = consumptionType;
            }
            else
            {
                subscription = new SubscribedEvent(eventName, consumptionType);
                _subscribedEvents = _subscribedEvents.Add(subscription);
            }
        }

        public void Remove(string eventName)
        {
            var subscription = GetSubscriptionOrDefault(eventName);
            if (subscription != null)
            {
                _subscribedEvents = _subscribedEvents.Remove(subscription);
            }
        }

        public bool Exists(string eventName, out ConsumptionType consumptionType)
        {
            var subscription = GetSubscriptionOrDefault(eventName);

            if (subscription != null)
            {
                consumptionType = subscription.ConsumptionType;
                return true;
            }

            consumptionType = default;
            return false;
        }

        public bool Exists(string eventName)
            => _subscribedEvents.Any(s => s.EventName == eventName);

        private SubscribedEvent GetSubscriptionOrDefault(string eventName)
            => _subscribedEvents.SingleOrDefault(s => s == eventName);
    }
}