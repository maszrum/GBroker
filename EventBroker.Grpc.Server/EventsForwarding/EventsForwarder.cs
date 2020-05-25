using System.Collections.Generic;
using System.Linq;
using EventBroker.Core;
using EventBroker.Grpc.Data;
using EventBroker.Grpc.Server.Sessions;

namespace EventBroker.Grpc.Server.EventsForwarding
{
	internal class EventsForwarder
	{
		private readonly Dictionary<ConsumptionType, ISpecificForwarder> _specificForwarders 
			= new Dictionary<ConsumptionType, ISpecificForwarder>();

		public EventsForwarder RegisterForwarder(ConsumptionType consumptionType, ISpecificForwarder forwarder)
		{
			_specificForwarders.Add(consumptionType, forwarder);

			return this;
		}

		public string[] Send(IEnumerable<Session> sessions, IEventData eventData, IEnumerable<string> servicesHandled)
		{
			var eventName = eventData.EventName;
			var sessionsWithSubscription = sessions
				.Select(session => session.Subscriptions.Exists(eventName, out var consumptionType) 
					? (Session: session, ConsumptionType: consumptionType) 
					: default)
				.Where(t => t != default)
				.ToArray();

			var servicesHandledBefore = servicesHandled as string[] ?? servicesHandled.ToArray();
			var result = new List<string>();

			foreach (var (consumptionType, forwarder) in _specificForwarders)
			{
				var sessionsWithConsumptionType = sessionsWithSubscription
					.Where(t => t.ConsumptionType == consumptionType)
					.Select(t => t.Session)
					.ToArray();

				if (sessionsWithConsumptionType.Length > 0)
				{
					var newServicesHandled = forwarder.Send(sessionsWithConsumptionType, eventData, servicesHandledBefore);
					result.AddRange(newServicesHandled);
				}
			}

			return result
				.Distinct()
				.ToArray();
		}
	}
}
