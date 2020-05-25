using System;

namespace EventBroker.Grpc.ValueConverters
{
    public class BooleanValueConverter : IValueConverter
    {
        public byte[] ToBytes(object value)
        {
            var boolean = (bool)value;
            return new[] { Convert.ToByte(boolean) };
        }

        public object ToValue(byte[] data)
        {
            if (data.Length != 1)
            {
                throw new ArgumentException(
                    $"byte array must contain exatcly 1 byte to be converted to bool", nameof(data));
            }

            return Convert.ToBoolean(data[0]);
        }
    }
}
