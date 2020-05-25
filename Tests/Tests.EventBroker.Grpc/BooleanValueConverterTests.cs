using System;
using EventBroker.Grpc.ValueConverters;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc
{
    [TestFixture]
    internal class BooleanValueConverterTests
    {
        [Test]
        public void convert_boolean_to_byte_array()
        {
            var converter = new BooleanValueConverter();

            var trueBytes = converter.ToBytes(true);
            var falseBytes = converter.ToBytes(false);

            Assert.Multiple(() =>
            {
                Assert.That(trueBytes.Length, Is.EqualTo(1));
                Assert.That(falseBytes.Length, Is.EqualTo(1));

                Assert.That(trueBytes[0], Is.EqualTo((byte)1));
                Assert.That(falseBytes[0], Is.EqualTo((byte)0));
            });
        }

        [Test]
        public void convert_byte_array_to_boolean()
        {
            var converter = new BooleanValueConverter();

            var trueBytes = new[] { (byte)1 };
            var falseBytes = new[] { (byte)0 };

            var trueObject = converter.ToValue(trueBytes);
            var falseObject = converter.ToValue(falseBytes);

            Assert.Multiple(() =>
            {
                Assert.That(trueObject, Is.TypeOf<bool>());
                Assert.That(falseObject, Is.TypeOf<bool>());

                Assert.IsTrue((bool)trueObject);
                Assert.IsFalse((bool)falseObject);
            });
        }

        [Test]
        public void should_throw_exception_on_invalid_array_size()
        {
            var converter = new BooleanValueConverter();

            var bytes = new[] { (byte)1, (byte)2, (byte)3 };
            var noBytes = Array.Empty<byte>();

            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentException>(() => converter.ToValue(bytes));
                Assert.Throws<ArgumentException>(() => converter.ToValue(noBytes));
            });
        }
    }
}
