using System;
using EventBroker.Core;

namespace EventBroker.Client.AspNetCore
{
	public abstract class EventBrokerHandler<TEvent> : IEventBrokerHandler, IDisposable where TEvent : IEvent
	{
		private IDisposable _subscription;

		public void Setup(IEventsConsumer consumer, ConsumptionType consumptionType)
		{
			var observable = consumer.EventsOfType<TEvent>(consumptionType);
			var filtered = ConfigureFilter(observable);

			_subscription = filtered.Subscribe(HandleEvent);
		}

		protected virtual IObservable<TEvent> ConfigureFilter(IObservable<TEvent> events)
		{
			return events;
		}

		protected abstract void HandleEvent(TEvent ev);

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_subscription?.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
