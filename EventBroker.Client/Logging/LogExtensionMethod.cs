using Microsoft.Extensions.Logging;

namespace EventBroker.Client.Logging
{
    public static class LogExtensionMethod
    {
        public static EventsClientBuilder WithLogger(this EventsClientBuilder builder,
            ILogger<IEventBrokerClient> logger)
        {
            var interceptor = new LoggerInterceptor(logger);
            builder.WithInterceptor(interceptor);

            builder.OnBuilding(client =>
            {

                client.ExceptionsCatcher.OnException(exception =>
                {
                    interceptor.LogException(exception);
                });
            });

            return builder;
        }
    }
}
