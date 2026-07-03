using Hively.Server.DbModel;
using Hively.Server.Dto;

namespace Hively.Server.Repository.Abstractions
{
    /// <summary>
    /// Repository interface for managing Consumer entities.
    /// </summary>
    public interface IConsumerRepository
    {
        /// <summary>
        /// Retrieves all consumers.
        /// </summary>
        Task<IEnumerable<Consumer>> GetConsumersAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a single consumer by ID.
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when consumer is not found.</exception>
        Task<Consumer> GetConsumerAsync(Guid consumerId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new consumer.
        /// </summary>
        Task<Consumer> InsertConsumerAsync(ConsumerDto consumer, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing consumer.
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when consumer is not found.</exception>
        Task UpdateConsumerAsync(ConsumerDto consumer, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a consumer. Topics referencing it have the association
        /// removed by the database (ON DELETE CASCADE on topic_consumers) —
        /// no in-use check here, deletion is never blocked.
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when consumer is not found.</exception>
        Task RemoveConsumerAsync(Guid consumerId, CancellationToken cancellationToken);
    }
}
