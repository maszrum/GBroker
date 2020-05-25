using System;
using EventBroker.Grpc.ValueConverters;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc
{
    [TestFixture]
    internal class DoubleValueConverterTests
    {
        [TestCase(-12.57)]
        [TestCase(-12035.57)]
        [TestCase(12.57439)]
        [TestCase(-13952.57)]
        [TestCase(122.457)]
        [TestCase(-0.57)]
        public void convert_double_in_both_directions(double value)
        {
            var converter = new DoubleValueConverter();

            var bytes = converter.ToBytes(value);
            var convertedNumber = converter.ToValue(bytes);

            Assert.That(convertedNumber, Is.EqualTo(value));
        }

        [TestCase(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
        [TestCase(new byte[] { 1, 2, 3, 4, 5, 6, 7 })]
        [TestCase(new byte[] { })]
        public void should_throw_on_incorrect_bytes_count(byte[] bytes)
        {
            var converter = new DoubleValueConverter();

            Assert.Throws<ArgumentException>(() => converter.ToValue(bytes));
        }
    }
}
