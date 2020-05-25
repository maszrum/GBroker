using System;
using System.Reactive.Subjects;
using EventBroker.Grpc.Data;

namespace EventBroker.Grpc.Server.Sessions
{
	internal class Session : ISession, IDisposable
	{
		private readonly Subject<IEventData> _subject = new Subject<IEventData>();

		public Session(Guid id, string serviceType)
		{
			Id = id;
			ServiceType = serviceType;
		}

		public Guid Id { get; }

		public string ServiceType { get; }

		public EventSubscriptions Subscriptions { get; } = new EventSubscriptions();

		public void FeedData(IEventData eventData)
		{
			_subject.OnNext(eventData);
		}

		public IObservable<IEventData> GetObservable()
		{
			return _subject;
		}

		public void Dispose()
		{
			_subject.Dispose();
		}
	}
}
