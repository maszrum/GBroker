using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventBroker.Core;
using EventBroker.Grpc.Data;
using Grpc.Core;

namespace EventBroker.Grpc.Client.Core
{
	internal class GrpcClientAdapter : IGrpcClient
	{
		private readonly EventsService.EventsServiceClient _rpcClient;
		private readonly ISessionProvider _sessionProvider;

		public GrpcClientAdapter(
			EventsService.EventsServiceClient rpcClient, ISessionProvider sessionProvider)
		{
			_rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
			_sessionProvider = sessionProvider ?? throw new ArgumentNullException(nameof(sessionProvider));
		}

		private string SessionId => _sessionProvider.GetSessionId();

		public void Subscribe(string eventName, ConsumptionType consumptionType)
		{
			var request = new SubscribeRequest()
			{
				SessionId = SessionId,
				Subscription = new SubscriptionData()
				{
					EventName = eventName,
					Type = ConvertConsumptionType(consumptionType)
				}
			};

			_rpcClient.Subscribe(request);
		}

		public async Task SubscribeAsync(string eventName, ConsumptionType consumptionType)
		{
			var request = new SubscribeRequest()
			{
				SessionId = SessionId,
				Subscription = new SubscriptionData()
				{
					EventName = eventName,
					Type = ConvertConsumptionType(consumptionType)
				}
			};

			await _rpcClient.SubscribeAsync(request);
		}

		public void SubscribeMany(IEnumerable<(string, ConsumptionType)> subscriptions)
		{
			var request = new SubscribeManyRequest()
			{
				SessionId = SessionId
			};

			var subscriptionsData = subscriptions
				.Select(s => new SubscriptionData()
				{
					EventName = s.Item1,
					Type = ConvertConsumptionType(s.Item2)
				});
			request.Subscriptions.AddRange(subscriptionsData);

			_rpcClient.SubscribeMany(request);
		}
		 
		public async Task SubscribeManyAsync(IEnumerable<(string, ConsumptionType)> subscriptions)
		{
			var request = new SubscribeManyRequest()
			{
				SessionId = SessionId
			};

			var subscriptionsData = subscriptions
				.Select(s => new SubscriptionData()
				{
					EventName = s.Item1,
					Type = ConvertConsumptionType(s.Item2)
				});
			request.Subscriptions.AddRange(subscriptionsData);

			await _rpcClient.SubscribeManyAsync(request);
		}

		public void Unsubscribe(string eventName)
		{
			var request = new UnsubscribeRequest()
			{
				SessionId = SessionId,
				EventName = eventName
			};

			_rpcClient.Unsubscribe(request);
		}

		public async Task UnsubscribeAsync(string eventName)
		{
			var request = new UnsubscribeRequest()
			{
				SessionId = SessionId,
				EventName = eventName
			};

			await _rpcClient.UnsubscribeAsync(request);
		}

		public string[] EmitEvent(IEventData eventData, HashSet<string> servicesHandled)
		{
			var request = new EmitEventRequest()
			{
				SessionId = SessionId,
				EventData = eventData.ToGrpcMessage(),
				ServicesHandled = { servicesHandled }
			};

			var response = _rpcClient.EmitEvent(request);

			var result = response.ServicesHandled
				.Where(s => !servicesHandled.Contains(s))
				.ToArray();

			return result;
		}

		public async Task<string[]> EmitEventAsync(IEventData eventData, HashSet<string> servicesHandled)
		{
			var request = new EmitEventRequest()
			{
				SessionId = SessionId,
				EventData = eventData.ToGrpcMessage(),
				ServicesHandled = { servicesHandled }
			};

			var response = await _rpcClient.EmitEventAsync(request);

			var result = response.ServicesHandled
				.Where(s => !servicesHandled.Contains(s))
				.ToArray();

			return result;
		}

		public IAsyncEnumerable<IEventData> Listen(CancellationToken cancellationToken = default)
		{
			var request = new ListenRequest()
			{
				SessionId = SessionId
			};
			var call = _rpcClient.ListenForEvents(request);

			return call.ResponseStream.ReadAllAsync(cancellationToken)
				.Select(EventDataWrapper.FromGrpcMessage);
		}

		private static SubscriptionData.Types.ConsumptionType ConvertConsumptionType(ConsumptionType type)
			=> type switch
			{
				ConsumptionType.ConsumeAll => SubscriptionData.Types.ConsumptionType.ConsumeAll,
				ConsumptionType.OneEventPerServiceType => SubscriptionData.Types.ConsumptionType.OneEventPerServiceType,
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			};
	}
}
