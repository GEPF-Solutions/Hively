using Hively.Server.DbModel;
using Hively.Server.Dto;

namespace Hively.Server.Repository.Abstractions
{
    /// <summary>
    /// Repository interface for managing Topic entities. Scoped to CRUD and
    /// relationship assignment — MQTT ingestion, the relocation-relink heuristic,
    /// and rule matching are separate, not-yet-built subsystems.
    /// </summary>
    public interface ITopicRepository
    {
        /// <summary>
        /// Retrieves all topics, with producer/schema/consumers/tags loaded.
        /// </summary>
        Task<IEnumerable<Topic>> GetTopicsAsync(CancellationToken cancellationToken);

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
        /// Updates the admin-editable fields of a topic — tracked, producer, schema,
        /// consumer/tag assignment. Does not touch ingestion-owned fields (last
        /// payload/seen-at/retained/violation count/activity histogram).
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when topic is not found.</exception>
        Task UpdateTopicAsync(TopicConfigureDto topic, CancellationToken cancellationToken);

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
    }
}
