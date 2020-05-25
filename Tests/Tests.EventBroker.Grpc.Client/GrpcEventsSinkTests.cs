using System.Collections.Generic;
using System.Threading.Tasks;
using EventBroker.Client.EventFlow;
using EventBroker.Core;
using EventBroker.Grpc.Client.Core;
using EventBroker.Grpc.Client.Sink;
using EventBroker.Grpc.Data;
using Moq;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc.Client
{
    internal class GrpcEventsSinkTests
    {
        public class MockEvent : IEvent
        {
        }

        [Test]
        public void check_if_send_event_calls_method_in_client()
        {
            var client = new Mock<IGrpcClient>();

            var sink = new GrpcEventsSink(client.Object);

            var state = new PublishingState<MockEvent>(new MockEvent());
            sink.SendEvent(state);

            client.Verify(
                m => m.EmitEvent(It.IsAny<IEventData>(), It.IsAny<HashSet<string>>()), 
                Times.Once);
        }

        [Test]
        public async Task check_if_send_event_async_calls_method_in_client()
        {
            var client = new Mock<IGrpcClient>();

            var sink = new GrpcEventsSink(client.Object);

            var state = new PublishingState<MockEvent>(new MockEvent());

            await sink.SendEventAsync(state);

            client.Verify(
                m => m.EmitEventAsync(It.IsAny<IEventData>(), It.IsAny<HashSet<string>>()),
                Times.Once);
        }
    }
}
