using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventBroker.Client.Exceptions;
using NUnit.Framework;

namespace Tests.EventBroker.Client
{
    [TestFixture]
    internal class ExceptionTasksContainerTests
    {
        [Test]
        public async Task create_task_for_one_typed_exception()
        {
            var container = new ExceptionTasksContainer();

            var task = container.CreateTask<InvalidOperationException>(CancellationToken.None);

            EnsureAllTasksAreRunning(task);

            var sentException = new InvalidOperationException();
            container.NextException(sentException);

            var receivedException = await task;

            Assert.That(receivedException, Is.SameAs(sentException));
        }

        [Test]
        public async Task create_many_tasks_and_wait_for_general_complete()
        {
            var container = new ExceptionTasksContainer();

            var cts = new CancellationTokenSource();

            var task1 = container.CreateTask<InvalidOperationException>(cts.Token);
            var task2 = container.CreateTask<Exception>(cts.Token);
            var task3 = container.CreateTask<Exception>(cts.Token);
            var task4 = container.CreateTask<InvalidOperationException>(cts.Token);

            EnsureAllTasksAreRunning(task1, task2, task3, task4);

            container.NextException(new IOException());

            var exception2 = await task2;
            var exception3 = await task3;

            Assert.Multiple(() =>
            {
                Assert.That(task1.IsCompleted, Is.False);
                Assert.That(task4.IsCompleted, Is.False);
                Assert.That(exception2, Is.TypeOf<IOException>());
                Assert.That(exception3, Is.TypeOf<IOException>());
            });
        }

        [Test]
        public async Task create_many_tasks_and_wait_for_all_complete()
        {
            var container = new ExceptionTasksContainer();

            var cts = new CancellationTokenSource();

            var task1 = container.CreateTask<InvalidOperationException>(cts.Token);
            var task2 = container.CreateTask<Exception>(cts.Token);
            var task3 = container.CreateTask<Exception>(cts.Token);
            var task4 = container.CreateTask<InvalidOperationException>(cts.Token);

            EnsureAllTasksAreRunning(task1, task2, task3, task4);

            container.NextException(new InvalidOperationException());

            var exception1 = await task1;
            var exception2 = await task2;
            var exception3 = await task3;
            var exception4 = await task4;

            Assert.Multiple(() =>
            {
                Assert.That(exception1, Is.TypeOf<InvalidOperationException>());
                Assert.That(exception2, Is.TypeOf<InvalidOperationException>());
                Assert.That(exception3, Is.TypeOf<InvalidOperationException>());
                Assert.That(exception4, Is.TypeOf<InvalidOperationException>());
            });
        }

        [TestCase(typeof(InvalidOperationException))]
        [TestCase(typeof(Exception))]
        public async Task create_task_for_one_general_exception(Type exceptionType)
        {
            var container = new ExceptionTasksContainer();

            var task = container.CreateTask<Exception>(CancellationToken.None);

            await Task.Delay(100);

            var sentException = (Exception)Activator.CreateInstance(exceptionType);

            container.NextException(sentException);

            var receivedException = await task;

            Assert.That(receivedException, Is.SameAs(sentException));
        }

        [Test]
        public async Task task_should_be_cancelled_after_token_source_cancel()
        {
            var container = new ExceptionTasksContainer();

            var cts = new CancellationTokenSource();

            var task = container.CreateTask<Exception>(cts.Token);

            await Task.Delay(100, CancellationToken.None);

            cts.Cancel();

            await Task.Delay(500, CancellationToken.None);

            Assert.That(task.IsCanceled, Is.True);
        }

        [Test]
        public async Task dispose_test()
        {
            var container = new ExceptionTasksContainer();

            var task = container.CreateTask<InvalidOperationException>(CancellationToken.None);

            EnsureAllTasksAreRunning(task);

            container.Dispose();

            var exceptionThrown = false;
            try
            {
                await container.CreateTask<IOException>(CancellationToken.None);
            }
#pragma warning disable CA1031
            catch (ObjectDisposedException)
            {
                exceptionThrown = true;
            }
#pragma warning restore CA1031

            Assert.That(exceptionThrown, Is.True);

        }

        private static void EnsureAllTasksAreRunning(params Task[] tasks)
        {
            while (tasks.Any(t => t.Status != TaskStatus.Running && t.Status != TaskStatus.WaitingForActivation))
            {
            }
        }
    }
}
