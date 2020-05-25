using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EventBroker.Client.Exceptions;
using NUnit.Framework;

namespace Tests.EventBroker.Client
{
    [TestFixture]
    internal class ExceptionActionsInvokerTests
    {
        [Test]
        public void one_action_should_be_invoked_one_time()
        {
            var invoker = new ActionsInvoker<IOException>();

            Exception receivedException = null;
            var invokedTimes = 0;

            invoker.AddAction(ex =>
            {
                receivedException = ex;
                invokedTimes++;
            });

            var sentException = new IOException();

            invoker.Invoke(sentException);

            Assert.Multiple(() =>
            {
                Assert.That(invokedTimes, Is.EqualTo(1));
                Assert.That(receivedException, Is.SameAs(sentException));
            });
        }

        [Test]
        public void three_actions_should_be_invoked_one_time()
        {
            var invoker = new ActionsInvoker<IOException>();

            var receivedExceptions = new List<IOException>();
            var invokedTimes = new Dictionary<int, int>()
            {
                [0] = 0,
                [1] = 0,
                [2] = 0
            };

            for (var i = 0; i < 3; i++)
            {
                var id = i;
                invoker.AddAction(ex =>
                {
                    receivedExceptions.Add(ex);
                    invokedTimes[id]++;
                });
            }

            var sentException = new IOException();

            invoker.Invoke(sentException);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(receivedExceptions.All(ex => ex == sentException));
                Assert.IsTrue(invokedTimes.Values.All(t => t == 1));
            });
        }

        [Test]
        public void one_action_should_be_invoked_three_times()
        {
            var invoker = new ActionsInvoker<IOException>();

            var receivedExceptions = new List<IOException>();

            invoker.AddAction(ex =>
            {
                receivedExceptions.Add(ex);
            });

            invoker.Invoke(new IOException("1"));
            invoker.Invoke(new IOException("2"));
            invoker.Invoke(new IOException("3"));

            CollectionAssert.AreEqual(
                receivedExceptions.Select(ex => ex.Message),
                new[] {"1", "2", "3"});
        }

        [Test]
        public void three_actions_should_be_invoked_three_times()
        {
            var invoker = new ActionsInvoker<IOException>();

            var receivedExceptions = new Dictionary<int, string>()
            {
                [0] = string.Empty,
                [1] = string.Empty,
                [2] = string.Empty
            };

            for (var i = 0; i < 3; i++)
            {
                var id = i;
                invoker.AddAction(ex =>
                {
                    receivedExceptions[id] += ex.Message;
                });
            }

            invoker.Invoke(new IOException("1"));
            invoker.Invoke(new IOException("2"));
            invoker.Invoke(new IOException("3"));

            Assert.IsTrue(receivedExceptions.Values.All(v => v == "123"));
        }

        [Test]
        public void should_throw_on_invalid_exception_type()
        {
            var invoker = new ActionsInvoker<IOException>();

            Assert.Throws<InvalidCastException>(() =>
            {
                invoker.Invoke(new ArgumentOutOfRangeException());
            });
        }

        [Test]
        public void invoke_on_empty()
        {
            var invoker = new ActionsInvoker<IOException>();

            invoker.Invoke(new IOException());
        }

        [Test]
        public void check_if_empty_after_clear()
        {
            var invoker = new ActionsInvoker<IOException>();

            var receivedExceptions = new List<IOException>();

            for (var i = 0; i < 3; i++)
            {
                invoker.AddAction(ex =>
                {
                    receivedExceptions.Add(ex);
                });
            }

            invoker.Clear();

            invoker.Invoke(new IOException("1"));
            invoker.Invoke(new IOException("2"));
            invoker.Invoke(new IOException("3"));

            Assert.That(receivedExceptions, Is.Empty);
        }
    }
}
