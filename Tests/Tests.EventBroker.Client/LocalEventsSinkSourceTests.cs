using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using EventBroker.Client.EventFlow;
using EventBroker.Client.Local;
using EventBroker.Core;
using NUnit.Framework;

namespace Tests.EventBroker.Client
{
    [TestFixture]
    internal class LocalEventsSinkSourceTests
    {
        public class StubFirstEvent : IEvent { }
        public class StubSecondEvent : IEvent
        {
            public int AnyValue { get; set; }
        }

        [Test]
        public void first_event_should_be_sent_and_received()
        {
            var eventsSinkSource = new LocalEventsSinkSource("SampleService");

            IEvent receivedEvent = null;
            var counter = 0;

            var subscription = eventsSinkSource
                .EventsOfType<StubFirstEvent>(ConsumptionType.ConsumeAll)
                .Subscribe(e =>
                {
                    receivedEvent = e;
                    counter++;
                });

            var firstState = new PublishingState<StubFirstEvent>(new StubFirstEvent());
            var secondState = new PublishingState<StubSecondEvent>(new StubSecondEvent());

            eventsSinkSource.SendEvent(firstState);
            eventsSinkSource.SendEvent(secondState);

            Assert.Multiple(() =>
            {
                Assert.That(receivedEvent, Is.SameAs(firstState.Event));
                Assert.That(counter, Is.EqualTo(1));
            });

            subscription.Dispose();
        }

        [Test]
        public void first_and_second_event_should_be_sent_and_received()
        {
            var eventsSinkSource = new LocalEventsSinkSource("SampleService");

            IEvent firstReceivedEvent = null;
            var counter = 0;

            var firstSubscription = eventsSinkSource
                .EventsOfType<StubFirstEvent>(ConsumptionType.ConsumeAll)
                .Subscribe(e =>
                {
                    firstReceivedEvent = e;
                    counter++;
                });

            IEvent secondReceivedEvent = null;

            var secondSubscription = eventsSinkSource
                .EventsOfType<StubSecondEvent>(ConsumptionType.ConsumeAll)
                .Subscribe(e =>
                {
                    secondReceivedEvent = e;
                    counter++;
                });

            var firstState = new PublishingState<StubFirstEvent>(new StubFirstEvent());
            var secondState = new PublishingState<StubSecondEvent>(new StubSecondEvent());

            eventsSinkSource.SendEvent(firstState);
            eventsSinkSource.SendEvent(secondState);

            Assert.Multiple(() =>
            {
                Assert.That(firstReceivedEvent, Is.SameAs(firstState.Event));
                Assert.That(secondReceivedEvent, Is.SameAs(secondState.Event));
                Assert.That(counter, Is.EqualTo(2));
            });

            firstSubscription.Dispose();
            secondSubscription.Dispose();
        }

        [Test]
        public void events_with_property_less_than_2_should_be_received()
        {
            var eventsSinkSource = new LocalEventsSinkSource("SampleService");

            var events = Enumerable
                .Range(-2, 6)
                .Select(i => new StubSecondEvent() { AnyValue = i });

            var receivedNumbers = new List<int>();

            var subscription = eventsSinkSource
                .EventsOfType<StubSecondEvent>(ConsumptionType.ConsumeAll)
                .Where(e => e.AnyValue < 2)
                .Subscribe(e =>
                {
                    receivedNumbers.Add(e.AnyValue);
                });

            foreach (var ev in events)
            {
                var state = new PublishingState<StubSecondEvent>(ev);

                eventsSinkSource.SendEvent(state);
            }

            var expectedNumbers = new[] { -2, -1, 0, 1 };
            Assert.That(receivedNumbers, Is.EquivalentTo(expectedNumbers));

            subscription.Dispose();
        }
    }
}
