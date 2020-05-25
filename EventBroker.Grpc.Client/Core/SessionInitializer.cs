using System;
using System.Threading.Tasks;

namespace EventBroker.Grpc.Client.Core
{
    internal class SessionInitializer : ISessionInitializer, ISessionProvider
    {
        private readonly EventsService.EventsServiceClient _grpc;
        private readonly string _serviceIdentificator;

        private readonly string _sessionId;

        public SessionInitializer(EventsService.EventsServiceClient grpc, string serviceIdentificator)
        {
            _grpc = grpc ?? throw new ArgumentNullException(nameof(grpc));
            _serviceIdentificator = serviceIdentificator ?? throw new ArgumentNullException(nameof(serviceIdentificator));

            var sessionId = Guid.NewGuid();
            _sessionId = GuidConverter.ToString(sessionId);
        }

        public string GetSessionId()
        {
            return _sessionId;
        }

        public void Initialize()
        {
            var request = GetRequest();
            _grpc.Init(request);
        }

        public async Task InitializeAsync()
        {
            var request = GetRequest();
            await _grpc.InitAsync(request);
        }

        private InitRequest GetRequest()
        {
            return new InitRequest()
            {
                ClientId = GetSessionId(),
                ClientType = _serviceIdentificator
            };
        }
    }
}
