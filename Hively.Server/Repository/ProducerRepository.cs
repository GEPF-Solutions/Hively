using Hively.Server.DbModel;
using Hively.Server.Dto;
using Hively.Server.Exceptions;
using Hively.Server.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Hively.Server.Repository
{
    /// <summary>
    /// Repository for managing Producer entities in the database.
    /// </summary>
    public class ProducerRepository : IProducerRepository
    {
        private readonly HivelyContext _dbContext;

        public ProducerRepository(HivelyContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Producer>> GetProducersAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Producers.AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Producer> GetProducerAsync(Guid producerId, CancellationToken cancellationToken)
        {
            var producer = await _dbContext.Producers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == producerId, cancellationToken);

            if (producer == null)
            {
                throw new EntityNotFoundException($"Producer id {producerId} did not reference a valid producer.");
            }

            return producer;
        }

        /// <inheritdoc />
        public async Task<Producer> InsertProducerAsync(ProducerDto producerDto, CancellationToken cancellationToken)
        {
            var newProducer = new Producer
            {
                Name = producerDto.Name,
                Description = producerDto.Description
            };

            await _dbContext.Producers.AddAsync(newProducer, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return newProducer;
        }

        /// <inheritdoc />
        public async Task UpdateProducerAsync(ProducerDto producerDto, CancellationToken cancellationToken)
        {
            var producerToUpdate = await _dbContext.Producers
                .FirstOrDefaultAsync(x => x.Id == producerDto.Id, cancellationToken);

            if (producerToUpdate == null)
            {
                throw new EntityNotFoundException($"Producer id {producerDto.Id} did not reference a valid producer.");
            }

            producerToUpdate.Name = producerDto.Name;
            producerToUpdate.Description = producerDto.Description;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveProducerAsync(Guid producerId, CancellationToken cancellationToken)
        {
            var producerToRemove = await _dbContext.Producers
                .FirstOrDefaultAsync(x => x.Id == producerId, cancellationToken);

            if (producerToRemove == null)
            {
                throw new EntityNotFoundException($"Producer id {producerId} did not reference a valid producer.");
            }

            _dbContext.Producers.Remove(producerToRemove);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
