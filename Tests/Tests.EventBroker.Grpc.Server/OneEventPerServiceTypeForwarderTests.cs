using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventBroker.Grpc.Data;
using EventBroker.Grpc.Server.EventsForwarding;
using EventBroker.Grpc.Server.Sessions;
using Moq;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc.Server
{
    [TestFixture]
    internal class OneEventPerServiceTypeForwarderTests
    {
        [Test]
        public void all_sessions_should_be_fed()
        {
            var sessions = Enumerable
                .Repeat(1, 3)
                .Select(_ => MockSession("ServiceOne"))
                .ToArray();

            var forwarder = new OneEventPerServiceTypeForwarder();

            foreach (var _ in sessions)
            {
                forwarder.Send(sessions.Select(s => s.Object), new EventDataWrapper(), Array.Empty<string>());
            }

            foreach (var session in sessions)
            {
                session.Verify(s => s.FeedData(It.IsAny<IEventData>()), Times.Once);
            }
        }

        [Test]
        public void check_feeding_in_parallel()
        {
            var callCounter = new Dictionary<int, int>(
                Enumerable.Range(1, 10).Select(i => new KeyValuePair<int, int>(i, 0)));

            var sessions = Enumerable
                .Range(1, 10)
                .Select(i =>
                {
                    var mock = i <= 5 ? MockSession("ServiceOne") : MockSession("ServiceTwo");
                    mock.Setup(m => m.FeedData(It.IsAny<IEventData>()))
                        .Callback(() => callCounter[i]++);
                    return mock;
                })
                .ToArray();

            var forwarder = new OneEventPerServiceTypeForwarder();

            var eventData = Enumerable
                .Repeat(1, 100)
                .Select(_ => new EventDataWrapper())
                .ToArray();

            var options = new ParallelOptions() { MaxDegreeOfParallelism = 5 };
            var result = Parallel.ForEach(eventData, options, ed =>
            {
                forwarder.Send(sessions.Select(s => s.Object), ed, Array.Empty<string>());
            });

            while (!result.IsCompleted) { }

            Assert.Multiple(() =>
            {
                var callSum = callCounter.Values.Sum();
                Assert.That(callSum, Is.EqualTo(200));

                foreach (var count in callCounter.Values)
                {
                    Assert.AreEqual(20, count, 1);
                }
            });
        }

        [Test]
        public void feed_three_sessions_and_then_feed_three_another_sessions()
        {
            var callCounter = new Dictionary<int, int>(
                Enumerable.Range(1, 6).Select(i => new KeyValuePair<int, int>(i, 0)));

            var sessions = Enumerable
                .Range(1, 6)
                .Select(i =>
                {
                    var mock = MockSession("ServiceOne");
                    mock.Setup(m => m.FeedData(It.IsAny<IEventData>()))
                        .Callback(() => callCounter[i]++);
                    return mock;
                })
                .ToArray();

            var forwarder = new OneEventPerServiceTypeForwarder();

            var eventData = Enumerable
                .Repeat(1, 12)
                .Select(_ => new EventDataWrapper())
                .ToArray();

            foreach (var ed in eventData.Take(6))
            {
                forwarder.Send(sessions.Select(s => s.Object).Take(3), ed, Array.Empty<string>());
            }

            foreach (var ed in eventData.Skip(6))
            {
                forwarder.Send(sessions.Select(s => s.Object), ed, Array.Empty<string>());
            }

            Assert.Multiple(() =>
            {
                Assert.That(
                    callCounter.Values.Take(3).All(c => c == 3),
                    Is.True);
                Assert.That(
                    callCounter.Values.Skip(3).All(c => c == 1),
                    Is.True);
            });
        }

        private static Mock<ISession> MockSession(string serviceType)
        {
            var session = new Mock<ISession>();
            var id = Guid.NewGuid();
            session.SetupGet(s => s.Id).Returns(id);
            session.SetupGet(s => s.ServiceType).Returns(serviceType);

            return session;
        }
    }
}
