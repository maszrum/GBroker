using System;
using System.Collections.Generic;

namespace EventBroker.Client.Exceptions
{
    internal sealed class ActionsInvoker<TException> 
        : IExceptionActionsInvoker where TException : Exception
    {
        private readonly List<Action<TException>> _actions = new List<Action<TException>>();

        public void AddAction(Action<TException> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _actions.Add(action);
        }

        public void Invoke(Exception exception)
        {
            var exceptionTyped = (TException) exception;

            foreach (var action in _actions)
            {
                action(exceptionTyped);
            }
        }

        public void Clear()
        {
            _actions.Clear();
        }
    }
}
