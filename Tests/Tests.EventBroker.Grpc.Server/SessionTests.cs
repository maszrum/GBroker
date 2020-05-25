using System;
using System.Collections.Generic;
using EventBroker.Grpc.Data;
using EventBroker.Grpc.Server.Sessions;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc.Server
{
	[TestFixture]
	internal class SessionTests
	{
		[Test]
		public void check_if_sent_events_are_received()
		{
			var session = new Session(Guid.NewGuid(), "ServiceOne");

			var receivedEvents = new List<string>();
			session.GetObservable().Subscribe(ed =>
			{
				receivedEvents.Add(ed.EventName);
			});

			session.FeedData(new EventDataWrapper() { EventName = "Event1"});
			session.FeedData(new EventDataWrapper() { EventName = "Event2"});
			session.FeedData(new EventDataWrapper() { EventName = "Event3"});
			session.FeedData(new EventDataWrapper() { EventName = "Event4" });

			CollectionAssert.AreEqual(
				new[] { "Event1", "Event2", "Event3", "Event4" },
				receivedEvents);
		}

		[Test]
		public void check_if_session_stops_feeding_after_disposal()
		{
			var session = new Session(Guid.NewGuid(), "ServiceOne");

			var receivedEvents = new List<string>();
			session.GetObservable().Subscribe(ed =>
			{
				receivedEvents.Add(ed.EventName);
			});

			session.Dispose();

			Assert.Throws<ObjectDisposedException>(() =>
			{
				session.FeedData(new EventDataWrapper() { EventName = "Event1" });
			});

			Assert.That(receivedEvents, Is.Empty);
		}
	}
}
