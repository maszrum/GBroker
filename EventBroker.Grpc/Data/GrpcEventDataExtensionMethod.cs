using Google.Protobuf;

namespace EventBroker.Grpc.Data
{
    public static class GrpcEventDataExtensionMethod
    {
        public static EventData ToGrpcMessage(this IEventData eventData)
        {
            var result = new EventData()
            {
                EventName = eventData.EventName,
                Data = ByteString.CopyFrom(eventData.GetData())
            };

            result.PropertyNames.AddRange(eventData.PropertyNames);
            result.PropertyPositions.AddRange(eventData.PropertyPositions);

            return result;
        }
    }
}
