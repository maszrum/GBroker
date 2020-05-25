using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventBroker.Client;
using EventBroker.Client.EventFlow;
using EventBroker.Client.Interceptor;
using EventBroker.Core;
using Moq;
using NUnit.Framework;

namespace Tests.EventBroker.Client
{
    [TestFixture]
    internal class EventsProducerTests
    {
        [Test]
        public void check_if_event_was_sent_to_all_sinks()
        {
            var sink1 = new Mock<IEventsSink>();
            var sink2 = new Mock<IEventsSink>();

            var producer = new EventsProducer(
                new[] {sink1.Object, sink2.Object},
                Enumerable.Empty<IEventInterceptor>());

            producer.Publish(new Mock<IEvent>().Object);

            Assert.Multiple(() =>
            {
                sink1.Verify(s => s.SendEvent(It.IsAny<IPublishingState<IEvent>>()), Times.Once);
                sink2.Verify(s => s.SendEvent(It.IsAny<IPublishingState<IEvent>>()), Times.Once);
            });
        }

        [Test]
        public async Task check_if_event_was_sent_async_to_all_sinks()
        {
            var sink1 = new Mock<IEventsSink>();
            var sink2 = new Mock<IEventsSink>();

            var producer = new EventsProducer(
                new[] { sink1.Object, sink2.Object },
                Enumerable.Empty<IEventInterceptor>());

            await producer.PublishAsync(new Mock<IEvent>().Object);

            Assert.Multiple(() =>
            {
                sink1.Verify(s => s.SendEventAsync(It.IsAny<IPublishingState<IEvent>>()), Times.Once);
                sink2.Verify(s => s.SendEventAsync(It.IsAny<IPublishingState<IEvent>>()), Times.Once);
            });
        }

        public class MockEvent : IEvent
        {
            public string StringProperty { get; set; }
        }

        [Test]
        public void check_if_published_event_was_intercepted()
        {
            var sentEvents = new List<MockEvent>();

            var sink = new Mock<IEventsSink>();
            sink.Setup(s => s.SendEvent(It.IsAny<IPublishingState<MockEvent>>()))
                .Callback<IPublishingState<MockEvent>>(state =>
                {
                    sentEvents.Add(state.Event);
                });

            var interceptor1 = MockInterceptor(" intercepted1", false);
            var interceptor2 = MockInterceptor(" intercepted2", false);


            var producer = new EventsProducer(
                new[] {sink.Object},
                new[] {interceptor1, interceptor2});

            producer.Publish(
                new MockEvent() {StringProperty = "event"});

            Assert.That(sentEvents[0].StringProperty, Is.EqualTo("event intercepted1 intercepted2"));
        }

        [Test]
        public async Task check_if_published_event_was_intercepted_async()
        {
            var sentEvents = new List<MockEvent>();

            var sink = new Mock<IEventsSink>();
            sink.Setup(s => s.SendEventAsync(It.IsAny<IPublishingState<MockEvent>>()))
                .Callback<IPublishingState<MockEvent>>(state =>
                {
                    sentEvents.Add(state.Event);
                });

            var interceptor1 = MockInterceptor(" intercepted1", true);
            var interceptor2 = MockInterceptor(" intercepted2", true);


            var producer = new EventsProducer(
                new[] { sink.Object },
                new[] { interceptor1, interceptor2 });

            await producer.PublishAsync(
                new MockEvent() { StringProperty = "event" });

            Assert.That(sentEvents[0].StringProperty, Is.EqualTo("event intercepted1 intercepted2"));
        }

        private static IEventInterceptor MockInterceptor(string addValue, bool async)
        {
            var interceptor = new Mock<IEventInterceptor>();
            if (!async)
            {
                interceptor.Setup(i => i.InterceptOutgoing(It.IsAny<MockEvent>()))
                    .Returns<MockEvent>(ev =>
                    {
                        ev.StringProperty += addValue;
                        return ev;
                    });
            }
            else
            {
                interceptor.Setup(i => i.InterceptOutgoingAsync(It.IsAny<MockEvent>()))
                    .Returns<MockEvent>(ev =>
                    {
                        ev.StringProperty += addValue;
                        return Task.FromResult(ev);
                    });
            }
            return interceptor.Object;
        }
    }
}