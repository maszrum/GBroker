using System;
using System.Threading.Tasks;
using EventBroker.Client;
using EventBroker.Client.EventFlow;
using EventBroker.Core;
using EventBroker.Grpc.Client.Core;
using EventBroker.Grpc.Data;

namespace EventBroker.Grpc.Client.Sink
{
    internal class GrpcEventsSink : IEventsSink
    {
        private readonly IGrpcClient _grpc;

        public GrpcEventsSink(IGrpcClient grpc)
        {
            _grpc = grpc ?? throw new ArgumentNullException(nameof(grpc));
        }

        public void SendEvent<TEvent>(IPublishingState<TEvent> state) where TEvent : IEvent
        {
            var eventData = Convert(state.Event);

            var newServicesHandled = _grpc.EmitEvent(eventData, state.ServicesHandled);

            state.ServicesHandled.UnionWith(newServicesHandled);
        }

        public async Task SendEventAsync<TEvent>(IPublishingState<TEvent> state) where TEvent : IEvent
        {
            var eventData = Convert(state.Event);

            var newServicesHandled = await _grpc.EmitEventAsync(eventData, state.ServicesHandled);

            state.ServicesHandled.UnionWith(newServicesHandled);
        }

        private static IEventData Convert<TEvent>(TEvent ev) where TEvent : IEvent
        {
            return EventConverter
                .EventToData()
                .Convert(ev);
        }
    }
}
