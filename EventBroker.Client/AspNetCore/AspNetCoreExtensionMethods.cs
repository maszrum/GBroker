using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace EventBroker.Client.AspNetCore
{
	public static class AspNetCoreExtensionMethods
	{
		public static IServiceCollection AddEventBrokerService(this IServiceCollection services)
		{
			services.AddHostedService<EventBrokerHostedService>();

			return services;
		}

		public static IServiceCollection AddEventsHandlers(this IServiceCollection services, Assembly fromAssembly)
		{
			var handlerTypes = fromAssembly
				.GetTypes()
				.Where(t => t.IsAssignableFrom(typeof(IEventBrokerHandler)) && !t.IsAbstract);

			foreach (var handlerType in handlerTypes)
			{
				AddEventBrokerHandler(services, handlerType);
			}

			return services;
		}

		public static IServiceCollection AddEventBrokerHandler(this IServiceCollection services, Type handlerType)
		{
			services.AddScoped(typeof(IEventBrokerHandler), handlerType);

			return services;
		}

		public static IServiceCollection AddEventBrokerHandler<THandler>(
			this IServiceCollection services) where THandler : class, IEventBrokerHandler
		{
			services.AddScoped<IEventBrokerHandler, THandler>();

			return services;
		}
	}
}
