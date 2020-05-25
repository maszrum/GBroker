using System;
using System.Collections.Generic;
using System.Linq;
using EventBroker.Grpc.Data;
using EventBroker.Grpc.Server.EventsForwarding;
using EventBroker.Grpc.Server.Sessions;
using Moq;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc.Server
{
	[TestFixture]
	internal class ConsumeAllEventsForwarderTests
	{
		[Test]
		public void event_data_should_be_sent_to_all_sessions()
		{
			var callCounter = new Dictionary<int, int>(
				Enumerable.Range(1, 10).Select(i => new KeyValuePair<int, int>(i, 0)));

			var sessions = Enumerable
				.Range(1, 10)
				.Select(i =>
				{
					var mock = MockSession("SessionTest");
					mock.Setup(m => m.FeedData(It.IsAny<IEventData>()))
						.Callback(() =>
						{
							callCounter[i]++;
						});
					return mock;
				})
				.ToArray();

			var eventData = Enumerable
				.Repeat(1, 10)
				.Select(_ => new EventDataWrapper())
				.ToArray();

			var forwarder = new ConsumeAllEventsForwarder();

			foreach (var ed in eventData)
			{
				forwarder.Send(sessions.Select(s => s.Object), ed, Array.Empty<string>());
			}

			Assert.That(
				callCounter.Values.All(c => c == 10), 
				Is.True);
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