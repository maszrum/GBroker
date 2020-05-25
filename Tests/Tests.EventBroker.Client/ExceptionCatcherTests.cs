using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventBroker.Client.Exceptions;
using NUnit.Framework;

namespace Tests.EventBroker.Client
{
    [TestFixture]
    internal class ExceptionCatcherTests
    {
        [Test]
        public async Task get_next_two_times_test()
        {
            var catcher = new ExceptionsCatcher();

            var ioeTask = catcher.GetNextAsync<IOException>();
            var generalTask = catcher.GetNextAsync();

            EnsureAllTasksAreRunning(ioeTask, generalTask);

            catcher.NextException(new IOException());

            var ioe = await ioeTask;
            var general = await generalTask;

            Assert.Multiple(() =>
            {
                Assert.That(ioe, Is.TypeOf<IOException>());
                Assert.That(general, Is.TypeOf<IOException>());
            });

            catcher.Dispose();
        }

        [Test]
        public async Task get_next_two_times_and_get_only_one()
        {
            var catcher = new ExceptionsCatcher();

            var ioeTask = catcher.GetNextAsync<IOException>();
            var generalTask = catcher.GetNextAsync();

            EnsureAllTasksAreRunning(ioeTask, generalTask);

            catcher.NextException(new ArgumentOutOfRangeException());

            var general = await generalTask;

            Assert.Multiple(() =>
            {
                Assert.That(general, Is.TypeOf<ArgumentOutOfRangeException>());
                Assert.That(ioeTask.IsCompleted, Is.False);
            });

            catcher.Dispose();
        }

        [Test]
        public async Task get_next_three_times_and_cancel_one()
        {
            var catcher = new ExceptionsCatcher();

            var cts = new CancellationTokenSource();

            var ioeTask1 = catcher.GetNextAsync<IOException>(cts.Token);
            var ioeTask2 = catcher.GetNextAsync<IOException>(CancellationToken.None);
            var generalTask = catcher.GetNextAsync(CancellationToken.None);

            EnsureAllTasksAreRunning(ioeTask1, ioeTask2, generalTask);

            cts.Cancel();

            catcher.NextException(new IOException());

            var ioe = await ioeTask2;
            var general = await generalTask;

            Exception thrownException = null;
            try
            {
                await ioeTask1;
            }
#pragma warning disable CA1031
            catch (Exception ex)
            {
                thrownException = ex;
            }
#pragma warning restore CA1031

            Assert.Multiple(() =>
            {
                Assert.That(thrownException, Is.TypeOf<OperationCanceledException>());
                Assert.That(ioe, Is.TypeOf<IOException>());
                Assert.That(general, Is.TypeOf<IOException>());
            });
        }

        [Test]
        public async Task check_if_get_next_throws_on_catcher_disposal()
        {
            var catcher = new ExceptionsCatcher();

            var ioeTask = catcher.GetNextAsync<IOException>();
            var generalTask = catcher.GetNextAsync(new CancellationTokenSource().Token);

            EnsureAllTasksAreRunning(ioeTask, generalTask);

            catcher.Dispose();

            Exception thrownException1 = null;
            try
            {
                await ioeTask;
            }
#pragma warning disable CA1031
            catch (Exception ex)
            {
                thrownException1 = ex;
            }
#pragma warning restore CA1031

            Exception thrownException2 = null;
            try
            {
                await generalTask;
            }
#pragma warning disable CA1031
            catch (Exception ex)
            {
                thrownException2 = ex;
            }
#pragma warning restore CA1031

            Assert.Multiple(() =>
            {
                Assert.That(thrownException1, Is.TypeOf<OperationCanceledException>());
                Assert.That(thrownException2, Is.TypeOf<OperationCanceledException>());
            });
        }

        [Test]
        public void on_exception_test()
        {
            var catcher = new ExceptionsCatcher();

            var exceptionsReceived = new List<Exception>();

            catcher.OnException<IOException>(ex =>
                exceptionsReceived.Add(ex));

            catcher.OnException(ex =>
                exceptionsReceived.Add(ex));

            catcher.OnException<ArgumentOutOfRangeException>(ex =>
                exceptionsReceived.Add(ex));

            catcher.NextException(new ArgumentOutOfRangeException());
            catcher.NextException(new IOException());

            Assert.Multiple(() =>
            {
                Assert.That(exceptionsReceived, Has.Count.EqualTo(4));
                Assert.That(
                    exceptionsReceived.OfType<ArgumentOutOfRangeException>().ToList(), 
                    Has.Count.EqualTo(2));
                Assert.That(
                    exceptionsReceived.OfType<IOException>().ToList(),
                    Has.Count.EqualTo(2));
            });
        }

        private static void EnsureAllTasksAreRunning(params Task[] tasks)
        {
            while (tasks.Any(t => t.Status != TaskStatus.Running && t.Status != TaskStatus.WaitingForActivation))
            {
            }
        }
    }
}
