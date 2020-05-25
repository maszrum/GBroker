using System;
using EventBroker.Grpc;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc
{
    [TestFixture]
    internal class GuidConverterTests
    {
        [Test]
        public void guid_converter_test()
        {
            var guid = Guid.NewGuid();

            var guidString = GuidConverter.ToString(guid);
            var guidParsed = GuidConverter.Parse(guidString);

            Assert.That(guidParsed, Is.EqualTo(guid));
        }

    }
}
