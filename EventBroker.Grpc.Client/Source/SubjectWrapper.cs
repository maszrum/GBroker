using System;
using System.Reactive.Subjects;
using EventBroker.Core;

namespace EventBroker.Grpc.Client.Source
{
    internal class SubjectWrapper<TEvent> : ISubjectWrapper where TEvent : IEvent
    {
        private readonly Subject<TEvent> _subject;

        public SubjectWrapper(Subject<TEvent> subject)
        {
            _subject = subject ?? throw new ArgumentNullException(nameof(subject));
        }

        public bool HasObservers => _subject.HasObservers;

        public void OnNext(IEvent e)
        {
            var eventTyped = (TEvent)e;
            _subject.OnNext(eventTyped);
        }

        public IObservable<TEvent> AsObservable()
        {
            return _subject;
        }

        public void Dispose()
        {
            _subject.Dispose();
        }

        public static SubjectWrapper<TEvent> Create()
        {
            var subject = new Subject<TEvent>();
            return new SubjectWrapper<TEvent>(subject);
        }
    }
}
