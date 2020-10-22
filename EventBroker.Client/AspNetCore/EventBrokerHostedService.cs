using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EventBroker.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventBroker.Client.AspNetCore
{
    internal class EventBrokerHostedService : IHostedService
    {
        private readonly Func<IServiceScope> _scopeFactory;
        private IServiceScope _scope;

        public EventBrokerHostedService(IServiceProvider serviceProvider)
        {
            _scopeFactory = serviceProvider.CreateScope;
        }

        private IServiceProvider Services
        {
            get
            {
                if (_scope == null)
                {
                    _scope = _scopeFactory();
                }
                return _scope.ServiceProvider;
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var consumer = Services.GetRequiredService<IEventsConsumer>();
            var handlers = Services.GetServices<IEventBrokerHandler>();

            foreach (var handler in handlers)
            {
                var consumptionType = GetHandlerConsumptionType(handler);
                handler.Setup(consumer, consumptionType);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_scope != null)
            {
                _scope.Dispose();
                _scope = null;
            }

            return Task.CompletedTask;
        }

        private static ConsumptionType GetHandlerConsumptionType(IEventBrokerHandler handler)
        {
            var handlerType = handler.GetType();
            var attribute = handlerType.GetCustomAttribute<ConsumeEventsAttribute>();

            if (attribute == null)
            {
                throw new ApplicationException(
                    $"event broker handler of type {handlerType.Name} has no defined {nameof(ConsumeEventsAttribute)} attribute");
            }

            return attribute.ConsumptionType;
        }
    }
}