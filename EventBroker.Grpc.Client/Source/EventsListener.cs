using System;
using System.Threading;
using System.Threading.Tasks;
using EventBroker.Grpc.Client.Core;
using EventBroker.Grpc.Data;

namespace EventBroker.Grpc.Client.Source
{
    internal class EventsListener : IDisposable
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly IGrpcClient _grpc;
        private readonly Action _initializeAction;

        private Action<IEventData> _onDataAction;
        private Action<Exception> _onFaultAction;
        private bool _started;

        public EventsListener(IGrpcClient grpc, Action initializeAction)
        {
            _grpc = grpc ?? throw new ArgumentNullException(nameof(grpc));
            _initializeAction = initializeAction ?? throw new ArgumentNullException(nameof(initializeAction));
        }

        public int WaitOnExceptionMilliseconds { get; set; } = 1000;

        public void Start(Action<IEventData> onData, Action<Exception> onError)
        {
            if (_started)
            {
                throw new InvalidOperationException(
                    $"method {nameof(Start)} can be called only once");
            }

            _onDataAction = onData ?? throw new ArgumentNullException(nameof(onData));
            _onFaultAction = onError ?? throw new ArgumentNullException(nameof(onError));

            var block = new ManualResetEventSlim(false);

            StartListening(block);

            block.Wait();
            block.Dispose();

            _started = true;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        private Task StartListening(ManualResetEventSlim block = null)
        {
            var task = Task.Factory.StartNew(async () =>
            {
                _initializeAction();

                block?.Set();

                await foreach (var eventData in _grpc.Listen(_cts.Token))
                {
                    _onDataAction(eventData);
                }
            }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
            
            task.ContinueWith(t =>
            {
                if (_cts.IsCancellationRequested)
                {
                    return;
                }

                Retry(t).ContinueWith(faultedRetryTask =>
                {
                    if (!_cts.IsCancellationRequested)
                    {
                        Retry(faultedRetryTask).ConfigureAwait(false);
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);
            });

            return task;
        }

        private async Task Retry(Task previousTask)
        {
            if (previousTask.IsFaulted)
            {
                if (previousTask.Exception != null)
                {
                    _onFaultAction(previousTask.Exception.InnerException);
                }

                if (WaitOnExceptionMilliseconds > 0)
                {
                    await Task.Delay(WaitOnExceptionMilliseconds, _cts.Token)
                        .ConfigureAwait(false);
                }
            }

            await StartListening().ConfigureAwait(false);
        }
    }
}
