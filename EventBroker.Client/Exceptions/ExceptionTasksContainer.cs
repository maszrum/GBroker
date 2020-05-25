using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventBroker.Client.Exceptions
{
    internal class ExceptionTasksContainer : IDisposable
    {
        private readonly Dictionary<Type, ExceptionSubscription> _subscriptions =
            new Dictionary<Type, ExceptionSubscription>();

        private readonly ExceptionSubscription _generalExceptionSubscription =
            new ExceptionSubscription();

        private bool _disposed;

        public async Task<TException> CreateTask<TException>(
            CancellationToken cancellationToken) where TException : Exception
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(
                    nameof(ExceptionTasksContainer));
            }

            var subscription = GetExceptionSubscription(typeof(TException));

            await subscription.WaitAsync(cancellationToken);

            var exception = subscription.Current;

            return (TException)exception;
        }

        public void NextException(Exception exception)
        {
            var exceptionType = exception.GetType();

            var subscription = GetExceptionSubscription(exceptionType);

            subscription.Next(exception);

            if (exceptionType != typeof(Exception))
            {
                _generalExceptionSubscription.Next(exception);
            }
        }

        private ExceptionSubscription GetExceptionSubscription(Type exceptionType)
        {
            if (exceptionType == typeof(Exception))
            {
                return _generalExceptionSubscription;
            }

            if (_subscriptions.TryGetValue(exceptionType, out var subscription))
            {
                return subscription;
            }

            subscription = new ExceptionSubscription();
            _subscriptions.Add(exceptionType, subscription);

            return subscription;
        }

        public void Dispose()
        {
            _generalExceptionSubscription.Dispose();

            foreach (var subscription in _subscriptions.Values)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();

            _disposed = true;
        }
    }
}
