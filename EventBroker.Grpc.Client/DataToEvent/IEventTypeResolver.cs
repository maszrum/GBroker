using System;
using System.Collections.Generic;
using System.Reflection;

namespace EventBroker.Grpc.Client.DataToEvent
{
    public interface IEventTypeResolver
    {
        Type GetEventType(string name);
        ConstructorInfo GetEventConstructor(Type type);
        PropertyInfo GetPropertyInfo(Type type, string name);
        IReadOnlyList<PropertyInfo> GetProperties(Type type);
    }
}
