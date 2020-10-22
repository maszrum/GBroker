using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EventBroker.Core;
using EventBroker.Grpc.Data;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace EventBroker.Grpc.Server
{
    public class EventBrokerGrpcService : EventsService.EventsServiceBase
    {
        private readonly ILogger<EventBrokerGrpcService> _logger;
        private readonly IServer _server;

        public EventBrokerGrpcService(ILogger<EventBrokerGrpcService> logger, IServer server)
        {
            _logger = logger;
            _server = server ?? throw new ArgumentNullException(nameof(server));
        }

        public override Task<InitResponse> Init(
            InitRequest request, ServerCallContext context)
        {
            var sessionId = GuidConverter.Parse(request.ClientId);
            _server.InitSession(sessionId, request.ClientType);

            _logger.LogInformation(
                "Client has connected ({ClientType}, {SessionId})", request.ClientType, sessionId);

            var response = new InitResponse();
            return Task.FromResult(response);
        }

        public override Task<EmitEventAcknowledgement> EmitEvent(
            EmitEventRequest request, ServerCallContext context)
        {
            var sessionId = GuidConverter.Parse(request.SessionId);
            var eventDataWrapper = EventDataWrapper.FromGrpcMessage(request.EventData);

            var newServicesHandled = _server.FeedEventData(sessionId, eventDataWrapper, request.ServicesHandled);

            var allServicesHandled = request.ServicesHandled
                .Concat(newServicesHandled)
                .Distinct();

            var response = new EmitEventAcknowledgement()
            {
                ServicesHandled = { allServicesHandled }
            };
            return Task.FromResult(response);
        }

        public override Task<SubscribeResponse> Subscribe(
            SubscribeRequest request, ServerCallContext context)
        {
            var sessionId = GuidConverter.Parse(request.SessionId);

            var consumptionType = ConvertConsumptionType(request.Subscription.Type);
            _server.CreateSubscription(sessionId, request.Subscription.EventName, consumptionType);

            var response = new SubscribeResponse();
            return Task.FromResult(response);
        }

        public override Task<SubscribeResponse> SubscribeMany(
            SubscribeManyRequest request, ServerCallContext context)
        {
            var sessionId = GuidConverter.Parse(request.SessionId);

            foreach (var subscriptionData in request.Subscriptions)
            {
                var consumptionType = ConvertConsumptionType(subscriptionData.Type);
                _server.CreateSubscription(sessionId, subscriptionData.EventName, consumptionType);
            }

            var response = new SubscribeResponse();
            return Task.FromResult(response);
        }

        public override Task<UnsubscribeResponse> Unsubscribe(
            UnsubscribeRequest request, ServerCallContext context)
        {
            var sessionId = GuidConverter.Parse(request.SessionId);

            _server.RemoveSubscription(sessionId, request.EventName);

            var response = new UnsubscribeResponse();
            return Task.FromResult(response);
        }

        public override async Task ListenForEvents(
            ListenRequest request, IServerStreamWriter<EventData> responseStream, ServerCallContext context)
        {
            var sessionId = GuidConverter.Parse(request.SessionId);

            var events = _server.ListenForEvents(sessionId);

            await foreach (var eventData in events)
            {
                var output = eventData.ToGrpcMessage();

                try
                {
                    await responseStream.WriteAsync(output);
                }
                catch (IOException ioe)
                {
                    _server.RemoveSession(sessionId);

                    _logger.LogWarning(
                        ioe, "Client has disconnected ({SessionId})", sessionId);
                }
            }
        }

        private static ConsumptionType ConvertConsumptionType(SubscriptionData.Types.ConsumptionType input)
            => input switch
            {
                SubscriptionData.Types.ConsumptionType.ConsumeAll => ConsumptionType.ConsumeAll,
                SubscriptionData.Types.ConsumptionType.OneEventPerServiceType => ConsumptionType.OneEventPerServiceType,
                _ => throw new ArgumentOutOfRangeException(nameof(input), input, null)
            };
    }
}
