using System;

namespace EventBroker.Client.Exceptions
{
    public interface IExceptionsSink
    {
        void NextException(Exception exception);
    }
}