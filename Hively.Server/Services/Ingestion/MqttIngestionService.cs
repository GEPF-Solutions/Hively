using System.Text;
using Hively.Server.Infrastructure;
using Hively.Server.Services.Abstractions;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace Hively.Server.Services.Ingestion
{
    /// <summary>
    /// Background hosted service that connects to the MQTT broker, subscribes to
    /// every topic (#), and records each incoming message via
    /// <see cref="ITopicIngestionService"/>. The client is a singleton, so a new DI
    /// scope is created per message to resolve the scoped ingestion service.
    /// </summary>
    public class MqttIngestionService : BackgroundService
    {
        private readonly MqttBrokerSettings _settings;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMqttStatusService _statusService;
        private readonly ILogger<MqttIngestionService> _logger;
        private IManagedMqttClient? _client;

        public MqttIngestionService(
            IOptions<MqttBrokerSettings> settings,
            IServiceScopeFactory scopeFactory,
            IMqttStatusService statusService,
            ILogger<MqttIngestionService> logger)
        {
            _settings = settings.Value;
            _scopeFactory = scopeFactory;
            _statusService = statusService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new MqttFactory();
            _client = factory.CreateManagedMqttClient();

            _client.ConnectedAsync += async _ =>
            {
                _logger.LogInformation("Connected to MQTT broker {Host}:{Port}.", _settings.Host, _settings.Port);
                await _statusService.SetConnectedAsync(CancellationToken.None);
            };
            _client.DisconnectedAsync += async args =>
            {
                _logger.LogWarning("Disconnected from MQTT broker {Host}:{Port}: {Reason}.", _settings.Host, _settings.Port, args.Reason);
                await _statusService.SetDisconnectedAsync(args.Reason.ToString(), CancellationToken.None);
            };
            _client.ConnectingFailedAsync += async args =>
            {
                _logger.LogWarning(args.Exception, "Failed to connect to MQTT broker {Host}:{Port}.", _settings.Host, _settings.Port);
                await _statusService.SetDisconnectedAsync(args.Exception?.Message ?? "connection failed", CancellationToken.None);
            };
            _client.ApplicationMessageReceivedAsync += HandleMessageReceivedAsync;

            var clientOptionsBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(_settings.Host, _settings.Port)
                .WithClientId(_settings.ClientId);

            if (!string.IsNullOrEmpty(_settings.Username))
            {
                clientOptionsBuilder = clientOptionsBuilder.WithCredentials(_settings.Username, _settings.Password);
            }

            if (_settings.UseTls)
            {
                clientOptionsBuilder = clientOptionsBuilder.WithTlsOptions(o => o.UseTls());
            }

            var managedOptions = new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(clientOptionsBuilder.Build())
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .Build();

            await _client.StartAsync(managedOptions);
            await _client.SubscribeAsync(new[] { new MqttTopicFilterBuilder().WithTopic("#").Build() });

            // StartAsync only kicks off the managed client's own background
            // maintenance loop — block here until shutdown so ExecuteAsync doesn't
            // return (and get treated as "done") immediately.
            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task HandleMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            var topicPath = args.ApplicationMessage.Topic;

            try
            {
                var payload = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);
                var retained = args.ApplicationMessage.Retain;

                using var scope = _scopeFactory.CreateScope();
                var ingestionService = scope.ServiceProvider.GetRequiredService<ITopicIngestionService>();
                await ingestionService.IngestMessageAsync(topicPath, payload, retained, DateTime.UtcNow, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to ingest MQTT message on topic {Topic}.", topicPath);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_client != null)
            {
                await _client.StopAsync();
                _client.Dispose();
            }

            await base.StopAsync(cancellationToken);
        }
    }
}
