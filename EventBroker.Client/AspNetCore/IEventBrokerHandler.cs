using EventBroker.Core;

namespace EventBroker.Client.AspNetCore
{
    public interface IEventBrokerHandler
    {
        void Setup(IEventsConsumer consumer, ConsumptionType consumptionType);
    }
}
