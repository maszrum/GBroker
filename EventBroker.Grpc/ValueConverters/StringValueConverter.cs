using System;
using System.Text;

namespace EventBroker.Grpc.ValueConverters
{
	public class StringValueConverter : IValueConverter
	{
		public byte[] ToBytes(object value)
		{
			var s = (string)value;

			if (s == null)
			{
				return Array.Empty<byte>();
			}
			if (s.Length == 0)
			{
				return new byte[] { 0x02, 0x03 };
			}

			return Encoding.UTF8.GetBytes(s);
		}

		public object ToValue(byte[] data)
		{
			return data.Length switch
			{
				0 => null,
				2 when data[0] == 0x02 && data[1] == 0x03 => string.Empty,
				_ => Encoding.UTF8.GetString(data)
			};
		}
	}
}
