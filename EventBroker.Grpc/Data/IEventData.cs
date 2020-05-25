using System.Collections.Generic;

namespace EventBroker.Grpc.Data
{
    public interface IEventData
    {
        string EventName { get; set; }
        List<int> PropertyPositions { get; }
        List<string> PropertyNames { get; }

        void SetData(byte[] data);
        byte[] GetData();
    }
}
