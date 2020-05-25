using System;
using System.Collections.Generic;
using EventBroker.Core;

namespace EventBroker.Client.EventFlow
{
    internal class ConsumptionTypes
    {
        private readonly Dictionary<Type, ConsumptionType> _consumptionTypes 
            = new Dictionary<Type, ConsumptionType>();

        public ConsumptionType Get<TEvent>()
        {
            var eventType = typeof(TEvent);
            if (_consumptionTypes.TryGetValue(eventType, out var consumptionType))
            {
                return consumptionType;
            }

            throw new InvalidOperationException(
                $"consumption type of event {eventType.Name} was not configured, use configuration method");
        }

        public void Set<TEvent>(ConsumptionType type)
        {
            var eventType = typeof(TEvent);

            if (!_consumptionTypes.TryGetValue(eventType, out var currentType))
            {
                _consumptionTypes.Add(eventType, type);
            }
            else if (currentType != type)
            {
                throw new InvalidOperationException(
                    $"consumption type of event {eventType.Name} has already been configured as {currentType}, cannot change");
            }
        }
    }
}