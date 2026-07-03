using Hively.Server.DbModel;
using Hively.Server.Dto;
using Hively.Server.Exceptions;
using Hively.Server.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Hively.Server.Repository
{
    /// <summary>
    /// Repository for managing Consumer entities in the database.
    /// </summary>
    public class ConsumerRepository : IConsumerRepository
    {
        private readonly HivelyContext _dbContext;

        public ConsumerRepository(HivelyContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Consumer>> GetConsumersAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Consumers.AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Consumer> GetConsumerAsync(Guid consumerId, CancellationToken cancellationToken)
        {
            var consumer = await _dbContext.Consumers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == consumerId, cancellationToken);

            if (consumer == null)
            {
                throw new EntityNotFoundException($"Consumer id {consumerId} did not reference a valid consumer.");
            }

            return consumer;
        }

        /// <inheritdoc />
        public async Task<Consumer> InsertConsumerAsync(ConsumerDto consumerDto, CancellationToken cancellationToken)
        {
            var newConsumer = new Consumer
            {
                Name = consumerDto.Name,
                Description = consumerDto.Description
            };

            await _dbContext.Consumers.AddAsync(newConsumer, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return newConsumer;
        }

        /// <inheritdoc />
        public async Task UpdateConsumerAsync(ConsumerDto consumerDto, CancellationToken cancellationToken)
        {
            var consumerToUpdate = await _dbContext.Consumers
                .FirstOrDefaultAsync(x => x.Id == consumerDto.Id, cancellationToken);

            if (consumerToUpdate == null)
            {
                throw new EntityNotFoundException($"Consumer id {consumerDto.Id} did not reference a valid consumer.");
            }

            consumerToUpdate.Name = consumerDto.Name;
            consumerToUpdate.Description = consumerDto.Description;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveConsumerAsync(Guid consumerId, CancellationToken cancellationToken)
        {
            var consumerToRemove = await _dbContext.Consumers
                .FirstOrDefaultAsync(x => x.Id == consumerId, cancellationToken);

            if (consumerToRemove == null)
            {
                throw new EntityNotFoundException($"Consumer id {consumerId} did not reference a valid consumer.");
            }

            _dbContext.Consumers.Remove(consumerToRemove);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
