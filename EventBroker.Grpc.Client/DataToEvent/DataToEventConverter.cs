using System;
using System.Linq;
using EventBroker.Core;
using EventBroker.Grpc.Data;
using EventBroker.Grpc.ValueConverters;

namespace EventBroker.Grpc.Client.DataToEvent
{
    public class DataToEventConverter
    {
        private readonly IPropertyValueConverter _converter;
        private readonly IEventTypeResolver _eventTypeResolver;

        public DataToEventConverter(IPropertyValueConverter converter, IEventTypeResolver eventTypeResolver)
        {
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
            _eventTypeResolver = eventTypeResolver ?? throw new ArgumentNullException(nameof(eventTypeResolver));
        }

        public IEvent Convert(EventData eventData)
        {
            var wrapper = EventDataWrapper.FromGrpcMessage(eventData);
            return Convert(wrapper);
        }

        public IEvent Convert(IEventData eventData)
        {
            var type = _eventTypeResolver.GetEventType(eventData.EventName);
            var constructor = _eventTypeResolver.GetEventConstructor(type);

            var instance = (IEvent)constructor.Invoke(Array.Empty<object>());

            var parametersEnumerator = new EventDataReader(eventData, (propertyName, data) =>
            {
                var propertyInfo = _eventTypeResolver.GetPropertyInfo(type, propertyName);
                if (propertyInfo == null)
                {
                    return (null, null);
                }

                var propertyType = propertyInfo.PropertyType;

                var propertyValue = _converter.ToValue(propertyType, data);
                return (propertyInfo, propertyValue);
            });

            var bindings = parametersEnumerator
                .Where(p => p.Property != null);

            foreach (var propertyBinding in bindings)
            {
                var propertyInfo = propertyBinding.Property;
                var propertyValue = propertyBinding.Value;

                propertyInfo.SetValue(instance, propertyValue);
            }

            return instance;
        }
    }
}
