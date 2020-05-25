using EventBroker.Core;
using EventBroker.Grpc.Server.Sessions;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc.Server
{
	[TestFixture]
	internal class EventSubscriptionsTests
	{
		[Test]
		public void event_should_not_be_present()
		{
			var subscriptions = new EventSubscriptions();

			subscriptions.Register("AnyEvent", ConsumptionType.OneEventPerServiceType);

			Assert.Multiple(() =>
			{
				Assert.That(subscriptions.Exists("FirstEvent"), Is.False);
				Assert.That(subscriptions.Exists("FirstEvent", out var consumptionType), Is.False);
			});
		}

		[Test]
		public void event_should_be_present_after_registration()
		{
			var subscriptions = new EventSubscriptions();

			subscriptions.Register("FirstEvent", ConsumptionType.OneEventPerServiceType);

			Assert.Multiple(() =>
			{
				Assert.That(subscriptions.Exists("FirstEvent"), Is.True);
				Assert.That(subscriptions.Exists("FirstEvent", out var consumptionType), Is.True);
				Assert.That(consumptionType, Is.EqualTo(ConsumptionType.OneEventPerServiceType));
			});
		}

		[Test]
		public void adding_event_twice_should_not_throw()
		{
			var subscriptions = new EventSubscriptions();

			Assert.DoesNotThrow(() =>
			{
				subscriptions.Register("FirstEvent", ConsumptionType.OneEventPerServiceType);
				subscriptions.Register("FirstEvent", ConsumptionType.OneEventPerServiceType);
			});
		}

		[Test]
		public void removing_event_that_not_exists_should_not_throw()
		{
			var subscriptions = new EventSubscriptions();

			Assert.DoesNotThrow(() =>
			{
				subscriptions.Remove("FirstEvent");
			});
		}

		[Test]
		public void check_if_event_was_removed()
		{
			var subscriptions = new EventSubscriptions();

			subscriptions.Register("FirstEvent", ConsumptionType.OneEventPerServiceType);
			subscriptions.Remove("FirstEvent");

			Assert.That(subscriptions.Exists("FirstEvent"), Is.False);
		}
	}
}
