using Hively.Server.Dto;

namespace Hively.Server.Services.Abstractions
{
    /// <summary>
    /// Tracks the MQTT broker connection's current state (for the header's
    /// connection indicator) and pushes changes to clients over
    /// <see cref="Hubs.TopicHub"/>. Updated by <see cref="Ingestion.MqttIngestionService"/>'s
    /// connect/disconnect/connecting-failed handlers.
    /// </summary>
    public interface IMqttStatusService
    {
        /// <summary>Current snapshot, for the initial <c>GET /api/mqtt-status</c> fetch.</summary>
        MqttStatusDto GetStatus();

        /// <summary>Records a successful (re)connection and notifies clients.</summary>
        Task SetConnectedAsync(CancellationToken cancellationToken);

        /// <summary>Records a disconnect or failed connection attempt and notifies clients.</summary>
        Task SetDisconnectedAsync(string reason, CancellationToken cancellationToken);
    }
}
