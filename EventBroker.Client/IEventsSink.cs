using System.Threading.Tasks;
using EventBroker.Client.EventFlow;
using EventBroker.Core;

namespace EventBroker.Client
{
	public interface IEventsSink
	{
		Task SendEventAsync<TEvent>(IPublishingState<TEvent> state) where TEvent : IEvent;
		void SendEvent<TEvent>(IPublishingState<TEvent> state) where TEvent : IEvent;
	}
}
