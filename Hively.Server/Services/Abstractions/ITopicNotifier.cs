using Hively.Server.Dto;

namespace Hively.Server.Services.Abstractions
{
    /// <summary>
    /// Pushes Topic changes to connected clients over <see cref="Hubs.TopicHub"/>.
    /// Called from the service layer only, after a mutation has committed to the DB —
    /// never from a repository or controller.
    /// </summary>
    public interface ITopicNotifier
    {
        /// <summary>
        /// Notifies clients that a previously-unseen topic path was catalogued as
        /// an untracked stub.
        /// </summary>
        Task NotifyTopicUntrackedAsync(TopicDto topic, CancellationToken cancellationToken);

        /// <summary>
        /// Notifies clients that an existing topic's fields changed (last message,
        /// compliance, violation count, tracked/producer/schema/tag assignment, etc.).
        /// </summary>
        Task NotifyTopicUpdatedAsync(TopicDto topic, CancellationToken cancellationToken);

        /// <summary>
        /// Notifies clients that a topic was deleted.
        /// </summary>
        Task NotifyTopicRemovedAsync(Guid topicId, CancellationToken cancellationToken);
    }
}
