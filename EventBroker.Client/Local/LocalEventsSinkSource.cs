using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using EventBroker.Client.EventFlow;
using EventBroker.Core;

namespace EventBroker.Client.Local
{
    internal class LocalEventsSinkSource : IEventsSink, IEventsSource
    {
        private readonly ArrayList _subjects = new ArrayList();

        private readonly Dictionary<Type, ConsumptionType> _consumptionTypes 
            = new Dictionary<Type, ConsumptionType>();

        private readonly string _serviceIdentificator;

        public LocalEventsSinkSource(string serviceIdentificator)
        {
            _serviceIdentificator = serviceIdentificator ?? throw new ArgumentNullException(nameof(serviceIdentificator));
        }

        public void SendEvent<TEvent>(IPublishingState<TEvent> state) where TEvent : IEvent
        {
            var subject = GetSubject<TEvent>();

            if (subject != null)
            {
                subject.OnNext(state.Event);

                var consumptionType = _consumptionTypes[typeof(TEvent)];
                if (consumptionType == ConsumptionType.OneEventPerServiceType)
                {
                    state.ServicesHandled.Add(_serviceIdentificator);
                }
            }
        }

        public Task SendEventAsync<TEvent>(IPublishingState<TEvent> state) where TEvent : IEvent
        {
            SendEvent(state);
            return Task.CompletedTask;
        }

        public IObservable<TEvent> EventsOfType<TEvent>(ConsumptionType consumptionType) where TEvent : IEvent
        {
            var subject = GetSubject<TEvent>();

            if (subject == null)
            {
                subject = new Subject<TEvent>();
                _subjects.Add(subject);

                _consumptionTypes.Add(typeof(TEvent), consumptionType);
            }
            else
            {
                var currentConsumptionType = _consumptionTypes[typeof(TEvent)];
                if (currentConsumptionType != consumptionType)
                {
                    throw new InvalidOperationException(
                        $"subscription of specified event type {typeof(TEvent).Name} was created before with another consumption type");
                }
            }

            return subject
                .AsObservable();
        }

        private Subject<TEvent> GetSubject<TEvent>()
        {
            return _subjects
                .OfType<Subject<TEvent>>()
                .SingleOrDefault();
        }

        public void Start()
        {
        }

        public void Dispose()
        {
        }
    }
}
