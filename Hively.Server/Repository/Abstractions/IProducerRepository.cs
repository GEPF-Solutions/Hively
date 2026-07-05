using Hively.Server.DbModel;
using Hively.Server.Dto;

namespace Hively.Server.Repository.Abstractions
{
    /// <summary>
    /// Repository interface for managing Producer entities.
    /// </summary>
    public interface IProducerRepository
    {
        /// <summary>
        /// Retrieves all producers.
        /// </summary>
        Task<IEnumerable<Producer>> GetProducersAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a single producer by ID.
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when producer is not found.</exception>
        Task<Producer> GetProducerAsync(Guid producerId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new producer.
        /// </summary>
        Task<Producer> InsertProducerAsync(ProducerDto producer, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing producer.
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when producer is not found.</exception>
        Task UpdateProducerAsync(ProducerDto producer, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a producer. Topics/Matches referencing it have their producer
        /// reference set to null by the database (ON DELETE SET NULL) — no
        /// in-use check here, deletion is never blocked.
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when producer is not found.</exception>
        Task RemoveProducerAsync(Guid producerId, CancellationToken cancellationToken);
    }
}
