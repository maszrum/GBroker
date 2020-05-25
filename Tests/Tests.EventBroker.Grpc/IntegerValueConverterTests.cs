using System;
using EventBroker.Grpc.ValueConverters;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc
{
    internal class IntegerValueConverterTests
    {
        [TestCase(12, new byte[] { 0x0C, 0x00, 0x00, 0x00 })]
        [TestCase(120, new byte[] { 0x78, 0x00, 0x00, 0x00 })]
        [TestCase(1200, new byte[] { 0xB0, 0x04,0x00, 0x00 })]
        [TestCase(120000, new byte[] { 0xC0, 0xD4, 0x01, 0x00 })]
        [TestCase(1200000000, new byte[] { 0x00, 0x8C , 0x86, 0x47 })]
        public void convert_integer_to_byte_array(int toConvert, byte[] expectedOutput)
        {
            var converter = new IntegerValueConverter();

            var bytes = converter.ToBytes(toConvert);

            Assert.Multiple(() =>
            {
                Assert.That(bytes.Length, Is.EqualTo(4));
                CollectionAssert.AreEqual(expectedOutput, bytes);
            });
        }

        [TestCase(new byte[] { 0x0C, 0x00, 0x00, 0x00 }, 12)]
        [TestCase(new byte[] { 0x78, 0x00, 0x00, 0x00 }, 120)]
        [TestCase(new byte[] { 0xB0, 0x04, 0x00, 0x00 }, 1200)]
        [TestCase(new byte[] { 0xC0, 0xD4, 0x01, 0x00 }, 120000)]
        [TestCase(new byte[] { 0x00, 0x8C, 0x86, 0x47 }, 1200000000)]
        public void convert_byte_array_to_integer(byte[] bytesToConvert, int expectedConverted)
        {
            var converter = new IntegerValueConverter();

            var integerObject = converter.ToValue(bytesToConvert);

            Assert.Multiple(() =>
            {
                Assert.That(integerObject, Is.TypeOf<int>());
                Assert.That((int)integerObject, Is.EqualTo(expectedConverted));
            });
        }

        [TestCase(new byte[] { 0, 1, 2, 3, 4 })]
        [TestCase(new byte[] { 0, 1, 2 })]
        [TestCase(new byte[] { 0, 1 })]
        [TestCase(new byte[] { 0 })]
        public void should_throw_on_invalid_bytes_count(byte[] bytes)
        {
            var converter = new IntegerValueConverter();

            Assert.Throws<ArgumentException>(() => converter.ToValue(bytes));
        }
    }
}
