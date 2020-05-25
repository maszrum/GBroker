using System;
using System.Collections.Generic;
using System.Linq;
using EventBroker.Grpc.Data;
using EventBroker.Grpc.Server.Sessions;

namespace EventBroker.Grpc.Server.EventsForwarding
{
	internal class OneEventPerServiceTypeForwarder : ISpecificForwarder
	{
		private class ServiceQueue
		{
			private readonly Dictionary<Guid, DateTime> _queue;

			public ServiceQueue()
			{
				_queue = new Dictionary<Guid, DateTime>();
			}

			public ServiceQueue(IEnumerable<Guid> sessionIds)
			{
				var kvp = sessionIds
					.Select(id => new KeyValuePair<Guid, DateTime>(id, DateTime.UtcNow.AddDays(-1)))
					.ToArray();

				_queue = new Dictionary<Guid, DateTime>(kvp);
			}

			public void Add(Guid sessionId, DateTime lastUsed)
			{
				_queue.Add(sessionId, lastUsed);
			}

			public ISession GetNext(IEnumerable<ISession> s)
			{
				var sessions = s.ToArray();

				if (sessions.Length == 0)
				{
					throw new ArgumentException(
						"must contain at least one element", nameof(sessions));
				}

				if (_queue.Count == 0)
				{
					Add(sessions[0].Id);
					return sessions[0];
				}

				var currentIndex = -1;
				var currentLastUsed = default(DateTime);

				for (var i = 0; i < sessions.Length; i++)
				{
					var sessionId = sessions[i].Id;

					if (_queue.TryGetValue(sessionId, out var lastUsed))
					{
						if (currentIndex == -1 || (currentIndex >= 0 && currentLastUsed > lastUsed))
						{
							currentIndex = i;
							currentLastUsed = lastUsed;
						}
					}
					else
					{
						Add(sessionId);
						return sessions[i];
					}
				}

				var result = sessions[currentIndex];
				_queue[result.Id] = DateTime.UtcNow;
				return result;
			}

			private void Add(Guid sessionId)
			{
				_queue.Add(sessionId, DateTime.UtcNow);
			}
		}

		private readonly Dictionary<string, ServiceQueue> _queues = new Dictionary<string, ServiceQueue>();

		private readonly object _padlock = new object();

		public string[] Send(IEnumerable<ISession> sessions, IEventData eventData, IReadOnlyList<string> servicesHandled)
		{
			var sessionsArray = sessions as ISession[] ?? sessions.ToArray();

			var groupedByType = sessionsArray
				.GroupBy(s => s.ServiceType)
				.ToArray();

			foreach (var group in groupedByType)
			{
				var serviceType = group.Key;

				ISession session;
				lock (_padlock)
				{
					if (_queues.TryGetValue(serviceType, out var queue))
					{
						session = queue.GetNext(group);
					}
					else
					{
						queue = new ServiceQueue(sessionsArray.Skip(1).Select(s => s.Id));
						_queues.Add(serviceType, queue);

						session = sessionsArray.First();
						queue.Add(session.Id, DateTime.UtcNow);
					}
				}

				session.FeedData(eventData);
			}

			return groupedByType
				.Select(g => g.Key)
				.ToArray();
		}
	}
}
