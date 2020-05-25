using System;

namespace EventBroker.Grpc.Client.Source
{
#pragma warning disable CA1032
    public class EventDataProcessingException : Exception
#pragma warning restore CA1032
    {
        public EventDataProcessingException(string message) : base(message)
        {
        }

        public EventDataProcessingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
