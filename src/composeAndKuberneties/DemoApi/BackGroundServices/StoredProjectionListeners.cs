using DemoApi.Controllers;
using DemoApi.Domain;
using EventStore.ClientAPI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApi.BackGroundServices
{
    public class StoredProjectionListeners : BackgroundService
    {
        private readonly ILogger<StoredProjectionListeners> _logger;

        public StoredProjectionListeners(ILogger<StoredProjectionListeners> logger)
        {
            _logger = logger;
        }

        private IEventStoreConnection GetEventStoreConnection
        {
            get
            {
                //    var conn = EventStoreConnection.Create(uri); not working on local host with tls
                var uri = new Uri(EnvVariable.GetValue(EnvVariable.EventStore, _logger));
                var settings = ConnectionSettings
                    .Create()
                    .KeepReconnecting()
                    .DisableTls()
                    .Build();

                var con = EventStoreConnection.Create(settings, uri);
                con.Connected += Conn_Connected;
                con.AuthenticationFailed += Conn_AuthenticationFailed;
                con.Closed += Conn_Closed;
                return con;
            }
        }

        private void Conn_Closed(object sender, ClientClosedEventArgs e)
        {
            _logger.LogInformation("Conn_Closed! " + e.Reason);
        }

        private void Conn_AuthenticationFailed(object sender, ClientAuthenticationFailedEventArgs e)
        {
            _logger.LogInformation("AuthenticationFailed!");
        }

        private void Conn_Connected(object sender, ClientConnectionEventArgs e)
        {
            _logger.LogInformation("Connected to store!");
        }

        public Task Process(EventStoreSubscription sub, ResolvedEvent e)
        {
            _logger.LogInformation($"StreamId={sub.StreamId},SubscriptionName={sub.StreamId}");
            var x = $"Data={UTF8Encoding.UTF8.GetString(e.Event.Data)},Type={e.Event.EventType},StreamId={e.OriginalStreamId}";

            _logger.LogInformation(x);
            return Task.CompletedTask;
        }

        //hm if I want to reconnect how will it work
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Started StoredProjectionListeners");

            var settings = new CatchUpSubscriptionSettings(
                maxLiveQueueSize: 10000,
                readBatchSize: 10,
                verboseLogging: false,
                resolveLinkTos: true,
                subscriptionName: $"{nameof(PersonaDetail)}-Subscription"
            );

            using var connection = GetEventStoreConnection;
            await connection.ConnectAsync();

            connection.SubscribeToStreamFrom($"$ce-{nameof(PersonaDetail)}", StreamPosition.Start, settings,
                eventAppeared: (s, e) =>
                {
                    try
                    {
                        _logger.LogInformation($"Event {e.Event.EventType}, data:{UTF8Encoding.UTF8.GetString(e.Event.Data)}");
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "Failed!");
                    }

                    return Task.CompletedTask;
                },
                liveProcessingStarted: s => _logger.LogInformation($"Live processing started:{s.StreamId},{s.SubscriptionName}"),
                subscriptionDropped: (s, r, ex)
                    => _logger.LogError(ex, $"Subscription dropped: {s.StreamId},{s.SubscriptionName}. Reason:{r}")
                );

            await Task.Delay(Timeout.Infinite, stoppingToken);
            _logger.LogInformation("Exited StoredProjectionListeners");
        }
    }
}