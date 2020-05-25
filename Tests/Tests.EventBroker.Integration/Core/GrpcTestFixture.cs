using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Tests.EventBroker.Integration.Core
{
	internal delegate void LogMessage(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception exception);

	internal sealed class GrpcTestFixture<TStartup> : IDisposable where TStartup : class
	{
		private readonly TestServer _server;
		private readonly IHost _host;
		private readonly List<HttpClient> _clients = new List<HttpClient>();

		public event LogMessage LoggedMessage;

		public GrpcTestFixture() : this(null) { }

		public GrpcTestFixture(Action<IServiceCollection> initialConfigureServices)
		{
			LoggerFactory = new LoggerFactory();
			LoggerFactory.AddProvider(new ForwardingLoggerProvider((logLevel, category, eventId, message, exception) =>
			{
				LoggedMessage?.Invoke(logLevel, category, eventId, message, exception);
			}));

			var builder = new HostBuilder()
				.ConfigureServices(services =>
				{
					initialConfigureServices?.Invoke(services);
					services.AddSingleton<ILoggerFactory>(LoggerFactory);
				})
				.ConfigureWebHostDefaults(webHost =>
				{
					webHost
						.UseTestServer()
						.UseStartup<TStartup>();
				});
			_host = builder.Start();
			_server = _host.GetTestServer();
		}

		public HttpClient CreateClient()
		{
			var responseVersionHandler = new ResponseVersionHandler()
			{
				InnerHandler = _server.CreateHandler()
			};

			var client = new HttpClient(responseVersionHandler)
			{
				BaseAddress = new Uri("http://localhost")
			};

			_clients.Add(client);

			return client;
		}

		public LoggerFactory LoggerFactory { get; }

		public void Dispose()
		{
			foreach (var client in _clients)
			{
				client.Dispose();
			}
			_host.Dispose();
			_server.Dispose();
		}

		private class ResponseVersionHandler : DelegatingHandler
		{
			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				var response = await base.SendAsync(request, cancellationToken);
				response.Version = request.Version;

				return response;
			}
		}

		public IDisposable GetTestContext()
		{
			return new GrpcTestContext<TStartup>(this);
		}
	}
}
