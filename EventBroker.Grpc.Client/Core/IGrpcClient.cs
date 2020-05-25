using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventBroker.Core;
using EventBroker.Grpc.Data;

namespace EventBroker.Grpc.Client.Core
{
    internal interface IGrpcClient
    {
        void Subscribe(string eventName, ConsumptionType consumptionType);
        Task SubscribeAsync(string eventName, ConsumptionType consumptionType);
        void SubscribeMany(IEnumerable<(string, ConsumptionType)> subscriptions);
        Task SubscribeManyAsync(IEnumerable<(string, ConsumptionType)> subscriptions);
        void Unsubscribe(string eventName);
        Task UnsubscribeAsync(string eventName);
        string[] EmitEvent(IEventData eventData, HashSet<string> servicesHandled);
        Task<string[]> EmitEventAsync(IEventData eventData, HashSet<string> servicesHandled);
        IAsyncEnumerable<IEventData> Listen(CancellationToken cancellationToken = default);
    }
}
