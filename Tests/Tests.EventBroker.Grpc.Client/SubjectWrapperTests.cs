using System;
using System.Reactive.Subjects;
using EventBroker.Core;
using EventBroker.Grpc.Client.Source;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc.Client
{
    internal class SubjectWrapperTests
    {
        public class StubEvent : IEvent
        {
            public StubEvent(int property)
            {
                Property = property;
            }

            public int Property { get; set; }
        }

        [Test]
        public void check_if_events_are_published_correctly()
        {
            var wrapper = SubjectWrapper<StubEvent>.Create();
            var wrapperInterface = (ISubjectWrapper)wrapper;

            var sum = 0;

            wrapperInterface.OnNext(new StubEvent(10));

            Assert.That(sum, Is.Zero);

            wrapper.AsObservable()
                .Subscribe(e =>
                {
                    sum += e.Property;
                });

            wrapperInterface.OnNext(new StubEvent(2));
            wrapperInterface.OnNext(new StubEvent(5));
            wrapperInterface.OnNext(new StubEvent(16));

            Assert.That(sum, Is.EqualTo(2 + 5 + 16));
        }

        [Test]
        public void check_has_observers_property()
        {
            var wrapper = SubjectWrapper<StubEvent>.Create();
            var wrapperInterface = (ISubjectWrapper)wrapper;

            Assert.That(wrapperInterface.HasObservers, Is.False);

            var subscription1 = wrapper.AsObservable()
                .Subscribe(e =>
                {
                });

            Assert.That(wrapperInterface.HasObservers, Is.True);

            var subscription2 = wrapper.AsObservable()
                .Subscribe(e =>
                {
                });

            Assert.That(wrapperInterface.HasObservers, Is.True);

            subscription1.Dispose();

            Assert.That(wrapperInterface.HasObservers, Is.True);

            subscription2.Dispose();

            Assert.That(wrapperInterface.HasObservers, Is.False);
        }

        [Test]
        public void is_observable_dispose_on_wrapper_dispose()
        {
            var subject = new Subject<StubEvent>();
            var wrapper = new SubjectWrapper<StubEvent>(subject);

            wrapper.Dispose();

            Assert.Multiple(() =>
            {
                Assert.Throws<ObjectDisposedException>(() =>
                    subject.OnNext(new StubEvent(10)));

                Assert.That(subject.IsDisposed, Is.True);
            });
        }
    }
}
