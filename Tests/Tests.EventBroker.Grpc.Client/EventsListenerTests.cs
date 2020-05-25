using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventBroker.Grpc.Client.Core;
using EventBroker.Grpc.Client.Source;
using EventBroker.Grpc.Data;
using Moq;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc.Client
{
    [TestFixture]
    internal class EventsListenerTests
    {
        [Test]
        [Timeout(1000)]
        public void wait_for_three_events_test()
        {
            var client = MockClient(GetAsyncEnumerable(10));

            var listener = new EventsListener(client, () => {});

            var resetEvent = new ManualResetEvent(false);
            var receivedData = new List<IEventData>();

            listener.Start(
                data =>
                {
                    receivedData.Add(data);
                    if (receivedData.Count == 3)
                    {
                        resetEvent.Set();
                    }
                },
                exception => { });

            resetEvent.WaitOne();

            listener.Dispose();
        }

        [Test]
        [Timeout(1000)]
        public void initialize_action_throws_exception_test()
        {
            var client = MockClient(GetAsyncEnumerable(3));

            var initializeCount = 0;

            var listener = new EventsListener(client, () =>
            {
                initializeCount++;
                if (initializeCount == 2)
                {
                    throw new AccessViolationException();
                }
            });

            var resetEvent = new ManualResetEvent(false);
            Exception exceptionThrown = null;

            listener.Start(
                data => { },
                exception =>
                {
                    exceptionThrown = exception;
                    resetEvent.Set();
                });

            resetEvent.WaitOne();

            Assert.That(exceptionThrown, Is.TypeOf(typeof(AccessViolationException)));

            listener.Dispose();
        }

        [Test]
        [Timeout(1000)]
        public void async_enumerable_throws_exception_test()
        {
            var client = MockClient(GetAsyncEnumerable(3, new AccessViolationException()));

            var listener = new EventsListener(client, () => { });

            var resetEvent = new ManualResetEvent(false);
            Exception exceptionThrown = null;

            listener.Start(
                data => { },
                exception =>
                {
                    exceptionThrown = exception;
                    resetEvent.Set();
                });

            resetEvent.WaitOne();

            Assert.That(exceptionThrown, Is.TypeOf(typeof(AccessViolationException)));

            listener.Dispose();
        }

        [Test]
        [Timeout(1000)]
        public void on_fail_action_throws_exception_test()
        {
            var client = MockClient(GetAsyncEnumerable(3, new AccessViolationException()));

            var listener = new EventsListener(client, () => { });

            var resetEvent = new ManualResetEvent(false);
            Exception exceptionThrown = null;

            listener.Start(
                data => { },
                exception =>
                {
                    if (exception is AccessViolationException)
                    {
                        throw new ArgumentOutOfRangeException();
                    }

                    exceptionThrown = exception;
                    resetEvent.Set();
                });

            resetEvent.WaitOne();

            Assert.That(exceptionThrown, Is.TypeOf(typeof(ArgumentOutOfRangeException)));

            listener.Dispose();
        }

        [Test]
        public async Task check_if_initialization_will_stop_after_disposal()
        {
            var client = MockClient(null);

            var initializedCount = 0;
            void InitializeAction()
            {
                initializedCount++;
            }

            var listener = new EventsListener(client, InitializeAction)
            {
                WaitOnExceptionMilliseconds = 1
            };

            listener.Start(_ => { }, _ => { });

            await Task.Delay(100);
            var initializedBefore = initializedCount;

            listener.Dispose();
            await Task.Delay(200);
            var initializedAfter = initializedCount;

            Assert.AreEqual(initializedAfter, initializedBefore, 3);
        }

        [Test]
        public void should_throw_on_double_start()
        {
            var client = MockClient(null);

            var listener = new EventsListener(client, () => {});

            listener.Start(_ => { }, _ => { });

            Assert.Throws<InvalidOperationException>(() =>
            {
                listener.Start(_ => {}, _ => {});
            });

            listener.Dispose();
        }

        private static IEventData MockEventData()
        {
            var mock = new Mock<IEventData>();

            mock.SetupGet(m => m.EventName).Returns("EventName");
            mock.SetupGet(m => m.PropertyNames).Returns(new List<string>());
            mock.SetupGet(m => m.PropertyPositions).Returns(new List<int>());
            mock.Setup(m => m.GetData()).Returns(Array.Empty<byte>());

            return mock.Object;
        }

        private static IGrpcClient MockClient(
            IAsyncEnumerable<IEventData> output, Action callback = default)
        {
            var mock = new Mock<IGrpcClient>();

            mock.Setup(m => m.Listen(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>(token =>
                {
                    callback?.Invoke();

                    if (token.IsCancellationRequested)
                    {
                        throw new Exception("cancelled");
                    }
                })
                .Returns(() => output);

            return mock.Object;
        }

        private static async IAsyncEnumerable<IEventData> GetAsyncEnumerable(
            int count, Exception exceptionToThrow = default)
        {
            for (var i = 0; i < count; i++)
            {
                yield return MockEventData();
                await Task.Delay(5);
            }

            if (exceptionToThrow != null)
            {
                throw exceptionToThrow;
            }
        }
    }
}
