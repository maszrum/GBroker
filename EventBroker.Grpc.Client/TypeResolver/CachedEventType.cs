using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EventBroker.Grpc.Client.TypeResolver
{
    public class CachedEventType
    {
        private readonly ConcurrentDictionary<string, PropertyInfo> _namesToProperties =
            new ConcurrentDictionary<string, PropertyInfo>();

        private PropertyInfo[] _properties;

        private ConstructorInfo _constructor;

        public CachedEventType(Type type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public Type Type { get; }

        public ConstructorInfo GetConstructor()
        {
            if (_constructor == null)
            {
                var ctorInfo = Type.GetConstructor(Type.EmptyTypes);

                _constructor = ctorInfo ?? throw new MissingMethodException(
                        $"event type: {Type.FullName} must contain parameterless public constructor");
            }

            return _constructor;
        }

        public PropertyInfo GetProperty(string name)
        {
            if (!_namesToProperties.TryGetValue(name, out var propertyInfo))
            {
                propertyInfo = Type.GetProperty(name);
                if (propertyInfo == null || propertyInfo.GetSetMethod() == null)
                {
                    return null;
                }

                _namesToProperties.TryAdd(name, propertyInfo);
            }

            return propertyInfo;
        }

        public IReadOnlyList<PropertyInfo> GetProperties()
        {
            if (_properties == null)
            {
                _properties = Type
                    .GetProperties()
                    .Where(p => p.GetSetMethod(false) != null)
                    .ToArray();
            }

            return _properties;
        }
    }
}
