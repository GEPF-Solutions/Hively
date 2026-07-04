using Hively.Server.Dto;

namespace Hively.Server.Hubs
{
    /// <summary>
    /// Strongly-typed client methods pushed by <see cref="TopicHub"/>. Implemented
    /// on the frontend's hub connection, never called directly by a controller —
    /// only services push through <see cref="Services.Abstractions.ITopicNotifier"/>.
    /// </summary>
    public interface ITopicHubClient
    {
        /// <summary>
        /// A previously-unseen topic path was observed and catalogued as an
        /// untracked stub (MQTT ingestion's first sighting, or the manual stub-insert endpoint).
        /// </summary>
        Task TopicUntracked(TopicDto topic, CancellationToken cancellationToken);

        /// <summary>
        /// An existing topic's fields changed — last message/compliance from
        /// ingestion, or a configure/apply-rule/accept-relink/clear-violations mutation.
        /// </summary>
        Task TopicUpdated(TopicDto topic, CancellationToken cancellationToken);

        /// <summary>
        /// A topic was deleted.
        /// </summary>
        Task TopicRemoved(Guid topicId, CancellationToken cancellationToken);
    }
}
