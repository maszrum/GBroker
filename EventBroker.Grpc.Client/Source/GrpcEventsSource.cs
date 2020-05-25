using System;
using System.Collections.Generic;
using EventBroker.Client;
using EventBroker.Client.Exceptions;
using EventBroker.Core;
using EventBroker.Grpc.Client.Core;
using EventBroker.Grpc.Client.EventToData;
using EventBroker.Grpc.Data;

namespace EventBroker.Grpc.Client.Source
{
    internal class GrpcEventsSource : IEventsSource
    {
        private readonly Dictionary<Type, ISubjectWrapper> _subjects = 
            new Dictionary<Type, ISubjectWrapper>();

        private readonly EventsListener _eventsListener;

        private readonly EventsSubscriber _subscriber;

        private readonly IExceptionsSink _exceptionsSink;

        public GrpcEventsSource(IGrpcClient grpc, ISessionInitializer initializer, IExceptionsSink exceptionsSink)
        {
            if (initializer == null)
            {
                throw new ArgumentNullException(nameof(initializer));
            }

            _exceptionsSink = exceptionsSink ?? throw new ArgumentNullException(nameof(exceptionsSink));

            _subscriber = new EventsSubscriber(grpc);

            _eventsListener = new EventsListener(grpc, () =>
            {
                initializer.Initialize();

                _subscriber.RefreshSubscriptions();
            });
        }

        public int WaitOnExceptionMilliseconds
        {
            get => _eventsListener.WaitOnExceptionMilliseconds;
            set => _eventsListener.WaitOnExceptionMilliseconds = value;
        }

        public void Start()
        {
            _eventsListener.Start(
                eventData =>
                {
                    try
                    {
                        ProcessEventData(eventData);
                    }
#pragma warning disable CA1031
                    catch (Exception ex)
                    {
                        var processingException = new EventDataProcessingException(
                            "Error while processing incoming event data", ex);

                        _exceptionsSink.NextException(processingException);
                    }
#pragma warning restore CA1031
                },
                exception =>
                {
                    _exceptionsSink.NextException(exception);
                });
        }

        public IObservable<TEvent> EventsOfType<TEvent>(ConsumptionType consumptionType) where TEvent : IEvent
        {
            var subject = GetSubjectTyped<TEvent>();

            if (subject == null)
            {
                var eventType = typeof(TEvent);

                subject = SubjectWrapper<TEvent>.Create();
                _subjects.Add(eventType, subject);

                var eventName = EventToDataConverter.GetClassName(eventType);

                _subscriber.Subscribe(eventName, consumptionType);
            }

            return subject.AsObservable();
        }

        public void Dispose()
        {
            _eventsListener.Dispose();
        }

        private void ProcessEventData(IEventData eventData)
        {
            var ev = EventConverter.DataToEvent().Convert(eventData);

            var eventType = ev.GetType();
            var subject = GetSubject(eventType);

            if (subject == null || !subject.HasObservers)
            {
                var eventName = EventToDataConverter.GetClassName(eventType);
                _subscriber.Unsubscribe(eventName);
            }
            else
            {
                subject.OnNext(ev);
            }
        }

        private ISubjectWrapper GetSubject(Type eventType)
        {
            return _subjects.TryGetValue(eventType, out var subject) 
                ? subject 
                : null;
        }

        private SubjectWrapper<TEvent> GetSubjectTyped<TEvent>() where TEvent : IEvent
        {
            var eventType = typeof(TEvent);
            if (_subjects.TryGetValue(eventType, out var subject))
            {
                return (SubjectWrapper<TEvent>)subject;
            }
            return null;
        }
    }
}
