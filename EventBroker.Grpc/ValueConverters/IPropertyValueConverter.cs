using System;

namespace EventBroker.Grpc.ValueConverters
{
    public interface IPropertyValueConverter
    {
        object ToValue(Type type, byte[] data);
        byte[] ToBytes(Type type, object value);
    }
}
