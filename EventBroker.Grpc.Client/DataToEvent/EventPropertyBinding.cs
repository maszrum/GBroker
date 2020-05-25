using System.Reflection;

namespace EventBroker.Grpc.Client.DataToEvent
{
    internal class EventPropertyBinding
    {
        public EventPropertyBinding(PropertyInfo property, object value)
        {
            Property = property;
            Value = value;
        }

        public PropertyInfo Property { get; }
        public object Value { get; }
    }
}
