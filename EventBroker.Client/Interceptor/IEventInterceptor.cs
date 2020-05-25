using System;
using System.Threading.Tasks;
using EventBroker.Core;

namespace EventBroker.Client.Interceptor
{
    public interface IEventInterceptor
    {
        TEvent InterceptIncoming<TEvent>(TEvent ev, Type sourceType) where TEvent : IEvent;
        TEvent InterceptOutgoing<TEvent>(TEvent ev) where TEvent : IEvent;
        Task<TEvent> InterceptOutgoingAsync<TEvent>(TEvent ev) where TEvent : IEvent;
    }
}
