using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventBroker.Client.Exceptions;
using NUnit.Framework;

namespace Tests.EventBroker.Client
{
    [TestFixture]
    internal class ExceptionSubscriptionTests
    {
        private const int TestOnXTasks = 10;

        [Test]
        public async Task send_exception_then_wait_then_send_second_exception()
        {
            static async Task<Exception> RunTask(ExceptionSubscription s)
            {
                await s.WaitAsync();
                return s.Current;
            }

            var subscription = new ExceptionSubscription();

            subscription.Next(new ArgumentOutOfRangeException());

            var tasks = Enumerable.Repeat(1, TestOnXTasks)
                .Select(_ => RunTask(subscription))
                .ToArray();

            // ReSharper disable once CoVariantArrayConversion
            EnsureAllTasksAreRunning(tasks);

            var sentException = new IndexOutOfRangeException();
            subscription.Next(sentException);

            await Task.WhenAll(tasks);

            Assert.IsTrue(tasks.All(t => t.Result == sentException));
        }

        [Test]
        public async Task check_if_task_will_be_blocked_after_send_exception()
        {
            static async Task<Exception> RunTask(ExceptionSubscription s)
            {

                await s.WaitAsync();
                return s.Current;
            }

            var subscription = new ExceptionSubscription();

            var tasks = Enumerable.Repeat(1, TestOnXTasks)
                .Select(_ => subscription.WaitAsync())
                .ToArray();

            EnsureAllTasksAreRunning(tasks);

            subscription.Next(new ArgumentOutOfRangeException());

            await Task.WhenAll(tasks);

            var blockedTask = RunTask(subscription);

            await Task.Delay(200);

            Assert.That(
                blockedTask.Status, 
                Is.EqualTo(TaskStatus.Running).Or.EqualTo(TaskStatus.WaitingForActivation));

            subscription.Next(new IndexOutOfRangeException());

            var receivedException = await blockedTask;

            Assert.That(receivedException, Is.TypeOf<IndexOutOfRangeException>());
        }

        [Test]
        public async Task check_if_sent_exception_is_received_exception()
        {
            static async Task<Exception> CreateTask(ExceptionSubscription s)
            {
                await s.WaitAsync();
                return s.Current;
            }

            var subscription = new ExceptionSubscription();

            var tasks = Enumerable.Repeat(1, TestOnXTasks)
                .Select(_ => CreateTask(subscription))
                .ToArray();

            // ReSharper disable once CoVariantArrayConversion
            EnsureAllTasksAreRunning(tasks);

            var sentException = new InvalidOperationException();
            subscription.Next(sentException);

            await Task.WhenAll(tasks);

            Assert.IsTrue(tasks.All(t => t.Result == sentException));
        }

        [Test]
        public async Task task_should_be_canceled()
        {
            var subscription = new ExceptionSubscription();

            var cts = new CancellationTokenSource();

            var task = subscription.WaitAsync(cts.Token);

            await Task.Delay(50, CancellationToken.None);

            cts.Cancel();

            Exception exceptionThrown = null;
            try
            {
                await task;
            }
#pragma warning disable CA1031
            catch (Exception ex)
            {
                exceptionThrown = ex;
            }
#pragma warning restore CA1031

            Assert.That(exceptionThrown, Is.TypeOf<OperationCanceledException>());
        }

        private static void EnsureAllTasksAreRunning(params Task[] tasks)
        {
            while (tasks.Any(t => t.Status != TaskStatus.Running && t.Status != TaskStatus.WaitingForActivation))
            {
            }
        }
    }
}