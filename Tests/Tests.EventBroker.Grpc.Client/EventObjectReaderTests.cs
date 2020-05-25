using System;
using System.Linq;
using System.Text;
using EventBroker.Core;
using EventBroker.Grpc.Client.EventToData;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc.Client
{
    internal class EventObjectReaderTests
    {
        public class MockEvent : IEvent
        {
            public int IntProperty { get; set; }
            public string StringProperty { get; set; }
        }

        [Test]
        public void check_if_returned_positions_are_correct()
        {
            var ev = new MockEvent()
            {
                IntProperty = 14,
                StringProperty = "anystring"
            };

            var properties = new[]
            {
                typeof(MockEvent).GetProperty("IntProperty"),
                typeof(MockEvent).GetProperty("StringProperty")
            };

            var reader = new EventObjectReader(ev, properties, ConvertToByte);

            var propertiesData = reader.ToArray();

            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(
                    new[] {0, 4},
                    propertiesData.Select(pd => pd.Position));
            });
        }

        [Test]
        public void check_if_returned_bytes_are_correct()
        {
            var ev = new MockEvent()
            {
                IntProperty = 14,
                StringProperty = "anystring"
            };

            var properties = new[]
            {
                typeof(MockEvent).GetProperty("IntProperty"),
                typeof(MockEvent).GetProperty("StringProperty")
            };

            var reader = new EventObjectReader(ev, properties, ConvertToByte);

            var propertiesData = reader.ToArray();

            var expectedIntBytes = ConvertToByte(14, typeof(int));
            var expectedStringBytes = ConvertToByte("anystring", typeof(string));

            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(
                    new[] { expectedIntBytes, expectedStringBytes },
                    propertiesData.Select(pd => pd.Bytes));
            });
        }

        [Test]
        public void check_if_returned_property_names_are_correct()
        {
            var ev = new MockEvent()
            {
                IntProperty = 14,
                StringProperty = "anystring"
            };

            var properties = new[]
            {
                typeof(MockEvent).GetProperty("IntProperty"),
                typeof(MockEvent).GetProperty("StringProperty")
            };

            var reader = new EventObjectReader(ev, properties, ConvertToByte);

            var propertiesData = reader.ToArray();

            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(
                    new[] { "IntProperty", "StringProperty" },
                    propertiesData.Select(pd => pd.PropertyName));
            });
        }

        private static byte[] ConvertToByte(object propertyValue, Type propertyType)
        {
            if (propertyType == typeof(int))
            {
                return BitConverter.GetBytes((int) propertyValue);
            }

            if (propertyType == typeof(string))
            {
                return Encoding.UTF8.GetBytes((string) propertyValue);
            }

            throw new Exception(
                $"invalid type to convert: {propertyType.Name}");
        }
    }
}