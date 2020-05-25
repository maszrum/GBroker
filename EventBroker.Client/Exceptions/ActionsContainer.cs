using System;
using System.Collections.Generic;

namespace EventBroker.Client.Exceptions
{
    internal sealed class ActionsContainer : IDisposable
    {
        private readonly List<Action<Exception>> _generalExceptionActions = 
            new List<Action<Exception>>();

        private readonly Dictionary<Type, IExceptionActionsInvoker> _invokers = 
            new Dictionary<Type, IExceptionActionsInvoker>();

        private bool _disposed = false;

        public void Invoke(Exception exception)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ActionsContainer));
            }

            InvokeGeneral(exception);

            InvokeTypedIfNotGeneral(exception);
        }

        public void Add<TException>(Action<TException> action) where TException : Exception
        {
            var exceptionType = typeof(TException);

            if (exceptionType == typeof(Exception))
            {
                var actionTyped = (Action<Exception>) action;
                _generalExceptionActions.Add(actionTyped);
            }
            else
            {
                ActionsInvoker<TException> invokerTyped;
                if (!_invokers.TryGetValue(exceptionType, out var invoker))
                {
                    invokerTyped = new ActionsInvoker<TException>();

                    _invokers.Add(exceptionType, invokerTyped);
                }
                else
                {
                    invokerTyped = (ActionsInvoker<TException>) invoker;
                }

                invokerTyped.AddAction(action);
            }
        }

        public void Dispose()
        {
            foreach (var invoker in _invokers.Values)
            {
                invoker.Clear();
            }
            _invokers.Clear();

            _generalExceptionActions.Clear();

            _disposed = true;
        }

        private void InvokeGeneral(Exception exception)
        {
            foreach (var action in _generalExceptionActions)
            {
                action(exception);
            }
        }

        private void InvokeTypedIfNotGeneral(Exception exception)
        {
            var exceptionType = exception.GetType();

            if (exceptionType != typeof(Exception) && 
                _invokers.TryGetValue(exceptionType, out var invoker))
            {
                invoker.Invoke(exception);
            }
        }
    }
}