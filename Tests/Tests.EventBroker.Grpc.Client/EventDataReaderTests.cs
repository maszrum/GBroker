using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EventBroker.Core;
using EventBroker.Grpc.Client.DataToEvent;
using EventBroker.Grpc.Data;
using Moq;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc.Client
{
    internal class EventDataReaderTests
    {
        public class StubEvent : IEvent
        {
            public int IntFirst { get; set; }
            public int IntSecond { get; set; }
            public bool Bool { get; set; }
            public string String { get; set; }
        }

        [Test]
        public void read_several_properties_from_event_data()
        {
            var firstIntBytes = BitConverter.GetBytes(120);
            var secondIntBytes = BitConverter.GetBytes(555000);
            var boolBytes = BitConverter.GetBytes(true);
            var stringBytes = Encoding.UTF8.GetBytes("this is example string");
            var concatenatedBytes = firstIntBytes
                .Concat(secondIntBytes)
                .Concat(boolBytes)
                .Concat(stringBytes)
                .ToArray();

            var eventData = new Mock<IEventData>();
            eventData.SetupGet(e => e.EventName).Returns("nevermind");
            eventData.SetupGet(e => e.PropertyNames).Returns(new List<string> { "IntFirst", "IntSecond", "Bool", "String" });
            eventData.SetupGet(e => e.PropertyPositions).Returns(new List<int> { 0, 4, 8, 9 });
            eventData.Setup(e => e.GetData()).Returns(concatenatedBytes);

            var reader = new EventDataReader(eventData.Object, ResolveValue);
            var result = reader.ToArray();

            Assert.Multiple(() =>
            {
                Assert.That(result.Length, Is.EqualTo(4));

                Assert.That(result[0].Property.Name, Is.EqualTo("IntFirst"));
                Assert.That(result[1].Property.Name, Is.EqualTo("IntSecond"));
                Assert.That(result[2].Property.Name, Is.EqualTo("Bool"));
                Assert.That(result[3].Property.Name, Is.EqualTo("String"));

                Assert.That(result[0].Value, Is.EqualTo(120));
                Assert.That(result[1].Value, Is.EqualTo(555000));
                Assert.That(result[2].Value, Is.EqualTo(true));
                Assert.That(result[3].Value, Is.EqualTo("this is example string"));
            });
        }

        public class EmptyEvent : IEvent
        {
        }

        [Test]
        public void read_event_data_with_no_properties()
        {
            var eventData = new Mock<IEventData>();
            eventData.SetupGet(e => e.EventName).Returns("nevermind");
            eventData.SetupGet(e => e.PropertyNames).Returns(new List<string>());
            eventData.SetupGet(e => e.PropertyPositions).Returns(new List<int>());
            eventData.Setup(e => e.GetData()).Returns(Array.Empty<byte>());

            var reader = new EventDataReader(eventData.Object, ResolveValue);
            var result = reader.ToArray();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void should_throw_on_invalid_last_property_position()
        {
            var eventData = new Mock<IEventData>();
            eventData.SetupGet(e => e.EventName).Returns("nevermind");
            eventData.SetupGet(e => e.PropertyNames).Returns(new List<string> {"Prop1", "Prop2"});
            eventData.SetupGet(e => e.PropertyPositions).Returns(new List<int> {0, 5});
            eventData.Setup(e => e.GetData()).Returns(new byte[] {0x00, 0x01, 0x02, 0x03, 0x04 });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new EventDataReader(eventData.Object, ResolveValue);
            });
        }

        [Test]
        public void should_throw_on_invalid_names_or_positions_count()
        {
            var eventData = new Mock<IEventData>();
            eventData.SetupGet(e => e.EventName).Returns("nevermind");
            eventData.SetupGet(e => e.PropertyNames).Returns(new List<string> { "Prop1", "Prop2", "Prop3" });
            eventData.SetupGet(e => e.PropertyPositions).Returns(new List<int> { 0, 3 });
            eventData.Setup(e => e.GetData()).Returns(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 });

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new EventDataReader(eventData.Object, ResolveValue);
            });
        }

        private static (PropertyInfo, object) ResolveValue(string propertyName, byte[] bytes)
        {
            return propertyName switch
            {
                "IntFirst" => (
                    typeof(StubEvent).GetProperty("IntFirst"), 
                    BitConverter.ToInt32(bytes)),
                "IntSecond" => (
                    typeof(StubEvent).GetProperty("IntSecond"), 
                    BitConverter.ToInt32(bytes)),
                "Bool" => (
                    typeof(StubEvent).GetProperty("Bool"), 
                    BitConverter.ToBoolean(bytes)),
                "String" => (
                    typeof(StubEvent).GetProperty("String"), 
                    Encoding.UTF8.GetString(bytes)),
                _ => (null, null)
            };
        }
    }
}
