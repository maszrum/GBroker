using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using EventBroker.Client;
using EventBroker.Client.EventFlow;
using EventBroker.Client.Interceptor;
using EventBroker.Core;
using Moq;
using NUnit.Framework;

namespace Tests.EventBroker.Client
{
    [TestFixture]
    internal class EventsConsumerTests
    {
        [Test]
        public void start_method_should_be_called_in_every_source()
        {
            var source1 = new Mock<IEventsSource>();
            var source2 = new Mock<IEventsSource>();

            var _ = new EventsConsumer(
                new [] {source1.Object, source2.Object}, 
                Enumerable.Empty<IEventInterceptor>());

            source1.Verify(s => s.Start(), Times.Once);
            source2.Verify(s => s.Start(), Times.Once);
        }

        [Test]
        public void dispose_method_should_be_called_in_every_source()
        {
            var source1 = new Mock<IEventsSource>();
            var source2 = new Mock<IEventsSource>();

            var consumer = new EventsConsumer(
                new[] { source1.Object, source2.Object }, 
                Enumerable.Empty<IEventInterceptor>());

            consumer.Dispose();

            source1.Verify(s => s.Dispose(), Times.Once);
            source2.Verify(s => s.Dispose(), Times.Once);
        }

        public class MockEvent : IEvent
        {
            public string StringProperty { get; set; }
        }

        [Test]
        public void check_if_events_are_merged_from_two_sources()
        {
            var subject1 = new Subject<MockEvent>();
            var subject2 = new Subject<MockEvent>();

            var source1 = new Mock<IEventsSource>();
            source1
                .Setup(s => s.EventsOfType<MockEvent>(ConsumptionType.ConsumeAll))
                .Returns(subject1);

            var source2 = new Mock<IEventsSource>();
            source2
                .Setup(s => s.EventsOfType<MockEvent>(ConsumptionType.ConsumeAll))
                .Returns(subject2);

            var consumer = new EventsConsumer(
                new[] { source1.Object, source2.Object },
                Enumerable.Empty<IEventInterceptor>());

            var receivedEvents = new List<MockEvent>();

            consumer.EventsOfType<MockEvent>(ConsumptionType.ConsumeAll)
                .Subscribe(ev =>
                {
                    receivedEvents.Add(ev);
                });

            var ev1 = new MockEvent();
            var ev2 = new MockEvent();
            var ev3 = new MockEvent();
            var ev4 = new MockEvent();

            subject1.OnNext(ev1);
            subject2.OnNext(ev2);
            subject1.OnNext(ev3);
            subject2.OnNext(ev4);

            CollectionAssert.AreEqual(
                new[] { ev1, ev2, ev3, ev4 },
                receivedEvents);
        }

        [Test]
        public void check_if_received_events_was_intercepted()
        {
            var subject = new Subject<MockEvent>();

            var source = new Mock<IEventsSource>();
            source
                .Setup(s => s.EventsOfType<MockEvent>(ConsumptionType.ConsumeAll))
                .Returns(subject);

            var interceptor1 = new Mock<IEventInterceptor>();
            interceptor1.Setup(i => i.InterceptIncoming(It.IsAny<IEvent>(), It.IsAny<Type>()))
                .Returns<IEvent, Type>((ev, st) =>
                {
                    if (ev is MockEvent me)
                    {
                        me.StringProperty += $" {st.Name}";
                    }
                    return ev;
                });
            var interceptor2 = new Mock<IEventInterceptor>();
            interceptor2.Setup(i => i.InterceptIncoming(It.IsAny<IEvent>(), It.IsAny<Type>()))
                .Returns<IEvent, Type>((ev, st) =>
                {
                    if (ev is MockEvent me)
                    {
                        me.StringProperty += " intercepted2";
                    }
                    return ev;
                });

            var consumer = new EventsConsumer(
                new[] {source.Object},
                new[] {interceptor1.Object, interceptor2.Object});

            var receivedEvents = new List<MockEvent>();

            consumer.EventsOfType<MockEvent>(ConsumptionType.ConsumeAll)
                .Subscribe(ev =>
                {
                    receivedEvents.Add(ev);
                });

            var ev1 = new MockEvent() {StringProperty = "first"};
            var ev2 = new MockEvent() {StringProperty = "second"};
            var ev3 = new MockEvent() {StringProperty = "third"};

            subject.OnNext(ev1);
            subject.OnNext(ev2);
            subject.OnNext(ev3);

            var sourceTypeName = source.Object.GetType().Name;

            Assert.Multiple(() =>
            {
                Assert.That(receivedEvents[0].StringProperty, Is.EqualTo($"first {sourceTypeName} intercepted2"));
                Assert.That(receivedEvents[1].StringProperty, Is.EqualTo($"second {sourceTypeName} intercepted2"));
                Assert.That(receivedEvents[2].StringProperty, Is.EqualTo($"third {sourceTypeName} intercepted2"));
            });
        }
    }
}