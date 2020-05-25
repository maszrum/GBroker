using System.Threading.Tasks;
using EventBroker.Core;

namespace EventBroker.Client
{
    public interface IEventsProducer
    {
        void Publish<TEvent>(TEvent e) where TEvent : IEvent;
        Task PublishAsync<TEvent>(TEvent e) where TEvent : IEvent;
    }
}
