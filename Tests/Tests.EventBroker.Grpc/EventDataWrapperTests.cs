using EventBroker.Grpc;
using EventBroker.Grpc.Data;
using Google.Protobuf;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc
{
    [TestFixture]
    internal class EventDataWrapperTests
    {
        [Test]
        public void constructor_test()
        {
            var eventData = new EventData()
            {
                EventName = "ASDFG",
                PropertyPositions = { 1, 5, 30 },
                PropertyNames = { "qwerty", "asdfg", "zxcvb" },
                Data = ByteString.CopyFrom(0x00, 0x10, 0x20)
            };

            var wrapper = EventDataWrapper.FromGrpcMessage(eventData);

            Assert.Multiple(() =>
            {
                Assert.That(wrapper.EventName, Is.EqualTo(eventData.EventName));
                CollectionAssert.AreEqual(eventData.PropertyPositions, wrapper.PropertyPositions);
                CollectionAssert.AreEqual(eventData.PropertyNames, wrapper.PropertyNames);
                CollectionAssert.AreEqual(eventData.Data, wrapper.GetData());
            });
        }

        [Test]
        public void set_data_get_data()
        {
            var wrapper = new EventDataWrapper();

            wrapper.SetData(new byte[] {0x10, 0x20, 0x30});

            CollectionAssert.AreEqual(
                new byte[] { 0x10, 0x20, 0x30 }, 
                wrapper.GetData());
        }
    }
}
