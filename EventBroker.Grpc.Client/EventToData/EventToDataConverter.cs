using System;
using System.IO;
using EventBroker.Core;
using EventBroker.Grpc.Client.DataToEvent;
using EventBroker.Grpc.Data;
using EventBroker.Grpc.ValueConverters;

namespace EventBroker.Grpc.Client.EventToData
{
    public class EventToDataConverter
    {
        private readonly IPropertyValueConverter _converter;
        private readonly IEventTypeResolver _eventTypeResolver;

        public EventToDataConverter(IPropertyValueConverter converter, IEventTypeResolver eventTypeResolver)
        {
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
            _eventTypeResolver = eventTypeResolver ?? throw new ArgumentNullException(nameof(eventTypeResolver));
        }

        public IEventData Convert<TEvent>(TEvent e) where TEvent : IEvent
        {
            var eventType = typeof(TEvent);

            var eventData = new EventDataWrapper()
            {
                EventName = GetClassName(eventType)
            };

            var propertiesToRead = _eventTypeResolver.GetProperties(eventType);

            var memoryStream = new MemoryStream();

            var reader = new EventObjectReader(e, propertiesToRead, (obj, type) 
                => _converter.ToBytes(type, obj));

            foreach (var propertyData in reader)
            {
                memoryStream.Write(propertyData.Bytes);

                eventData.PropertyNames.Add(propertyData.PropertyName);
                eventData.PropertyPositions.Add(propertyData.Position);
            }

            eventData.SetData(memoryStream.ToArray());

            return eventData;
        }

        public static string GetClassName<TEvent>() where TEvent : IEvent
        {
            return GetClassName(typeof(TEvent));
        }

        public static string GetClassName(Type eventType)
        {
            return eventType.FullName;
        }
    }
}
