using Hively.Server.Dto;

namespace Hively.Server.Services.Abstractions
{
    /// <summary>
    /// Service interface for Consumer business logic.
    /// </summary>
    public interface IConsumerService
    {
        /// <summary>
        /// Retrieves all consumers.
        /// </summary>
        Task<IEnumerable<ConsumerDto>> GetConsumersAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a single consumer by ID.
        /// </summary>
        Task<ConsumerDto> GetConsumerAsync(Guid consumerId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new consumer.
        /// </summary>
        Task<ConsumerDto> InsertConsumerAsync(ConsumerDto consumer, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing consumer.
        /// </summary>
        Task UpdateConsumerAsync(ConsumerDto consumer, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a consumer.
        /// </summary>
        Task RemoveConsumerAsync(Guid consumerId, CancellationToken cancellationToken);
    }
}
