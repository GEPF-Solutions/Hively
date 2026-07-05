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
        /// ingestion, or a configure/match-recompute/accept-relink/clear-violations mutation.
        /// </summary>
        Task TopicUpdated(TopicDto topic, CancellationToken cancellationToken);

        /// <summary>
        /// A topic was deleted.
        /// </summary>
        Task TopicRemoved(Guid topicId, CancellationToken cancellationToken);

        /// <summary>
        /// The MQTT broker connection went up or down (see
        /// <see cref="Services.Abstractions.IMqttStatusService"/>). Not strictly a
        /// "topic" event, but pushed through the same hub rather than standing up
        /// a second SignalR connection for one small status indicator.
        /// </summary>
        Task BrokerStatusChanged(MqttStatusDto status, CancellationToken cancellationToken);

        /// <summary>
        /// A match was created, edited, or deleted. No payload — clients just
        /// refetch their matches list, since what changed (specificity ordering,
        /// which untracked topics now match) is cheaper to recompute client-side
        /// than to model here.
        /// </summary>
        Task MatchesChanged(CancellationToken cancellationToken);
    }
}
