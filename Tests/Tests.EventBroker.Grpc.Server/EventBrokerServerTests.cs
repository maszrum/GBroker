using System;
using System.Collections.Generic;
using System.Linq;
using EventBroker.Core;
using EventBroker.Grpc.Data;
using EventBroker.Grpc.Server;
using Grpc.Core;
using Moq;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc.Server
{
	[TestFixture]
	internal class EventBrokerServerTests
	{
		[Test]
		public void sender_of_event_should_not_receive_own_event()
		{
			var server = new EventBrokerServer();

			var sessionId = Guid.NewGuid();
			server.InitSession(sessionId, "ServiceOne");

			server.CreateSubscription(sessionId, "SomeEvent", ConsumptionType.OneEventPerServiceType);

			var receivedEvents = new List<string>();
			server.ListenForEvents(sessionId)
				.ToObservable()
				.Subscribe(eventData =>
				{
					receivedEvents.Add(eventData.EventName);
				});

			server.FeedEventData(sessionId, MockEventData("SomeEvent"), Enumerable.Empty<string>());

			Assert.That(receivedEvents, Is.Empty);
		}

		[Test]
		public void server_should_return_three_events()
		{
			var server = new EventBrokerServer();

			var senderId = Guid.NewGuid();
			server.InitSession(senderId, "ServiceOne");

			var receiverId = Guid.NewGuid();
			server.InitSession(receiverId, "ServiceTwo");

			server.CreateSubscription(receiverId, "SomeEvent", ConsumptionType.OneEventPerServiceType);

			var receivedEvents = new List<string>();
			server.ListenForEvents(receiverId)
				.ToObservable()
				.Subscribe(eventData =>
				{
					receivedEvents.Add(eventData.EventName);
				});

			server.FeedEventData(senderId, MockEventData("SomeEvent"), Enumerable.Empty<string>());
			server.FeedEventData(senderId, MockEventData("AnotherEvent"), Enumerable.Empty<string>());
			server.FeedEventData(senderId, MockEventData("EventThatWasNotSubscribed"), Enumerable.Empty<string>());
			server.FeedEventData(senderId, MockEventData("SomeEvent"), Enumerable.Empty<string>());

			server.CreateSubscription(receiverId, "AnotherEvent", ConsumptionType.OneEventPerServiceType);

			server.FeedEventData(senderId, MockEventData("AnotherEvent"), Enumerable.Empty<string>());
			server.FeedEventData(senderId, MockEventData("EventThatWasNotSubscribed"), Enumerable.Empty<string>());

			CollectionAssert.AreEqual(
				new[] { "SomeEvent", "SomeEvent", "AnotherEvent" },
				receivedEvents);
		}

		[Test]
		public void initialize_session_twice_and_check_if_subscriptions_were_cleared()
		{
			var server = new EventBrokerServer();

			var senderId = Guid.NewGuid();
			server.InitSession(senderId, "ServiceOne");

			var receiverId = Guid.NewGuid();
			server.InitSession(receiverId, "ServiceTwo");

			server.CreateSubscription(receiverId, "SomeEvent", ConsumptionType.OneEventPerServiceType);

			var receivedEvents = new List<string>();
			server.ListenForEvents(receiverId)
				.ToObservable()
				.Subscribe(eventData =>
				{
					receivedEvents.Add(eventData.EventName);
				});

			server.FeedEventData(senderId, MockEventData("SomeEvent"), Enumerable.Empty<string>());

			server.InitSession(receiverId, "ServiceTwo");

			server.FeedEventData(senderId, MockEventData("SomeEvent"), Enumerable.Empty<string>());

			CollectionAssert.AreEqual(
				new[] { "SomeEvent" },
				receivedEvents);
		}

		[Test]
		public void check_if_event_subscription_was_removed()
		{
			var server = new EventBrokerServer();

			var senderId = Guid.NewGuid();
			server.InitSession(senderId, "ServiceOne");

			var receiverId = Guid.NewGuid();
			server.InitSession(receiverId, "ServiceTwo");

			server.CreateSubscription(receiverId, "SomeEvent", ConsumptionType.OneEventPerServiceType);

			var receivedEvents = new List<string>();
			server.ListenForEvents(receiverId)
				.ToObservable()
				.Subscribe(eventData =>
				{
					receivedEvents.Add(eventData.EventName);
				});

			server.FeedEventData(senderId, MockEventData("SomeEvent"), Enumerable.Empty<string>());

			server.RemoveSubscription(receiverId, "SomeEvent");

			server.FeedEventData(senderId, MockEventData("SomeEvent"), Enumerable.Empty<string>());

			CollectionAssert.AreEqual(
				new[] { "SomeEvent" },
				receivedEvents);
		}

		[Test]
		public void should_throw_on_creating_subscription_with_not_existing_session()
		{
			var server = new EventBrokerServer();

			Assert.Throws<RpcException>(() =>
			{
				server.CreateSubscription(Guid.NewGuid(), "SomeEvent", ConsumptionType.OneEventPerServiceType);
			});
		}

		[Test]
		public void should_throw_on_feeding_data_with_not_existing_session()
		{
			var server = new EventBrokerServer();

			var sessionId = Guid.NewGuid();
			server.InitSession(sessionId, "ServiceOne");
			server.RemoveSession(sessionId);

			Assert.Throws<RpcException>(() =>
			{
				server.FeedEventData(sessionId, new Mock<IEventData>().Object, Enumerable.Empty<string>());
			});
		}

		[Test]
		public void should_throw_when_listen_with_not_existing_session()
		{
			var server = new EventBrokerServer();

			Assert.ThrowsAsync<RpcException>(async () =>
			{
				await foreach (var _ in server.ListenForEvents(Guid.NewGuid()))
				{
				}
			});
		}

		private static IEventData MockEventData(string eventName)
		{
			var mock = new Mock<IEventData>();
			mock.SetupGet(m => m.EventName).Returns(eventName);
			return mock.Object;
		}
	}
}
