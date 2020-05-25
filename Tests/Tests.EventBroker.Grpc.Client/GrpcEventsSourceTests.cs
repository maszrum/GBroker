using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventBroker.Client;
using EventBroker.Client.Exceptions;
using EventBroker.Core;
using EventBroker.Grpc.Client;
using EventBroker.Grpc.Client.Core;
using EventBroker.Grpc.Client.EventToData;
using EventBroker.Grpc.Client.Source;
using EventBroker.Grpc.Data;
using Moq;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc.Client
{
    [TestFixture]
    internal class GrpcEventsSourceTests
    {
        [Test]
        public void check_if_initialize_was_called()
        {
            static async IAsyncEnumerable<IEventData> GetEventData()
            {
                await Task.Delay(1000);
                yield return new EventDataWrapper();
                await Task.Delay(1000);
            }

            var subscribedEvents = new List<string>();

            var sessionInitializer = new Mock<ISessionInitializer>();
            var client = MockClient(subscribedEvents, GetEventData());
            var exceptionsSink = new Mock<IExceptionsSink>();

            var source = new GrpcEventsSource(client, sessionInitializer.Object, exceptionsSink.Object);
            source.Start();

            source.Dispose();

            sessionInitializer.Verify(si => si.Initialize(), Times.Once);
        }

        [Test]
        public async Task check_if_initialize_was_called_on_listener_exception()
        {
            static async IAsyncEnumerable<IEventData> GetEventData()
            {
                await foreach (var eventData in Enumerable.Repeat(new EventDataWrapper(), 10).ToAsyncEnumerable())
                {
                    yield return eventData;
                }
                throw new InvalidOperationException();
            }

            var subscribedEvents = new List<string>();

            var sessionInitializer = new Mock<ISessionInitializer>();
            var client = MockClient(subscribedEvents, GetEventData());
            var exceptionsSink = new Mock<IExceptionsSink>();

            var source = new GrpcEventsSource(client, sessionInitializer.Object, exceptionsSink.Object)
            {
                WaitOnExceptionMilliseconds = 10
            };
            source.Start();

            await Task.Delay(200);

            source.Dispose();

            sessionInitializer.Verify(si => si.Initialize(), Times.AtLeast(2));
        }

        [Test]
        public void received_event_with_invalid_event_data()
        {
            static async IAsyncEnumerable<IEventData> GetEventData()
            {
                await foreach (var eventData in Enumerable.Repeat(new EventDataWrapper(), 10).ToAsyncEnumerable())
                {
                    yield return eventData;
                }
            }

            var subscribedEvents = new List<string>();

            var sessionInitializer = new Mock<ISessionInitializer>();
            var client = MockClient(subscribedEvents, GetEventData());
             
            var thrownExceptions = new List<Exception>();
            var exceptionsSink = new Mock<IExceptionsSink>();
            exceptionsSink.Setup(es => es.NextException(It.IsAny<Exception>()))
                .Callback<Exception>(ex =>
                {
                    thrownExceptions.Add(ex);
                });

            var source = new GrpcEventsSource(client, sessionInitializer.Object, exceptionsSink.Object);
            source.Start();

            while (thrownExceptions.Count == 0)
            {
            }

            source.Dispose();

            Assert.That(thrownExceptions.First(), Is.TypeOf<EventDataProcessingException>());
        }

        [Test]
        public void client_throws_exception_when_listening()
        {
            static async IAsyncEnumerable<IEventData> GetEventData()
            {
                await foreach (var eventData in Enumerable.Repeat(new EventDataWrapper(), 3).ToAsyncEnumerable())
                {
                    yield return eventData;
                }
                throw new InvalidOperationException();
            }

            var subscribedEvents = new List<string>();

            var sessionInitializer = new Mock<ISessionInitializer>();
            var client = MockClient(subscribedEvents, GetEventData());

            var thrownExceptions = new List<Exception>();
            var exceptionsSink = new Mock<IExceptionsSink>();
            exceptionsSink.Setup(es => es.NextException(It.IsAny<Exception>()))
                .Callback<Exception>(ex =>
                {
                    if (ex is InvalidOperationException ioe)
                    {
                        thrownExceptions.Add(ioe);
                    }
                });

            var source = new GrpcEventsSource(client, sessionInitializer.Object, exceptionsSink.Object)
            {
                WaitOnExceptionMilliseconds = 10
            };
            source.Start();

            while (thrownExceptions.Count == 0)
            {
            }

            source.Dispose();

            Assert.That(thrownExceptions.First(), Is.TypeOf<InvalidOperationException>());
        }

        public class FakeEvent : IEvent
        {
            public int IntegerProperty { get; set; }
            public string StringProperty { get; set; }
        }

        [Test]
        public void listen_for_event_and_inject_fake_event()
        {
            EventConverter.RegisterEventsAssembly(typeof(FakeEvent).Assembly);

            var eventTypeName = EventToDataConverter.GetClassName<FakeEvent>();

            static async IAsyncEnumerable<IEventData> GetEventData()
            {
                var events = Enumerable
                    .Range(1, 3)
                    .Select(i =>
                    {
                        var ev = new FakeEvent()
                        {
                            IntegerProperty = i,
                            StringProperty = $"string test {i}"
                        };
                        return EventConverter.EventToData().Convert(ev);
                    });

                foreach (var eventData in events)
                {
                    await Task.Delay(10);
                    yield return eventData;
                }
            }

            var subscribedEvents = new List<string>();

            var sessionInitializer = new Mock<ISessionInitializer>();
            var client = MockClient(subscribedEvents, GetEventData());
            var exceptionsSink = new Mock<IExceptionsSink>();

            var source = new GrpcEventsSource(client, sessionInitializer.Object, exceptionsSink.Object);

            source.Start();

            var receivedEvents = new List<FakeEvent>();
            source.EventsOfType<FakeEvent>(ConsumptionType.ConsumeAll)
                .Subscribe(ev =>
                {
                    receivedEvents.Add(ev);
                });

            while (receivedEvents.Count != 3)
            {
            }

            Assert.Multiple(() =>
            {
                Assert.That(subscribedEvents.All(e => e == eventTypeName), Is.True);

                Assert.That(receivedEvents[0].StringProperty, Is.EqualTo("string test 1"));
                Assert.That(receivedEvents[1].StringProperty, Is.EqualTo("string test 2"));
                Assert.That(receivedEvents[2].StringProperty, Is.EqualTo("string test 3"));

                Assert.That(receivedEvents[0].IntegerProperty, Is.EqualTo(1));
                Assert.That(receivedEvents[1].IntegerProperty, Is.EqualTo(2));
                Assert.That(receivedEvents[2].IntegerProperty, Is.EqualTo(3));
            });
        }

        [Test]
        public async Task check_if_unsubscribed_after_observable_disposal()
        {
            EventConverter.RegisterEventsAssembly(typeof(FakeEvent).Assembly);

            static async IAsyncEnumerable<IEventData> GetEventData()
            {
                var events = Enumerable
                    .Range(1, 4)
                    .Select(i =>
                    {
                        var ev = new FakeEvent()
                        {
                            IntegerProperty = i,
                            StringProperty = $"string test {i}"
                        };
                        return EventConverter.EventToData().Convert(ev);
                    });

                foreach (var eventData in events)
                {
                    await Task.Delay(10);
                    yield return eventData;
                }
            }

            var subscribedEvents = new List<string>();

            var sessionInitializer = new Mock<ISessionInitializer>();
            var client = MockClient(subscribedEvents, GetEventData());
            var exceptionsSink = new Mock<IExceptionsSink>();

            var source = new GrpcEventsSource(client, sessionInitializer.Object, exceptionsSink.Object);

            source.Start();

            var receivedEvents = new List<FakeEvent>();
            var subscription1 = source.EventsOfType<FakeEvent>(ConsumptionType.ConsumeAll)
                .Subscribe(ev =>
                {
                    receivedEvents.Add(ev);
                });
            var subscription2 = source.EventsOfType<FakeEvent>(ConsumptionType.ConsumeAll)
                .Subscribe(ev =>
                {
                    receivedEvents.Add(ev);
                });

            while (receivedEvents.Count != 4)
            {
            }

            subscription1.Dispose();
            subscription2.Dispose();

            await Task.Delay(100);

            Assert.That(subscribedEvents, Is.Empty);
        }

        private static IGrpcClient MockClient(ICollection<string> subscriptions, IAsyncEnumerable<IEventData> eventsData)
        {
            var mock = new Mock<IGrpcClient>();

            mock
                .Setup(m => m.SubscribeMany(It.IsAny<IEnumerable<(string, ConsumptionType)>>()))
                .Callback<IEnumerable<(string, ConsumptionType)>>(tuples =>
                {
                    foreach (var t in tuples)
                    {
                        if (!subscriptions.Contains(t.Item1))
                        {
                            subscriptions.Add(t.Item1);
                        }
                    }
                });

            mock
                .Setup(m => m.Subscribe(It.IsAny<string>(), It.IsAny<ConsumptionType>()))
                .Callback<string, ConsumptionType>((en, ct) =>
                {
                    if (!subscriptions.Contains(en))
                    { 
                        subscriptions.Add(en);
                    }
                });

            mock
                .Setup(m => m.Unsubscribe(It.IsAny<string>()))
                .Callback<string>(en =>
                {
                    if (subscriptions.Contains(en))
                    {
                        subscriptions.Remove(en);
                    }
                });

            mock
                .Setup(m => m.Listen(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>(token =>
                {
                    if (token.IsCancellationRequested)
                    {
                        throw new Exception("cancelled");
                    }
                })
                .Returns(() => eventsData);

            return mock.Object;
        }
    }
}