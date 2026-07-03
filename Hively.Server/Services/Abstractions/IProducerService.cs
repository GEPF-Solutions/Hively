using Hively.Server.Dto;

namespace Hively.Server.Services.Abstractions
{
    /// <summary>
    /// Service interface for Producer business logic.
    /// </summary>
    public interface IProducerService
    {
        /// <summary>
        /// Retrieves all producers.
        /// </summary>
        Task<IEnumerable<ProducerDto>> GetProducersAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a single producer by ID.
        /// </summary>
        Task<ProducerDto> GetProducerAsync(Guid producerId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new producer.
        /// </summary>
        Task<ProducerDto> InsertProducerAsync(ProducerDto producer, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing producer.
        /// </summary>
        Task UpdateProducerAsync(ProducerDto producer, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a producer.
        /// </summary>
        Task RemoveProducerAsync(Guid producerId, CancellationToken cancellationToken);
    }
}
