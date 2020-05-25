using System;

namespace EventBroker.Grpc.ValueConverters
{
    public class FloatValueConverter : IValueConverter
    {
        public byte[] ToBytes(object value)
        {
            var v = (float)value;
            return BitConverter.GetBytes(v);
        }

        public object ToValue(byte[] data)
        {
            if (data.Length != 4)
            {
                throw new ArgumentException(
                    "must contain 4 bytes", nameof(data));
            }

            return BitConverter.ToSingle(data);
        }
    }
}
