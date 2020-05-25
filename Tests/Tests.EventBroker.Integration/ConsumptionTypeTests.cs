using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventBroker.Core;
using EventBroker.Grpc.Client;
using NUnit.Framework;
using Tests.EventBroker.Integration.Core;

namespace Tests.EventBroker.Integration
{
	[TestFixture]
	[Explicit]
	internal class ConsumptionTypeTests : FunctionalTestBase
	{
		[OneTimeSetUp]
		public void SetUpConverter()
		{
			EventConverter.RegisterEventsAssembly(typeof(FirstEvent).Assembly);
		}

		[Test]
		public async Task check_if_events_are_consumed_correctly()
		{
			var consumerClients = Enumerable.Range(1, 4)
				.Select(i => i <= 3
					? CreateClient("OnePerService")
					: CreateClient("GiveMeAll"))
				.ToArray();

			var publisherClients = Enumerable.Range(1, 2)
				.Select(_ => CreateClient("PublisherService"))
				.ToArray();

			var receivedEvents = Enumerable
				.Range(1, 4)
				.Select(_ => new List<FirstEvent>())
				.ToArray();

			consumerClients[0].Consumer
				.EventsOfType<FirstEvent>(ConsumptionType.OneEventPerServiceType)
				.Subscribe(ev =>
				{
					receivedEvents[0].Add(ev);
				});

			consumerClients[1].Consumer
				.EventsOfType<FirstEvent>(ConsumptionType.OneEventPerServiceType)
				.Subscribe(ev =>
				{
					receivedEvents[1].Add(ev);
				});

			consumerClients[2].Consumer
				.EventsOfType<FirstEvent>(ConsumptionType.OneEventPerServiceType)
				.Subscribe(ev =>
				{
					receivedEvents[2].Add(ev);
				});

			consumerClients[3].Consumer
				.EventsOfType<FirstEvent>(ConsumptionType.ConsumeAll)
				.Subscribe(ev =>
				{
					receivedEvents[3].Add(ev);
				});

			await Task.Delay(5000);

			publisherClients[0].Producer.Publish(new FirstEvent());
			await publisherClients[1].Producer.PublishAsync(new FirstEvent());
			await publisherClients[0].Producer.PublishAsync(new FirstEvent());

			while (receivedEvents.Select(re => re.Count).Sum() != 6)
			{
				await Task.Delay(10);
			}

			Assert.Multiple(() =>
			{
				Assert.That(receivedEvents[0], Has.Count.EqualTo(1));
				Assert.That(receivedEvents[1], Has.Count.EqualTo(1));
				Assert.That(receivedEvents[2], Has.Count.EqualTo(1));
				Assert.That(receivedEvents[3], Has.Count.EqualTo(3));
			});
		}
	}
}
