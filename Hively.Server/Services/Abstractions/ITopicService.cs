using Hively.Server.Dto;

namespace Hively.Server.Services.Abstractions
{
    /// <summary>
    /// Service interface for Topic business logic, including compliance computation.
    /// </summary>
    public interface ITopicService
    {
        /// <summary>
        /// Retrieves all topics, each with compliance computed against its assigned schema.
        /// </summary>
        Task<IEnumerable<TopicDto>> GetTopicsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a single topic by ID, with compliance computed against its assigned schema.
        /// </summary>
        Task<TopicDto> GetTopicAsync(Guid topicId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new topic stub (see <see cref="Repository.Abstractions.ITopicRepository.InsertTopicAsync"/>).
        /// </summary>
        Task<TopicDto> InsertTopicAsync(TopicDto topic, CancellationToken cancellationToken);

        /// <summary>
        /// Updates a topic's tracked/producer/schema/consumer/tag assignment (the "Configure Topic" flow).
        /// </summary>
        Task<TopicDto> UpdateTopicAsync(TopicConfigureDto topic, CancellationToken cancellationToken);

        /// <summary>
        /// Clears the violation counter.
        /// </summary>
        Task<TopicDto> ClearViolationsAsync(Guid topicId, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a topic.
        /// </summary>
        Task RemoveTopicAsync(Guid topicId, CancellationToken cancellationToken);
    }
}
