using System;
using System.Collections.Generic;
using System.Linq;
using EventBroker.Core;
using EventBroker.Grpc.Data;
using EventBroker.Grpc.Server.EventsForwarding;
using EventBroker.Grpc.Server.Sessions;
using Grpc.Core;

namespace EventBroker.Grpc.Server
{
	public class EventBrokerServer : IServer
	{
		private readonly SessionsContainer _sessions = new SessionsContainer();
		private readonly EventsForwarder _forwarder;

		public EventBrokerServer()
		{
			_forwarder = new EventsForwarder()
				.RegisterForwarder(ConsumptionType.OneEventPerServiceType, new OneEventPerServiceTypeForwarder())
				.RegisterForwarder(ConsumptionType.ConsumeAll, new ConsumeAllEventsForwarder());
		}

		public void InitSession(Guid sessionId, string serviceIdentificator)
		{
			if (_sessions.Exists(sessionId))
			{
				_sessions.Remove(sessionId);
			}

			var session = new Session(sessionId, serviceIdentificator);
			_sessions.Add(session);
		}

		public void RemoveSession(Guid sessionId)
		{
			if (_sessions.Exists(sessionId))
			{
				_sessions.Remove(sessionId);
			}
		}

		public void CreateSubscription(Guid sessionId, string eventName, ConsumptionType consumptionType)
		{
			var session = GetSessionOrThrow(sessionId);

			session.Subscriptions.Register(eventName, consumptionType);
		}

		public void RemoveSubscription(Guid sessionId, string eventName)
		{
			var session = GetSessionOrThrow(sessionId);

			session.Subscriptions.Remove(eventName);
		}

		public string[] FeedEventData(Guid sessionId, IEventData eventData, IEnumerable<string> servicesHandled)
		{
			var emitterSession = GetSessionOrThrow(sessionId);

			var targetSessions = _sessions.Where(s => s != emitterSession);
			return _forwarder.Send(targetSessions, eventData, servicesHandled);
		}

		public IAsyncEnumerable<IEventData> ListenForEvents(Guid sessionId)
		{
			var session = GetSessionOrThrow(sessionId);

			var eventsObservable = session.GetObservable();
			return eventsObservable.ToAsyncEnumerable();
		}

		private Session GetSessionOrThrow(Guid sessionId)
		{
			if (_sessions.TryGetSession(sessionId, out var session))
			{
				return session;
			}

			throw new RpcException(
				new Status(StatusCode.Unauthenticated, "session id was not found"));
		}
	}
}
