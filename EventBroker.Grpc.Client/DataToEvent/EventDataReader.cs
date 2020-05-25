using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EventBroker.Grpc.Data;

namespace EventBroker.Grpc.Client.DataToEvent
{
    internal class EventDataReader : IEnumerable<EventPropertyBinding>
    {
        private readonly IEventData _eventData;
        private readonly Func<string, byte[], (PropertyInfo, object)> _valueResolver;

        public EventDataReader(IEventData eventData, Func<string, byte[], (PropertyInfo, object)> propertyValueResolver)
        {
            _eventData = eventData ?? throw new ArgumentNullException(nameof(eventData));
            _valueResolver = propertyValueResolver ?? throw new ArgumentNullException(nameof(propertyValueResolver));
            
            if (eventData.PropertyPositions.Any())
            {
                var lastPosition = eventData.PropertyPositions.Last();
                var dataLength = eventData.GetData().Length;
                if (lastPosition >= dataLength)
                {
                    throw new ArgumentOutOfRangeException(
                        $"last position ({lastPosition}) must be less than data length ({dataLength})");
                }

                var propertyNamesCount = eventData.PropertyNames.Count;
                var propertyPositionsCount = eventData.PropertyPositions.Count;
                if (propertyNamesCount != propertyPositionsCount)
                {
                    throw new ArgumentException(
                        $"property names count ({propertyNamesCount}) must be equal to propery positions count ({propertyPositionsCount})");
                }
            }
        }

        public IEnumerator<EventPropertyBinding> GetEnumerator()
        {
            if (_eventData.PropertyPositions.Any())
            {
                var positions = _eventData.PropertyPositions;
                var data = _eventData.GetData();
                var names = _eventData.PropertyNames;

                for (var i = 0; i < positions.Count; i++)
                {
                    var position = positions[i];
                    var name = names[i];

                    var dataLength = positions.Count - 1 == i ?
                        data.Length - position :
                        positions[i + 1] - position;

                    var parameterData = new byte[dataLength];
                    Array.Copy(data, position, parameterData, 0, dataLength);

                    var (propertyInfo, bytes) = _valueResolver(name, parameterData);

                    yield return new EventPropertyBinding(propertyInfo, bytes);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
