using System;

namespace EventBroker.Grpc.ValueConverters
{
    public class DoubleValueConverter : IValueConverter
    {
        public byte[] ToBytes(object value)
        {
            var v = (double)value;
            return BitConverter.GetBytes(v);
        }

        public object ToValue(byte[] data)
        {
            if (data.Length != 8)
            {
                throw new ArgumentException(
                    "must contain 8 bytes", nameof(data));
            }

            return BitConverter.ToDouble(data);
        }
    }
}
