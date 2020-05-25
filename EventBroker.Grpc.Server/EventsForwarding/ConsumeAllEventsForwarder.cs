using System;
using System.Collections.Generic;
using EventBroker.Grpc.Data;
using EventBroker.Grpc.Server.Sessions;

namespace EventBroker.Grpc.Server.EventsForwarding
{
    internal class ConsumeAllEventsForwarder : ISpecificForwarder
    {
        public string[] Send(IEnumerable<ISession> sessions, IEventData eventData, IReadOnlyList<string> servicesHandle)
        {
            foreach (var session in sessions)
            {
                session.FeedData(eventData);
            }

            return Array.Empty<string>();
        }
    }
}
