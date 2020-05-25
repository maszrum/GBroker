using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EventBroker.Client;
using EventBroker.Core;
using EventBroker.Grpc.Client.Core;
using EventBroker.Grpc.Client.DataToEvent;
using EventBroker.Grpc.Client.EventToData;
using EventBroker.Grpc.Client.Source;
using EventBroker.Grpc.ValueConverters;
using Moq;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc.Client
{
    internal class EventsSubscriberTests
    {
        [Test]
        public void check_if_subscribe_method_calls_subscribe_in_client()
        {
            var client = new Mock<IGrpcClient>();

            var subscriber = new EventsSubscriber(client.Object);

            subscriber.Subscribe("test1", ConsumptionType.ConsumeAll);
            subscriber.Subscribe("test2", ConsumptionType.ConsumeAll);
            subscriber.Subscribe("test3", ConsumptionType.ConsumeAll);

            client.Verify(
                c => c.Subscribe(It.IsAny<string>(), It.IsAny<ConsumptionType>()), 
                Times.Exactly(3));
        }

        [Test]
        public void check_if_unsubscribe_method_calls_unsubscribe_in_client()
        {
            var client = new Mock<IGrpcClient>();

            var subscriber = new EventsSubscriber(client.Object);

            subscriber.Subscribe("test1", ConsumptionType.ConsumeAll);
            subscriber.Subscribe("test2", ConsumptionType.ConsumeAll);

            subscriber.Unsubscribe("test1");
            subscriber.Unsubscribe("test2");
            subscriber.Unsubscribe("test3");

            client.Verify(
                c => c.Unsubscribe(It.IsAny<string>()),
                Times.Exactly(2));
        }

        [Test]
        public void refresh_subscriptions()
        {
            var subscribedEvents = new HashSet<string>();

            var client = new Mock<IGrpcClient>();
            client
                .Setup(c => c.Subscribe(It.IsAny<string>(), It.IsAny<ConsumptionType>()))
                .Callback<string, ConsumptionType>((en, ct) =>
                {
                    subscribedEvents.Add(en);
                    throw new Exception("test exception");
                });
            client
                .Setup(c => c.Unsubscribe(It.IsAny<string>()))
                .Callback<string>(en =>
                {
                    subscribedEvents.Remove(en);
                    throw new Exception("test exception");
                });
            client.Setup(c => c.SubscribeMany(It.IsAny<IEnumerable<(string, ConsumptionType)>>()))
                .Callback<IEnumerable<(string, ConsumptionType)>>(en =>
                {
                    foreach (var e in en)
                    {
                        subscribedEvents.Add(e.Item1);
                    }
                });

            var subscriber = new EventsSubscriber(client.Object);

            subscriber.Subscribe("test1", ConsumptionType.ConsumeAll);
            subscriber.Subscribe("test2", ConsumptionType.ConsumeAll);
            subscriber.Subscribe("test3", ConsumptionType.ConsumeAll);

            subscriber.Unsubscribe("test2");

            subscriber.RefreshSubscriptions();

            CollectionAssert.AreEqual(
                new[] {"test1", "test3"}, 
                subscribedEvents);

            client.Verify(
                c => c.SubscribeMany(It.Is<IEnumerable<(string, ConsumptionType)>>(
                    en => en.Count() == 2)), Times.Once);
        }
    }

    internal class EventToDataConverterTests
    {
        public class MockEvent : IEvent
        {
            public int IntProperty { get; set; }
            public string StringProperty { get; set; }
        }

        [Test]
        public void get_class_name_test()
        {
            var eventType = typeof(MockEvent);
            var returnedClassName = EventToDataConverter.GetClassName(eventType);

            Assert.That(
                returnedClassName, 
                Is.EqualTo(eventType.FullName));
        }

        [Test]
        public void convert_mock_event()
        {
            var valueConverter = MockPropertyValueConverter();

            var converter = new EventToDataConverter(
                valueConverter, 
                MockEventTypeResolver());

            var ev = new MockEvent()
            {
                IntProperty = -142,
                StringProperty = "anystring"
            };

            var eventData = converter.Convert(ev);

            Assert.Multiple(() =>
            {
                Assert.That(
                    eventData.EventName, 
                    Is.EqualTo(typeof(MockEvent).FullName));

                CollectionAssert.AreEqual(
                    new[] {"IntProperty", "StringProperty"},
                    eventData.PropertyNames);

                CollectionAssert.AreEqual(
                    new[] {0, 4},
                    eventData.PropertyPositions);

                var expectedBytes = valueConverter.ToBytes(typeof(int), -142)
                    .Concat(valueConverter.ToBytes(typeof(string), "anystring"))
                    .ToArray();

                CollectionAssert.AreEqual(
                    expectedBytes, eventData.GetData());
            });
        }

        public class PropertylessEvent : IEvent
        {
        }

        [Test]
        public void convert_propertyless_event()
        {
            var converter = new EventToDataConverter(
                MockPropertyValueConverter(),
                MockEventTypeResolver());

            var ev = new PropertylessEvent();

            var eventData = converter.Convert(ev);

            Assert.Multiple(() =>
            {
                Assert.That(
                    eventData.EventName,
                    Is.EqualTo(typeof(PropertylessEvent).FullName));

                Assert.That(eventData.PropertyNames, Is.Empty);

                Assert.That(eventData.PropertyPositions, Is.Empty);

                Assert.That(eventData.GetData(), Is.Empty);
            });
        }

        private static IPropertyValueConverter MockPropertyValueConverter()
        {
            var mock = new Mock<IPropertyValueConverter>();

            mock.Setup(m => m.ToBytes(It.Is<Type>(t => t == typeof(int)), It.IsAny<int>()))
                .Returns<Type, int>(
                    (t, v) => BitConverter.GetBytes(v));

            mock.Setup(m => m.ToBytes(It.Is<Type>(t => t == typeof(string)), It.IsAny<string>()))
                .Returns<Type, string>(
                    (t, v) => Encoding.UTF8.GetBytes(v));

            return mock.Object;
        }

        private static IEventTypeResolver MockEventTypeResolver()
        {
            var mock = new Mock<IEventTypeResolver>();

            mock.Setup(m => m.GetProperties(It.Is<Type>(t => t == typeof(MockEvent))))
                .Returns(new[]
                {
                    typeof(MockEvent).GetProperty("IntProperty"),
                    typeof(MockEvent).GetProperty("StringProperty")
                });

            mock.Setup(m => m.GetProperties(It.Is<Type>(t => t == typeof(PropertylessEvent))))
                .Returns(Array.Empty<PropertyInfo>());

            return mock.Object;
        }
    }
}
