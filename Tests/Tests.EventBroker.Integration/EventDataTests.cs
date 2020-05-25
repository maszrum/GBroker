using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventBroker.Core;
using EventBroker.Grpc.Client;
using NUnit.Framework;
using Tests.EventBroker.Integration.Core;

namespace Tests.EventBroker.Integration
{
	[TestFixture]
	[Explicit]
	internal class EventDataTests : FunctionalTestBase
	{
		[OneTimeSetUp]
		public void SetUpConverter()
		{
			EventConverter.RegisterEventsAssembly(typeof(FirstEvent).Assembly);
		}

		[Test]
		public async Task check_if_received_event_is_same_as_published_event()
		{
			var receivedEvents = new List<FirstEvent>();

			var consumerClient = CreateClient("TestSubscriber");
			consumerClient.Consumer
				.EventsOfType<FirstEvent>(ConsumptionType.ConsumeAll)
				.Subscribe(e =>
				{
					receivedEvents.Add(e);
				});

			var producerClient = CreateClient("TestSubscriber");
			var eventToPublish = new FirstEvent()
			{
				BoolProperty = true,
				IntProperty = 2348938,
				StringProperty = "sample string"
			};
			await producerClient.Producer.PublishAsync(eventToPublish);

			await Task.Delay(100);

			Assert.Multiple(() =>
			{
				Assert.That(receivedEvents, Has.Count.EqualTo(1));

				Assert.That(receivedEvents[0].IntProperty, Is.EqualTo(2348938));
				Assert.That(receivedEvents[0].BoolProperty, Is.True);
				Assert.That(receivedEvents[0].StringProperty, Is.EqualTo("sample string"));
			});
		}

		[Test]
		public async Task check_if_received_enum_value_is_equal_to_sent_value()
		{
			var receivedEvents = new List<SecondEvent>();

			var consumerClient = CreateClient("TestSubscriber");
			consumerClient.Consumer
				.EventsOfType<SecondEvent>(ConsumptionType.ConsumeAll)
				.Subscribe(e =>
				{
					receivedEvents.Add(e);
				});

			var producerClient = CreateClient("TestSubscriber");
			var eventToPublish = new SecondEvent()
			{
				EnumValue = TestEnum.SecondOption | TestEnum.FourthOption
			};
			await producerClient.Producer.PublishAsync(eventToPublish);

			await Task.Delay(100);

			Assert.Multiple(() =>
			{
				Assert.That(
					receivedEvents, 
					Has.Count.EqualTo(1));
				Assert.That(
					receivedEvents[0].EnumValue == (TestEnum.SecondOption | TestEnum.FourthOption), 
					Is.True);
			});
		}
	}
}
