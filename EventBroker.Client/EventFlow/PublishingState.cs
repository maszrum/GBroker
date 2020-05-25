using System.Collections.Generic;
using EventBroker.Core;

namespace EventBroker.Client.EventFlow
{
	internal class PublishingState<TEvent> : IPublishingState<TEvent> where TEvent : IEvent
	{
		public PublishingState(TEvent e)
		{
			Event = e;
		}

		public HashSet<string> ServicesHandled { get; } = new HashSet<string>();
		public TEvent Event { get; }
	}
}