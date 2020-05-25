using System;
using System.Text;
using EventBroker.Grpc.ValueConverters;
using Moq;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc
{
    internal class PropertyValueConverterTests
    {
        [Test]
        public void should_convert_bytes_to_string_and_int()
        {
            var converter = new PropertyValueConverter();

            converter.RegisterConverter<string>(MockStringConverter());
            converter.RegisterConverter<int>(MockIntConverter());

            var intConverted = converter.ToValue(typeof(int), BitConverter.GetBytes(-163563));
            var stringConverted = converter.ToValue(typeof(string), Encoding.UTF8.GetBytes("sample string 653"));

            Assert.Multiple(() =>
            {
                Assert.That(intConverted, Is.TypeOf<int>());
                Assert.That(intConverted, Is.EqualTo(-163563));

                Assert.That(stringConverted, Is.TypeOf<string>());
                Assert.That(stringConverted, Is.EqualTo("sample string 653"));
            });
        }

        [Test]
        public void should_convert_string_and_int_to_bytes()
        {
            var converter = new PropertyValueConverter();

            converter.RegisterConverter<string>(MockStringConverter());
            converter.RegisterConverter<int>(MockIntConverter());

            var intConverted = converter.ToBytes(typeof(int), -163563);
            var stringConverted = converter.ToBytes(typeof(string), "sample string 653");

            Assert.Multiple(() =>
            {
                Assert.That(
                    Encoding.UTF8.GetBytes("sample string 653"), 
                    Is.EqualTo(stringConverted));

                Assert.That(
                    BitConverter.GetBytes(-163563),
                    Is.EqualTo(intConverted));
            });
        }

        [Test]
        public void should_throw_on_invalid_data_type()
        {
            var converter = new PropertyValueConverter();

            converter.RegisterConverter<string>(MockStringConverter());

            Assert.Throws<InvalidCastException>(() =>
            {
                converter.ToValue(typeof(int), BitConverter.GetBytes(-163563));
            });
        }

        public enum TestEnum
        {
            One,
            Two,
            Three
        }

        [Test]
        public void enum_test()
        {
            var converter = new PropertyValueConverter();

            converter.RegisterConverter<string>(MockStringConverter());
            converter.RegisterConverter<int>(MockIntConverter());

            var enumBytes = converter.ToBytes(typeof(TestEnum), TestEnum.Two);
            var enumConverted = converter.ToValue(typeof(TestEnum), enumBytes);

            Assert.That((TestEnum) enumConverted, Is.EqualTo(TestEnum.Two));
        }

        private static IValueConverter MockStringConverter()
        {
            var stringConverter = new Mock<IValueConverter>();
            stringConverter
                .Setup(c => c.ToValue(It.IsAny<byte[]>()))
                .Returns<byte[]>(bytes => Encoding.UTF8.GetString(bytes));
            stringConverter
                .Setup(c => c.ToBytes(It.IsAny<string>()))
                .Returns<string>(input => Encoding.UTF8.GetBytes(input));
            return stringConverter.Object;
        }

        private static IValueConverter MockIntConverter()
        {
            var intConverter = new Mock<IValueConverter>();
            intConverter
                .Setup(c => c.ToValue(It.IsAny<byte[]>()))
                .Returns<byte[]>(bytes => BitConverter.ToInt32(bytes));
            intConverter
                .Setup(c => c.ToBytes(It.IsAny<object>()))
                .Returns<object>(input => BitConverter.GetBytes((int)input));
            return intConverter.Object;
        }
    }
}
