using System;

namespace EventBroker.Client.Exceptions
{
    internal interface IExceptionActionsInvoker
    {
        void Invoke(Exception exception);
        void Clear();
    }
}