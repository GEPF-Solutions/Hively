using Hively.Server.Dto;
using Hively.Server.Hubs;
using Hively.Server.Infrastructure;
using Hively.Server.Services.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace Hively.Server.Services
{
    /// <summary>
    /// Singleton (registered once, shared across all requests and the
    /// singleton <see cref="Ingestion.MqttIngestionService"/> background service) —
    /// holds the current broker connection snapshot behind a lock since it's
    /// written from the MQTT client's own callback thread and read from
    /// request threads concurrently.
    /// </summary>
    public class MqttStatusService : IMqttStatusService
    {
        private readonly IHubContext<TopicHub, ITopicHubClient> _hubContext;
        private readonly MqttBrokerSettings _settings;
        private readonly object _lock = new();

        private bool _connected;
        private DateTime? _connectedAt;
        private string? _lastDisconnectReason;

        public MqttStatusService(IHubContext<TopicHub, ITopicHubClient> hubContext, IOptions<MqttBrokerSettings> settings)
        {
            _hubContext = hubContext;
            _settings = settings.Value;
        }

        /// <inheritdoc />
        public MqttStatusDto GetStatus()
        {
            lock (_lock)
            {
                return new MqttStatusDto
                {
                    Connected = _connected,
                    Host = _settings.Host,
                    Port = _settings.Port,
                    ConnectedAt = _connectedAt,
                    LastDisconnectReason = _lastDisconnectReason,
                };
            }
        }

        /// <inheritdoc />
        public Task SetConnectedAsync(CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                _connected = true;
                _connectedAt = DateTime.UtcNow;
                _lastDisconnectReason = null;
            }

            return _hubContext.Clients.All.BrokerStatusChanged(GetStatus(), cancellationToken);
        }

        /// <inheritdoc />
        public Task SetDisconnectedAsync(string reason, CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                _connected = false;
                _connectedAt = null;
                _lastDisconnectReason = reason;
            }

            return _hubContext.Clients.All.BrokerStatusChanged(GetStatus(), cancellationToken);
        }
    }
}
