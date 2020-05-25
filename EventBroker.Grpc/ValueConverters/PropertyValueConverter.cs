using System;
using System.Collections.Generic;

namespace EventBroker.Grpc.ValueConverters
{
    public class PropertyValueConverter : IPropertyValueConverter
    {
        private readonly Dictionary<Type, IValueConverter> _typesConverters =
            new Dictionary<Type, IValueConverter>();

        public PropertyValueConverter RegisterConverter<TType>(IValueConverter converter)
        {
            _typesConverters.Add(typeof(TType), converter);
            return this;
        }

        public byte[] ToBytes(Type type, object value)
        {
            var typeConverter = GetTypeConverter(type);
            return typeConverter.ToBytes(value);
        }

        public object ToValue(Type type, byte[] data)
        {
            var typeConverter = GetTypeConverter(type);
            return typeConverter.ToValue(data);
        }

        private IValueConverter GetTypeConverter(Type type)
        {
            if (type.IsEnum)
            {
                var enumType = type;
                type = Enum.GetUnderlyingType(enumType);
            }

            if (_typesConverters.TryGetValue(type, out var converter))
            {
                return converter;
            }

            throw new InvalidCastException(
                $"cannot convert data of type {type.Name}, unknown converter");
        }
    }
}
