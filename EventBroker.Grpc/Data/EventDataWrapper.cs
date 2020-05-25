using System.Collections.Generic;
using System.Linq;

namespace EventBroker.Grpc.Data
{
    public class EventDataWrapper : IEventData
    {
        private byte[] _data;

        public EventDataWrapper()
        {
            PropertyPositions = new List<int>();
            PropertyNames = new List<string>();
        }

        private EventDataWrapper(EventData eventData)
        {
            EventName = eventData.EventName;
            PropertyPositions = eventData.PropertyPositions.ToList();
            PropertyNames = eventData.PropertyNames.ToList();
            _data = eventData.Data.ToByteArray();
        }

        public string EventName { get; set; }
        public List<int> PropertyPositions { get; }
        public List<string> PropertyNames { get; }

        public void SetData(byte[] data)
        {
            _data = data;
        }

        public byte[] GetData()
        {
            return _data;
        }

        public static EventDataWrapper FromGrpcMessage(EventData eventData)
        {
            return new EventDataWrapper(eventData);
        }
    }
}
