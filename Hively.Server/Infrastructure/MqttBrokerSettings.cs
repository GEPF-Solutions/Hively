namespace Hively.Server.Infrastructure
{
    /// <summary>
    /// Strongly-typed settings for connecting to the MQTT broker, bound from the
    /// "MqttBroker" configuration section (env vars in a container: MqttBroker__Host, etc.).
    /// </summary>
    public class MqttBrokerSettings
    {
        public required string Host { get; init; }
        public required int Port { get; init; }
        public required string ClientId { get; init; }
        public string? Username { get; init; }
        public string? Password { get; init; }
        public required bool UseTls { get; init; }
    }
}
