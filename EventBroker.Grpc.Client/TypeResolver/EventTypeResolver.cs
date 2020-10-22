using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EventBroker.Grpc.Client.DataToEvent;

namespace EventBroker.Grpc.Client.TypeResolver
{
    internal class EventTypeResolver : IEventTypeResolver
    {
        private readonly HashSet<Assembly> _assemblies = new HashSet<Assembly>();

        private readonly ConcurrentDictionary<string, Type> _cachedEventNames 
            = new ConcurrentDictionary<string, Type>();

        private readonly ConcurrentDictionary<Type, CachedEventType> _cachedEventTypes 
            = new ConcurrentDictionary<Type, CachedEventType>();

        public bool ThrowOnNotExistingProperty { get; set; }

        public Type GetEventType(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(
                    nameof(name), "event name cannot be null");
            }

            if (_cachedEventNames.TryGetValue(name, out var result))
            {
                return result;
            }

            var type = _assemblies
                .Select(a => a.GetType(name))
                .FirstOrDefault(t => t != null);

            if (type == null)
            {
                throw new TypeLoadException(
                    $"cannot load event type {name}");
            }

            _cachedEventNames.TryAdd(name, type);

            return type;
        }

        public ConstructorInfo GetEventConstructor(Type type)
        {
            var cache = GetCache(type);
            return cache.GetConstructor();
        }

        public PropertyInfo GetPropertyInfo(Type type, string name)
        {
            var cache = GetCache(type);
            var property = cache.GetProperty(name);

            ThrowIfNotExistingProperty(property, type, name);

            return property;
        }

        public void RegisterEventsAssembly(Assembly assembly)
        {
            if (!_assemblies.Contains(assembly))
            {
                _assemblies.Add(assembly);
            }
        }

        public IReadOnlyList<PropertyInfo> GetProperties(Type type)
        {
            var cache = GetCache(type);
            return cache.GetProperties();
        }

        private CachedEventType GetCache(Type type)
        {
            if (!_cachedEventTypes.TryGetValue(type, out var eventTypeCache))
            {
                eventTypeCache = new CachedEventType(type);
                _cachedEventTypes.TryAdd(type, eventTypeCache);
            }

            return eventTypeCache;
        }

        private void ThrowIfNotExistingProperty(PropertyInfo propertyInfo, Type eventType, string propertyName)
        {
            if (ThrowOnNotExistingProperty
                && propertyInfo == null)
            {
                throw new InvalidOperationException(
                    $"error while converting event of type {eventType.FullName}: " + 
                    $"cannot find property {propertyName} with public getter and setter");
            }
        }
    }
}
