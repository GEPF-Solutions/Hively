using Hively.Server.DbModel;
using Hively.Server.Dto;

namespace Hively.Server.Repository.Abstractions
{
    /// <summary>
    /// Repository interface for managing Topic entities: CRUD, resolved
    /// producer/schema/consumer/tag assignment, relink acceptance, and MQTT
    /// ingestion writes.
    /// </summary>
    public interface ITopicRepository
    {
        /// <summary>
        /// Retrieves all topics, with producer/schema/consumers/tags loaded.
        /// </summary>
        Task<IEnumerable<Topic>> GetTopicsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Finds a topic by its exact path, with schema loaded, or null if no topic
        /// with that path has ever been seen. Unlike <see cref="GetTopicAsync"/>, a
        /// miss is the expected/common case for an incoming ingestion message on a
        /// brand-new path, not an error, so this does not throw.
        /// </summary>
        Task<Topic?> FindTopicByPathAsync(string path, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a single topic by ID, with producer/schema/consumers/tags loaded.
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when topic is not found.</exception>
        Task<Topic> GetTopicAsync(Guid topicId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new topic stub. Only <see cref="TopicDto.Path"/> is used —
        /// this mirrors how the (future) MQTT ingestion service creates untracked
        /// stub rows for paths never seen before; everything else stays at DB defaults.
        /// </summary>
        Task<Guid> InsertTopicAsync(TopicDto topic, CancellationToken cancellationToken);

        /// <summary>
        /// Persists a resolved producer/schema/consumer/tag assignment (the output of
        /// <see cref="Services.MatchResolver.Resolve"/>) onto a topic. <paramref name="tracked"/>
        /// is <c>null</c> to leave the flag untouched (a Match-deletion recompute never
        /// tracks/untracks anything), or an explicit value to set it (a direct admin
        /// edit always pins it; a Match insert/update sweep or first-sighting auto-apply
        /// always sets it <c>true</c>). Does not touch ingestion-owned fields (last
        /// payload/seen-at/retained/violation count/activity histogram).
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when topic is not found.</exception>
        Task ApplyResolvedAssignmentAsync(
            Guid topicId,
            Guid? producerId,
            Guid? schemaId,
            List<Guid> consumerIds,
            List<string> tagIds,
            bool? tracked,
            CancellationToken cancellationToken);

        /// <summary>
        /// Resets the violation counter and stamps LastClearedAt to now.
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when topic is not found.</exception>
        Task ClearViolationsAsync(Guid topicId, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a topic and its consumer/tag associations (ON DELETE CASCADE).
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when topic is not found.</exception>
        Task RemoveTopicAsync(Guid topicId, CancellationToken cancellationToken);

        /// <summary>
        /// Marks the target tracked, copies violation count/LastClearedAt from
        /// <paramref name="oldTopicId"/> (producer/schema/tags are not copied here —
        /// the caller re-keys the old topic's private Match, if any, and re-resolves
        /// via <see cref="ApplyResolvedAssignmentAsync"/> instead), then retires the
        /// old topic (stamps RetiredAt/MergedIntoTopicId) rather than deleting it.
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when either topic is not found.</exception>
        Task AcceptRelinkAsync(Guid topicId, Guid oldTopicId, CancellationToken cancellationToken);

        /// <summary>
        /// Persists the fields owned exclusively by MQTT ingestion — last payload,
        /// last-seen timestamp, retained flag, violation count, and activity
        /// histogram — for an already-existing topic. The caller (ingestion service)
        /// computes all of these values; this just writes them.
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when topic is not found.</exception>
        Task RecordIngestedMessageAsync(
            Guid topicId,
            string payloadJson,
            DateTime seenAtUtc,
            bool retained,
            int violationCount,
            string activityHistogramJson,
            CancellationToken cancellationToken);
    }
}
