using EventBroker.Grpc.Data;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc
{
    [TestFixture]
    internal class GrpcEventDataExtensionMethodTests
    {
        [Test]
        public void convert_test()
        {
            var wrapper = new EventDataWrapper()
            {
                EventName = "asdfg",
                PropertyPositions = {1, 5, 8},
                PropertyNames = {"qwe", "asd", "zxc"}
            };
            wrapper.SetData(new byte[] { 0x10, 0x20, 0x30 });

            var eventData = wrapper.ToGrpcMessage();

            Assert.Multiple(() =>
            {
                Assert.That(eventData.EventName, Is.EqualTo(wrapper.EventName));
                CollectionAssert.AreEqual(wrapper.PropertyPositions, eventData.PropertyPositions);
                CollectionAssert.AreEqual(wrapper.PropertyNames, eventData.PropertyNames);
                CollectionAssert.AreEqual(wrapper.GetData(), eventData.Data);
            });
        }
    }
}