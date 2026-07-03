using Hively.Server.Dto;
using Hively.Server.Repository.Abstractions;
using Hively.Server.Services.Abstractions;

namespace Hively.Server.Services
{
    /// <summary>
    /// Service for managing Producer entities.
    /// </summary>
    public class ProducerService : IProducerService
    {
        private readonly IProducerRepository _producerRepository;

        public ProducerService(IProducerRepository producerRepository)
        {
            _producerRepository = producerRepository;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ProducerDto>> GetProducersAsync(CancellationToken cancellationToken)
        {
            var producers = await _producerRepository.GetProducersAsync(cancellationToken);
            return producers.Select(x => new ProducerDto(x));
        }

        /// <inheritdoc />
        public async Task<ProducerDto> GetProducerAsync(Guid producerId, CancellationToken cancellationToken)
        {
            var producer = await _producerRepository.GetProducerAsync(producerId, cancellationToken);
            return new ProducerDto(producer);
        }

        /// <inheritdoc />
        public async Task<ProducerDto> InsertProducerAsync(ProducerDto producer, CancellationToken cancellationToken)
        {
            var createdProducer = await _producerRepository.InsertProducerAsync(producer, cancellationToken);
            return new ProducerDto(createdProducer);
        }

        /// <inheritdoc />
        public Task UpdateProducerAsync(ProducerDto producer, CancellationToken cancellationToken)
        {
            return _producerRepository.UpdateProducerAsync(producer, cancellationToken);
        }

        /// <inheritdoc />
        public Task RemoveProducerAsync(Guid producerId, CancellationToken cancellationToken)
        {
            return _producerRepository.RemoveProducerAsync(producerId, cancellationToken);
        }
    }
}
