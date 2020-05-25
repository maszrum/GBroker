using System;
using EventBroker.Grpc.Data;

namespace EventBroker.Grpc.Server.Sessions
{
    internal interface ISession
    {
        Guid Id { get; }
        string ServiceType { get; }
        void FeedData(IEventData eventData);
    }
}