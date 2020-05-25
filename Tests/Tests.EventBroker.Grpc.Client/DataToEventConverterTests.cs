using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventBroker.Core;
using EventBroker.Grpc.Client.DataToEvent;
using EventBroker.Grpc.Data;
using EventBroker.Grpc.ValueConverters;
using Moq;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc.Client
{
    [TestFixture]
    internal class DataToEventConverterTests
    {
        public class FirstEvent : IEvent
        {
            public int IntProperty { get; set; }
            public string StringProperty { get; set; }
            public int NotUsedProperty { get; set; }
        }

        [Test]
        public void convert_event_data_to_event_object()
        {
            var bytes = BitConverter.GetBytes(1527468)
                .Concat(new byte[] {0, 0, 0})
                .Concat(Encoding.UTF8.GetBytes("sample string 123"))
                .ToArray();

            var eventData = new Mock<IEventData>();
            eventData.SetupGet(e => e.EventName).Returns("FirstEvent");
            eventData.SetupGet(e => e.PropertyNames).Returns(new List<string> { "IntProperty", "NotExistsProperty", "StringProperty" });
            eventData.SetupGet(e => e.PropertyPositions).Returns(new List<int> { 0, 4, 7 });
            eventData.Setup(e => e.GetData()).Returns(bytes);

            var parametersConverter = MockParametersConverter();
            var eventTypeResolver = MockEventTypeResolver();

            var converter = new DataToEventConverter(parametersConverter, eventTypeResolver);

            var ev = converter.Convert(eventData.Object);

            Assert.That(ev, Is.TypeOf<FirstEvent>());

            var eventTyped = (FirstEvent)ev;

            Assert.Multiple(() =>
            {
                Assert.That(eventTyped.IntProperty, Is.EqualTo(1527468));
                Assert.That(eventTyped.StringProperty, Is.EqualTo("sample string 123"));
            });
        }

        private static IPropertyValueConverter MockParametersConverter()
        {
            var parametersConverter = new Mock<IPropertyValueConverter>();
            parametersConverter
                .Setup(pc => pc.ToValue(It.IsAny<Type>(), It.IsAny<byte[]>()))
                .Returns<Type, byte[]>((t, b) =>
                {
                    if (t == typeof(int))
                    {
                        return BitConverter.ToInt32(b);
                    }
                    if (t == typeof(string))
                    {
                        return Encoding.UTF8.GetString(b);
                    }
                    return null;
                });

            return parametersConverter.Object;
        }

        private static IEventTypeResolver MockEventTypeResolver()
        {
            var eventTypeResolver = new Mock<IEventTypeResolver>();
            eventTypeResolver
                .Setup(etr => etr.GetEventConstructor(It.IsAny<Type>()))
                .Returns<Type>(t => t.GetConstructor(Type.EmptyTypes));
            eventTypeResolver
                .Setup(etr => etr.GetEventType(It.Is<string>(t => t == "FirstEvent")))
                .Returns<string>(t => typeof(FirstEvent));
            eventTypeResolver
                .Setup(etr => etr.GetPropertyInfo(It.IsAny<Type>(), It.IsAny<string>()))
                .Returns<Type, string>((t, p) => t.GetProperty(p));

            return eventTypeResolver.Object;
        }
    }
}
