using EventBroker.Grpc.ValueConverters;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc
{
	internal class StringValueConverterTests
	{
		[TestCase("test")]
		[TestCase("test test")]
		[TestCase("test 123 qwerty")]
		public void test_converting_in_both_directions(string input)
		{
			var converter = new StringValueConverter();

			var bytes = converter.ToBytes(input);
			var text = converter.ToValue(bytes);

			Assert.Multiple(() =>
			{
				Assert.That(bytes.Length, Is.EqualTo(input.Length));
				Assert.That(text, Is.EqualTo(input));
			});
		}

		[Test]
		public void test_null()
		{
			var converter = new StringValueConverter();

			var bytes = converter.ToBytes(null);
			var text = converter.ToValue(bytes);

			Assert.That(text, Is.Null);
		}

		[Test]
		public void test_empty()
		{
			var converter = new StringValueConverter();

			var bytes = converter.ToBytes(string.Empty);
			var text = converter.ToValue(bytes);

			Assert.That(text, Is.EqualTo(string.Empty));
		}
	}
}
