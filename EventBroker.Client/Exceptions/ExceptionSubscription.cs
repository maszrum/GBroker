using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventBroker.Client.Exceptions
{
    internal sealed class ExceptionSubscription : IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);

        public Exception Current { get; private set; }

        private int _waitingCount;

        public void Next(Exception exception)
        {
            if (_waitingCount > 0)
            {
                Current = exception ?? throw new ArgumentNullException(nameof(exception));
                _semaphore.Release(_waitingCount);
                _waitingCount = 0;
            }
            else
            {
                Current = null;
            }
        }

        public Task WaitAsync(CancellationToken cancellationToken = default)
        {
            void Wait()
            {
                _semaphore.Wait(cancellationToken);
            }

            IncrementWaitingCount();

            return Task.Factory.StartNew(
                action: Wait,
                cancellationToken: cancellationToken, 
                creationOptions: TaskCreationOptions.LongRunning, 
                scheduler: TaskScheduler.Current);
        }

        private int IncrementWaitingCount()
        {
            return Interlocked.Increment(ref _waitingCount);
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }
    }
}