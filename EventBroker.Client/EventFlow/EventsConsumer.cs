using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using EventBroker.Client.Interceptor;
using EventBroker.Core;

namespace EventBroker.Client.EventFlow
{
	internal class EventsConsumer : IEventsConsumer
	{
		private readonly HashSet<IEventsSource> _sources;
		private readonly HashSet<IEventInterceptor> _interceptors;
		private readonly ConsumptionTypes _consumptionTypes = new ConsumptionTypes();

		public EventsConsumer(IEnumerable<IEventsSource> sources, IEnumerable<IEventInterceptor> interceptors)
		{
			_interceptors = interceptors.ToHashSet();
			_sources = sources.ToHashSet();


			foreach (var source in _sources)
			{
				source.Start();
			}
		}

		public IObservable<TEvent> EventsOfType<TEvent>() where TEvent : IEvent
		{
			var consumptionType = _consumptionTypes.Get<TEvent>();

			return GetObservableForEventType<TEvent>(consumptionType);
		}

		public IObservable<TEvent> EventsOfType<TEvent>(ConsumptionType consumptionType) where TEvent : IEvent
		{
			ConfigureSubscription<TEvent>(consumptionType);

			return GetObservableForEventType<TEvent>(consumptionType);
		}

		public void ConfigureSubscription<TEvent>(ConsumptionType consumptionType)
		{
			_consumptionTypes.Set<TEvent>(consumptionType);
		}

		public void Dispose()
		{
			foreach (var source in _sources)
			{
				source.Dispose();
			}
		}

		private IObservable<TEvent> GetObservableForEventType<TEvent>(
			ConsumptionType consumptionType) where TEvent : IEvent
		{
			static IObservable<(TEvent, Type)> CreateTuple(IEventsSource source, ConsumptionType ct)
			{
				return source
					.EventsOfType<TEvent>(ct)
					.Select(e => 
						(e, source.GetType()));
			}

			return _sources
				.Select(s => CreateTuple(s, consumptionType))
				.Merge()
				.Select(Intercept);
		}

		private TEvent Intercept<TEvent>((TEvent, Type) eventAndSourceType) where TEvent : IEvent
		{
			var (ev, sourceType) = eventAndSourceType;

			return _interceptors
				.Aggregate(ev, (current, interceptor) => 
					interceptor.InterceptIncoming(current, sourceType));
		}
	}
}
