using Hively.Server.Dto;
using Hively.Server.Repository.Abstractions;
using Hively.Server.Services.Abstractions;

namespace Hively.Server.Services
{
    /// <summary>
    /// Service for managing Consumer entities.
    /// </summary>
    public class ConsumerService : IConsumerService
    {
        private readonly IConsumerRepository _consumerRepository;

        public ConsumerService(IConsumerRepository consumerRepository)
        {
            _consumerRepository = consumerRepository;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ConsumerDto>> GetConsumersAsync(CancellationToken cancellationToken)
        {
            var consumers = await _consumerRepository.GetConsumersAsync(cancellationToken);
            return consumers.Select(x => new ConsumerDto(x));
        }

        /// <inheritdoc />
        public async Task<ConsumerDto> GetConsumerAsync(Guid consumerId, CancellationToken cancellationToken)
        {
            var consumer = await _consumerRepository.GetConsumerAsync(consumerId, cancellationToken);
            return new ConsumerDto(consumer);
        }

        /// <inheritdoc />
        public async Task<ConsumerDto> InsertConsumerAsync(ConsumerDto consumer, CancellationToken cancellationToken)
        {
            var createdConsumer = await _consumerRepository.InsertConsumerAsync(consumer, cancellationToken);
            return new ConsumerDto(createdConsumer);
        }

        /// <inheritdoc />
        public Task UpdateConsumerAsync(ConsumerDto consumer, CancellationToken cancellationToken)
        {
            return _consumerRepository.UpdateConsumerAsync(consumer, cancellationToken);
        }

        /// <inheritdoc />
        public Task RemoveConsumerAsync(Guid consumerId, CancellationToken cancellationToken)
        {
            return _consumerRepository.RemoveConsumerAsync(consumerId, cancellationToken);
        }
    }
}
