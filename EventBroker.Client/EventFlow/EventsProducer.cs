using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventBroker.Client.Interceptor;
using EventBroker.Core;

namespace EventBroker.Client.EventFlow
{
    internal class EventsProducer : IEventsProducer
    {
        private readonly HashSet<IEventsSink> _sinks;
        private readonly HashSet<IEventInterceptor> _interceptors;

        public EventsProducer(IEnumerable<IEventsSink> sinks, IEnumerable<IEventInterceptor> interceptors)
        {
            _sinks = sinks.ToHashSet();
            _interceptors = interceptors.ToHashSet();
        }

        public void Publish<TEvent>(TEvent e) where TEvent : IEvent
        {
            e = _interceptors
                .Aggregate(e, (current, interceptor) => 
                    interceptor.InterceptOutgoing(current));

            var state = new PublishingState<TEvent>(e);

            foreach (var sink in _sinks)
            {
                sink.SendEvent(state);
            }
        }

        public async Task PublishAsync<TEvent>(TEvent e) where TEvent : IEvent
        {
            foreach (var interceptor in _interceptors)
            {
                var intercepted = await interceptor.InterceptOutgoingAsync(e);
                e = intercepted;
            }

            var state = new PublishingState<TEvent>(e);

            foreach (var sink in _sinks)
            {
                await sink.SendEventAsync(state);
            }
        }
    }
}
