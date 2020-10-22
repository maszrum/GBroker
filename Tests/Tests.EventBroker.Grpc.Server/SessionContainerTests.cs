using System;
using System.Linq;
using System.Threading.Tasks;
using EventBroker.Grpc.Server.Sessions;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc.Server
{
    [TestFixture]
    internal class SessionContainerTests
    {
        [Test]
        public void check_if_sessions_was_added()
        {
            var container = new SessionsContainer();

            var session1 = new Session(Guid.NewGuid(), "ServiceOne");
            var session2 = new Session(Guid.NewGuid(), "ServiceTwo");
            container.Add(session1);
            container.Add(session2);

            Assert.Multiple(() =>
            {
                Assert.That(container.Exists(session1.Id), Is.True);
                Assert.That(container.Exists(session2.Id), Is.True);
            });
        }

        [Test]
        public void exists_should_return_false()
        {
            var container = new SessionsContainer();

            var session = new Session(Guid.NewGuid(), "ServiceOne");
            container.Add(session);

            Assert.That(container.Exists(Guid.NewGuid()), Is.False);
        }

        [Test]
        public void try_get_session_should_return_session()
        {
            var container = new SessionsContainer();

            var session = new Session(Guid.NewGuid(), "ServiceOne");
            container.Add(session);

            Assert.Multiple(() =>
            {
                Assert.That(container.TryGetSession(session.Id, out var s));
                Assert.That(s, Is.SameAs(session));
            });
        }

        [Test]
        public void get_enumerator_should_return_all_added_sessions()
        {
            var container = new SessionsContainer();

            var session1 = new Session(Guid.NewGuid(), "ServiceOne");
            var session2 = new Session(Guid.NewGuid(), "ServiceTwo");
            var session3 = new Session(Guid.NewGuid(), "ServiceThree");
            container.Add(session1);
            container.Add(session2);
            container.Add(session3);

            CollectionAssert.AreEquivalent(
                new[] { session1, session2, session3 },
                container);
        }

        [Test]
        public void adding_sessions_in_parallel()
        {
            var container = new SessionsContainer();

            var sessions = Enumerable
                .Range(1, 100)
                .Select(i => new Session(Guid.NewGuid(), $"Service{i}"))
                .ToArray();

            var foreachResult = Parallel.ForEach(sessions, s =>
            {
                container.Add(s);
            });

            while (!foreachResult.IsCompleted) { }

            Assert.That(container.ToArray(), Has.Length.EqualTo(100));
        }

        [Test]
        public void check_if_session_was_removed()
        {
            var container = new SessionsContainer();

            var session1 = new Session(Guid.NewGuid(), "ServiceOne");
            var session2 = new Session(Guid.NewGuid(), "ServiceTwo");
            var session3 = new Session(Guid.NewGuid(), "ServiceThree");
            container.Add(session1);
            container.Add(session2);
            container.Add(session3);
            container.Remove(session2.Id);
            container.Remove(session3.Id);

            CollectionAssert.AreEqual(
                new[] { session1 },
                container);
        }
    }
}
