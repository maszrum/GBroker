using System;
using System.Collections.Generic;
using EventBroker.Core;
using EventBroker.Grpc.Data;

namespace EventBroker.Grpc.Server
{
    public interface IServer
    {
        void InitSession(Guid sessionId, string serviceIdentificator);
        void RemoveSession(Guid sessionId);
        string[] FeedEventData(Guid sessionId, IEventData eventData, IEnumerable<string> servicesHandled);
        void CreateSubscription(Guid sessionId, string eventName, ConsumptionType consumptionType);
        void RemoveSubscription(Guid sessionId, string eventName);
        IAsyncEnumerable<IEventData> ListenForEvents(Guid sessionId);
    }
}
