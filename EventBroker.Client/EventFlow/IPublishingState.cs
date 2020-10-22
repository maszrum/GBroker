using System.Collections.Generic;
using EventBroker.Core;

namespace EventBroker.Client.EventFlow
{
    public interface IPublishingState<out TEvent> where TEvent : IEvent
    {
        HashSet<string> ServicesHandled { get; }
        TEvent Event { get; }
    }
}
