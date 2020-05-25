using EventBroker.Client;
using EventBroker.Grpc.Client.Core;
using EventBroker.Grpc.Client.Sink;
using EventBroker.Grpc.Client.Source;
using Grpc.Net.Client;

namespace EventBroker.Grpc.Client
{
	public static class GrpcExtensionMethods
	{
		public static EventsClientBuilder WithGrpc(
			this EventsClientBuilder builder, GrpcChannel grpcChannel)
		{
			builder.OnBuilding(clientBuilder =>
			{
				var rpcClient = new EventsService.EventsServiceClient(grpcChannel);

				var sessionInitializer = new SessionInitializer(rpcClient, clientBuilder.ServiceIdentificator);
				var grpcClient = new GrpcClientAdapter(rpcClient, sessionInitializer);

				var sink = new GrpcEventsSink(grpcClient);
				clientBuilder.WithSink(sink);

				var source = new GrpcEventsSource(grpcClient, sessionInitializer, clientBuilder.ExceptionsSink);
				clientBuilder.WithSource(source);
			});

			return builder;
		}

		public static EventsClientBuilder WithGrpc(
			this EventsClientBuilder builder, string rpcServerAddress)
		{
			var channel = GrpcChannel.ForAddress(rpcServerAddress);

			return WithGrpc(builder, channel);
		}
	}
}
