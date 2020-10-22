using System;
using EventBroker.Client;
using EventBroker.Grpc.Client;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Sample.EventBroker.Api;

namespace Tests.EventBroker.Integration.Core
{
    internal class FunctionalTestBase
    {
        private IDisposable _testContext;

        protected GrpcTestFixture<Startup> Fixture { get; private set; }

        protected ILoggerFactory LoggerFactory => Fixture.LoggerFactory;

        protected GrpcChannel CreateChannel()
        {
            var httpClient = Fixture.CreateClient();

            return GrpcChannel.ForAddress(httpClient.BaseAddress, new GrpcChannelOptions
            {
                LoggerFactory = LoggerFactory,
                HttpClient = httpClient
            });
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Fixture = new GrpcTestFixture<Startup>(ConfigureServices);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Fixture.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            _testContext = Fixture.GetTestContext();
        }

        [TearDown]
        public void TearDown()
        {
            _testContext?.Dispose();
        }

        protected IEventBrokerClient CreateClient(string serviceId)
        {
            var channel = CreateChannel();

            var eventBrokerClient = new EventsClientBuilder()
                .Named(serviceId)
                .WithGrpc(channel)
                .Build();

            return eventBrokerClient;
        }
    }
}
