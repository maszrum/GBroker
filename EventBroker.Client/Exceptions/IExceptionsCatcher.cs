using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventBroker.Client.Exceptions
{
    public interface IExceptionsCatcher
    {
        void OnException(Action<Exception> callback);
        void OnException<TException>(Action<TException> callback) where TException : Exception;
        Task<Exception> GetNextAsync();
        Task<Exception> GetNextAsync(CancellationToken cancellationToken);
        Task<TException> GetNextAsync<TException>() where TException : Exception;
        Task<TException> GetNextAsync<TException>(CancellationToken cancellationToken) where TException : Exception;
    }
}