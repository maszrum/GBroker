using System;
using System.Collections.Generic;
using System.Linq;
using EventBroker.Core;
using EventBroker.Grpc.Data;
using EventBroker.Grpc.Server.EventsForwarding;
using EventBroker.Grpc.Server.Sessions;
using Moq;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc.Server
{
    [TestFixture]
    internal class EventsForwarderTests
    {
        [Test]
        public void session_with_subscribed_event_should_be_fed()
        {
            var specificForwarder = new Mock<ISpecificForwarder>();
            var sessionsThatReceivedEvent = new List<Guid>();
            specificForwarder.Setup(sf => sf.Send(It.IsAny<IEnumerable<ISession>>(), It.IsAny<IEventData>(), It.IsAny<IReadOnlyList<string>>()))
                .Callback<IEnumerable<ISession>, IEventData, IReadOnlyList<string>>((s, ed, sh) =>
                {
                    sessionsThatReceivedEvent.AddRange(s.Select(i => i.Id));
                });

            var forwarder = new EventsForwarder();
            forwarder.RegisterForwarder(ConsumptionType.OneEventPerServiceType, specificForwarder.Object);

            var sessions = new[]
            {
                CreateSession(
                    ("EventOne", ConsumptionType.OneEventPerServiceType),
                    ("EventTwo", ConsumptionType.OneEventPerServiceType)),
                CreateSession(
                    ("EventOne", ConsumptionType.OneEventPerServiceType))
            };

            forwarder.Send(sessions, CreateEventData("EventTwo"), Enumerable.Empty<string>());

            Assert.Multiple(() =>
            {
                Assert.That(sessionsThatReceivedEvent, Has.Count.EqualTo(1));
                Assert.That(sessionsThatReceivedEvent[0], Is.EqualTo(sessions[0].Id));
            });
        }

        [Test]
        public void forwarding_test()
        {
            var firstSpecificForwarder = new Mock<ISpecificForwarder>();
            var sentByFirst = new List<Guid>();
            firstSpecificForwarder.Setup(sf => sf.Send(It.IsAny<IEnumerable<ISession>>(), It.IsAny<IEventData>(), It.IsAny<IReadOnlyList<string>>()))
                .Callback<IEnumerable<ISession>, IEventData, IReadOnlyList<string>>((s, ed, sh) =>
                {
                    sentByFirst.AddRange(s.Select(i => i.Id));
                });

            var secondSpecificForwarder = new Mock<ISpecificForwarder>();
            var sentBySecond = new List<Guid>();
            secondSpecificForwarder.Setup(sf => sf.Send(It.IsAny<IEnumerable<ISession>>(), It.IsAny<IEventData>(), It.IsAny<IReadOnlyList<string>>()))
                .Callback<IEnumerable<ISession>, IEventData, IReadOnlyList<string>>((s, ed, sh) =>
                {
                    sentBySecond.AddRange(s.Select(i => i.Id));
                });

            var forwarder = new EventsForwarder();
            forwarder.RegisterForwarder(ConsumptionType.OneEventPerServiceType, firstSpecificForwarder.Object);
            forwarder.RegisterForwarder(ConsumptionType.ConsumeAll, secondSpecificForwarder.Object);

            var sessions = new[]
            {
                CreateSession(
                    ("EventOne", ConsumptionType.OneEventPerServiceType),
                    ("EventTwo", ConsumptionType.ConsumeAll),
                    ("EventThree", ConsumptionType.OneEventPerServiceType)),
                CreateSession(),
                CreateSession(
                    ("EventOne", ConsumptionType.ConsumeAll),
                    ("EventTwo", ConsumptionType.ConsumeAll))
            };

            forwarder.Send(sessions, CreateEventData("EventOne"), Enumerable.Empty<string>());
            forwarder.Send(sessions, CreateEventData("EventTwo"), Enumerable.Empty<string>());
            forwarder.Send(sessions, CreateEventData("EventThree"), Enumerable.Empty<string>());

            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(
                    new[] { sessions[0].Id, sessions[0].Id },
                    sentByFirst);

                CollectionAssert.AreEqual(
                    new[] { sessions[2].Id, sessions[0].Id, sessions[2].Id },
                    sentBySecond);
            });
        }

        private static Session CreateSession(params (string, ConsumptionType)[] subscriptions)
        {
            var session = new Session(Guid.NewGuid(), "ServiceOne");
            foreach (var (eventName, consumptionType) in subscriptions)
            {
                session.Subscriptions.Register(eventName, consumptionType);
            }
            return session;
        }

        private static IEventData CreateEventData(string eventName)
        {
            return new EventDataWrapper()
            {
                EventName = eventName
            };
        }
    }
}