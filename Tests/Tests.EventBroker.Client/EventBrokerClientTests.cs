using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventBroker.Client;
using EventBroker.Client.Exceptions;
using EventBroker.Client.Interceptor;
using EventBroker.Core;
using Moq;
using NUnit.Framework;

namespace Tests.EventBroker.Client
{
	[TestFixture]
	internal class EventBrokerClientTests
	{
		[Test]
		public void dispose_method_should_be_invoked_on_client_disposal()
		{
			var sink = new Mock<IEventsSink>();
			var source = new Mock<IEventsSource>();

			var sinks = new HashSet<IEventsSink>()
			{
				sink.Object
			};

			var sources = new HashSet<IEventsSource>()
			{
				source.Object
			};

			var client = new EventBrokerClient(
				"TestService", 
				sinks, 
				sources, 
				new HashSet<IEventInterceptor>(), 
				new ExceptionsCatcher());

			client.Dispose();

			source.Verify(s => s.Dispose(), Times.Once);
		}

		public class MockEvent : IEvent
		{
		}

		[Test]
		public void local_event_should_be_consumed()
		{
			var client = new EventBrokerClient(
				"TestService",
				new HashSet<IEventsSink>(), 
				new HashSet<IEventsSource>(), 
				new HashSet<IEventInterceptor>(),
				new ExceptionsCatcher());

			var producer = client.Producer;
			var consumer = client.Consumer;

			var receivedEvents = new List<MockEvent>();

			consumer.EventsOfType<MockEvent>(ConsumptionType.ConsumeAll)
				.Subscribe(ev =>
				{
					receivedEvents.Add(ev);
				});

			var ev1 = new MockEvent();
			var ev2 = new MockEvent();

			producer.Publish(ev1);
			producer.Publish(ev2);

			CollectionAssert.AreEqual(
				new[] { ev1, ev2 },
				receivedEvents);
		}

		[Test]
		public async Task local_event_should_be_consumed_async()
		{
			var client = new EventBrokerClient(
				"TestService",
				new HashSet<IEventsSink>(), 
				new HashSet<IEventsSource>(), 
				new HashSet<IEventInterceptor>(),
				new ExceptionsCatcher());

			var producer = client.Producer;
			var consumer = client.Consumer;

			var receivedEvents = new List<MockEvent>();

			consumer.EventsOfType<MockEvent>(ConsumptionType.ConsumeAll)
				.Subscribe(ev =>
				{
					receivedEvents.Add(ev);
				});

			var ev1 = new MockEvent();
			var ev2 = new MockEvent();

			await producer.PublishAsync(ev1);
			await producer.PublishAsync(ev2);

			CollectionAssert.AreEqual(
				new[] { ev1, ev2 },
				receivedEvents);
		}
	}
}
