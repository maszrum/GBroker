using System;
using System.Collections.Generic;
using EventBroker.Client.Exceptions;
using EventBroker.Client.Interceptor;

namespace EventBroker.Client
{
	public sealed class EventsClientBuilder
	{
		private readonly List<Action<EventsClientBuilder>> _onBuildingActions = new List<Action<EventsClientBuilder>>();

		private readonly HashSet<IEventsSink> _sinks = new HashSet<IEventsSink>();
		private readonly HashSet<IEventsSource> _sources = new HashSet<IEventsSource>();
		private readonly HashSet<IEventInterceptor> _interceptors = new HashSet<IEventInterceptor>();
		private readonly ExceptionsCatcher _exceptionsCatcher = new ExceptionsCatcher();

		public string ServiceIdentificator { get; private set; }
 
		public IEventBrokerClient Build()
		{
			if (string.IsNullOrWhiteSpace(ServiceIdentificator))
			{
				throw new InvalidOperationException(
					$"method {nameof(Named)} was not called");
			}


			foreach (var action in _onBuildingActions)
			{
				action(this);
			}

			var client = new EventBrokerClient(
				serviceIdentificator: ServiceIdentificator, 
				sinks: _sinks, 
				sources: _sources, 
				interceptors: _interceptors, 
				exceptionsCatcher: _exceptionsCatcher);

			return client;
		}

		public IExceptionsCatcher ExceptionsCatcher => _exceptionsCatcher;

		public IExceptionsSink ExceptionsSink => _exceptionsCatcher;

		public EventsClientBuilder Named(string serviceIdentificator)
		{
			ServiceIdentificator = serviceIdentificator;
			return this;
		}

		public EventsClientBuilder WithSink(IEventsSink sink)
		{
			_sinks.Add(sink);
			return this;
		}

		public EventsClientBuilder WithSource(IEventsSource source)
		{
			_sources.Add(source);
			return this;
		}

		public EventsClientBuilder WithInterceptor(IEventInterceptor interceptor)
		{
			_interceptors.Add(interceptor);
			return this;
		}

		public void OnBuilding(Action<EventsClientBuilder> callback)
		{
			if (callback == null)
			{
				throw new ArgumentNullException(nameof(callback));
			}
			_onBuildingActions.Add(callback);
		}
	}
}
