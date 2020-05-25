using System;

namespace EventBroker.Grpc.ValueConverters
{
    public class IntegerValueConverter : IValueConverter
    {
        public byte[] ToBytes(object value)
        {
            var integer = (int)value;
            return BitConverter.GetBytes(integer);
        }

        public object ToValue(byte[] data)
        {
            if (data.Length != 4)
            {
                throw new ArgumentException(
                    $"byte array must contain exatcly 4 bytes to be converted to int", nameof(data));
            }

            return BitConverter.ToInt32(data, 0);
        }
    }
}
