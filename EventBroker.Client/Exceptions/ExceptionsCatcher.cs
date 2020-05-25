using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventBroker.Client.Exceptions
{
    internal class ExceptionsCatcher : IExceptionsCatcher, IExceptionsSink, IDisposable
    {
        private readonly ExceptionTasksContainer _tasksContainer = new ExceptionTasksContainer();

        private readonly HashSet<CancellationTokenSource> _cancellationTokens =
            new HashSet<CancellationTokenSource>();

        private  readonly ActionsContainer _actionsContainer = new ActionsContainer();

        public void NextException(Exception exception)
        {
            _tasksContainer.NextException(exception);

            _actionsContainer.Invoke(exception);
        }

        public void OnException(Action<Exception> callback)
        {
            _actionsContainer.Add(callback);
        }

        public void OnException<TException>(Action<TException> callback) where TException : Exception
        {
            _actionsContainer.Add(callback);
        }

        public Task<Exception> GetNextAsync()
        {
            var cts = new CancellationTokenSource();

            var task = _tasksContainer.CreateTask<Exception>(cts.Token);
            AttachCancellationToken(task, cts);

            return task;
        }

        public Task<Exception> GetNextAsync(CancellationToken cancellationToken)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var task = _tasksContainer.CreateTask<Exception>(cts.Token);
            AttachCancellationToken(task, cts);

            return task;
        }

        public Task<TException> GetNextAsync<TException>() where TException : Exception
        {
            var cts = new CancellationTokenSource();

            var task = _tasksContainer.CreateTask<TException>(cts.Token);
            AttachCancellationToken(task, cts);

            return task;
        }

        public Task<TException> GetNextAsync<TException>(
            CancellationToken cancellationToken) where TException : Exception
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var task = _tasksContainer.CreateTask<TException>(cts.Token);
            AttachCancellationToken(task, cts);

            return task;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _actionsContainer.Dispose();

                _tasksContainer.Dispose();

                foreach (var cts in _cancellationTokens)
                {
                    cts.Cancel();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void AttachCancellationToken<TException>(
            Task<TException> task, CancellationTokenSource cts) where TException : Exception
        {
            _cancellationTokens.Add(cts);

            task.ContinueWith(_ =>
            {
                _cancellationTokens.Remove(cts);
            });
        }
    }
}
