using System;
using System.Collections.Generic;
using System.IO;
using EventBroker.Client.Exceptions;
using NUnit.Framework;

namespace Tests.EventBroker.Client
{
    [TestFixture]
    internal class ActionsContainerTests
    {
        [Test]
        public void add_general_action_and_invoke_two_exceptions()
        {
            var container = new ActionsContainer();

            var receivedExceptions = new List<Exception>();

            container.Add<Exception>(ex =>
            {
                receivedExceptions.Add(ex);
            });
             
            container.Invoke(new InvalidOperationException());
            container.Invoke(new IOException());

            Assert.Multiple(() =>
            {
                Assert.That(receivedExceptions, Has.Count.EqualTo(2));
                Assert.That(receivedExceptions[0], Is.TypeOf<InvalidOperationException>());
                Assert.That(receivedExceptions[1], Is.TypeOf<IOException>());
            });
        }

        [Test]
        public void add_typed_action_and_invoke_two_exceptions()
        {
            var container = new ActionsContainer();

            var receivedExceptions = new List<Exception>();

            container.Add<ArgumentOutOfRangeException>(ex =>
            {
                receivedExceptions.Add(ex);
            });

            container.Invoke(new ArgumentOutOfRangeException());
            container.Invoke(new ArgumentOutOfRangeException());

            Assert.Multiple(() =>
            {
                Assert.That(receivedExceptions, Has.Count.EqualTo(2));
                Assert.That(receivedExceptions[0], Is.TypeOf<ArgumentOutOfRangeException>());
                Assert.That(receivedExceptions[1], Is.TypeOf<ArgumentOutOfRangeException>());
            });
        }

        [Test]
        public void multiple_actions_multiple_exceptions()
        {
            var container = new ActionsContainer();

            var receivedExceptionsCounter = new Dictionary<int, int>()
            {
                [0] = 0,
                [1] = 0,
                [2] = 0,
                [3] = 0,
                [4]=  0
            };

            container.Add<Exception>(ex => { receivedExceptionsCounter[0]++; });
            container.Add<Exception>(ex => { receivedExceptionsCounter[1]++; });
            container.Add<InvalidOperationException>(ex => { receivedExceptionsCounter[2]++; });
            container.Add<ArgumentNullException>(ex => { receivedExceptionsCounter[3]++; });
            container.Add<ArgumentNullException>(ex => { receivedExceptionsCounter[4]++; });


            container.Invoke(new ArgumentNullException());
            container.Invoke(new ArgumentNullException());
            container.Invoke(new ArgumentOutOfRangeException());
            container.Invoke(new InvalidOperationException());
            container.Invoke(new ArgumentOutOfRangeException());
            container.Invoke(new InvalidOperationException());
            container.Invoke(new Exception());

            CollectionAssert.AreEqual(
                receivedExceptionsCounter.Values,
                new[] {7, 7, 2, 2, 2});
        }

        [Test]
        public void no_actions_test()
        {
            var container = new ActionsContainer();

            container.Invoke(new InvalidOperationException());
        }

        [Test]
        public void dispose_test()
        {
            var container = new ActionsContainer();

            container.Add<InvalidOperationException>(ex =>
            {
            });

            container.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                container.Invoke(new Exception());
            });
        }
    }
}