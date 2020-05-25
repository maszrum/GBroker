using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EventBroker.Core;

namespace EventBroker.Grpc.Client.EventToData
{
    internal class EventObjectReader : IEnumerable<PropertyEventData>
    {
        private readonly IEnumerable<PropertyInfo> _properties;
        private readonly IEvent _event;
        private readonly Func<object, Type, byte[]> _toBytesConverter;

        public EventObjectReader(
            IEvent e, IEnumerable<PropertyInfo> properties, Func<object, Type, byte[]> toBytesConverter)
        {
            _event = e ?? throw new ArgumentNullException(nameof(e));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _toBytesConverter = toBytesConverter ?? throw new ArgumentNullException(nameof(toBytesConverter));
        }

        public IEnumerator<PropertyEventData> GetEnumerator()
        {
            var position = 0;
            foreach (var property in _properties)
            {
                var propertyType = property.PropertyType;
                var propertyValue = property.GetValue(_event);
                var bytes = _toBytesConverter(propertyValue, propertyType);

                yield return new PropertyEventData()
                {
                    Bytes = bytes,
                    Position = position,
                    PropertyName = property.Name
                };
                position += bytes.Length;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
