using System;
using EventBroker.Core;

namespace EventBroker.Grpc.Client.Source
{
    internal interface ISubjectWrapper : IDisposable
    {
        bool HasObservers { get; }
        void OnNext(IEvent e);
    }
}
