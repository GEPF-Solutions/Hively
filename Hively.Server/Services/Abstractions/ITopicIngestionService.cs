namespace Hively.Server.Services.Abstractions
{
    /// <summary>
    /// Service for recording MQTT messages against the topic they were published to.
    /// Called from the MQTT ingestion hosted service, never from a controller.
    /// </summary>
    public interface ITopicIngestionService
    {
        /// <summary>
        /// Upserts the matching topic for an incoming message: creates an untracked
        /// stub if the path has never been seen, runs schema compliance validation
        /// (incrementing the violation counter on failure), and rolls the activity
        /// histogram forward.
        /// </summary>
        Task IngestMessageAsync(
            string topicPath,
            string rawPayload,
            bool retained,
            DateTime receivedAtUtc,
            CancellationToken cancellationToken);
    }
}
