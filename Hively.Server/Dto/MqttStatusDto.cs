namespace Hively.Server.Dto
{
    /// <summary>
    /// Current MQTT broker connection state, for the header's connection
    /// indicator. Pushed live via <see cref="Hubs.ITopicHubClient.BrokerStatusChanged"/>
    /// and available on demand via <c>GET /api/mqtt-status</c>.
    /// </summary>
    public class MqttStatusDto
    {
        public bool Connected { get; set; }
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public DateTime? ConnectedAt { get; set; }
        public string? LastDisconnectReason { get; set; }
    }
}
