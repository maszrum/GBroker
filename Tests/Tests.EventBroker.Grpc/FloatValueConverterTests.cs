using System;
using EventBroker.Grpc.ValueConverters;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc
{
    internal class FloatValueConverterTests
    {
        [TestCase(-12.57F)]
        [TestCase(-12035.57F)]
        [TestCase(12.57439F)]
        [TestCase(-13952.57F)]
        [TestCase(122.457F)]
        [TestCase(-0.57F)]
        public void convert_double_in_both_directions(float value)
        {
            var converter = new FloatValueConverter();

            var bytes = converter.ToBytes(value);
            var convertedNumber = converter.ToValue(bytes);

            Assert.That(convertedNumber, Is.EqualTo(value));
        }

        [TestCase(new byte[] { 1, 2, 3, 4, 5 })]
        [TestCase(new byte[] { 1, 2, 3 })]
        [TestCase(new byte[] { })]
        public void should_throw_on_incorrect_bytes_count(byte[] bytes)
        {
            var converter = new FloatValueConverter();

            Assert.Throws<ArgumentException>(() => converter.ToValue(bytes));
        }
    }
}
