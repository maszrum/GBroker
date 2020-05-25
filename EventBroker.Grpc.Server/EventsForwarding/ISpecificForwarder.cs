using System.Collections.Generic;
using EventBroker.Grpc.Data;
using EventBroker.Grpc.Server.Sessions;

namespace EventBroker.Grpc.Server.EventsForwarding
{
    internal interface ISpecificForwarder
    {
        string[] Send(IEnumerable<ISession> sessions, IEventData eventData, IReadOnlyList<string> servicesHandled);
    }
}
