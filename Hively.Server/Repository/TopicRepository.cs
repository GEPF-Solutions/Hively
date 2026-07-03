using Hively.Server.DbModel;
using Hively.Server.Dto;
using Hively.Server.Exceptions;
using Hively.Server.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Hively.Server.Repository
{
    /// <summary>
    /// Repository for managing Topic entities in the database.
    /// </summary>
    public class TopicRepository : ITopicRepository
    {
        private readonly HivelyContext _dbContext;

        public TopicRepository(HivelyContext dbContext)
        {
            _dbContext = dbContext;
        }

        private IQueryable<Topic> TopicsWithRelations(bool asNoTracking)
        {
            var query = asNoTracking ? _dbContext.Topics.AsNoTracking() : _dbContext.Topics;
            return query
                .Include(x => x.Schema)
                .Include(x => x.Consumers)
                .Include(x => x.Tags);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Topic>> GetTopicsAsync(CancellationToken cancellationToken)
        {
            return await TopicsWithRelations(asNoTracking: true)
                .OrderBy(x => x.Path)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Topic> GetTopicAsync(Guid topicId, CancellationToken cancellationToken)
        {
            var topic = await TopicsWithRelations(asNoTracking: true)
                .FirstOrDefaultAsync(x => x.Id == topicId, cancellationToken);

            if (topic == null)
            {
                throw new EntityNotFoundException($"Topic id {topicId} did not reference a valid topic.");
            }

            return topic;
        }

        /// <inheritdoc />
        public async Task<Guid> InsertTopicAsync(TopicDto topicDto, CancellationToken cancellationToken)
        {
            var newTopic = new Topic
            {
                Path = topicDto.Path
            };

            await _dbContext.Topics.AddAsync(newTopic, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return newTopic.Id;
        }

        /// <inheritdoc />
        public async Task UpdateTopicAsync(TopicConfigureDto topicDto, CancellationToken cancellationToken)
        {
            var topicToUpdate = await TopicsWithRelations(asNoTracking: false)
                .FirstOrDefaultAsync(x => x.Id == topicDto.Id, cancellationToken);

            if (topicToUpdate == null)
            {
                throw new EntityNotFoundException($"Topic id {topicDto.Id} did not reference a valid topic.");
            }

            topicToUpdate.Tracked = topicDto.Tracked;
            topicToUpdate.ProducerId = topicDto.ProducerId;
            topicToUpdate.SchemaId = topicDto.SchemaId;

            var consumers = await _dbContext.Consumers
                .Where(c => topicDto.ConsumerIds.Contains(c.Id))
                .ToListAsync(cancellationToken);
            topicToUpdate.Consumers.Clear();
            foreach (var consumer in consumers)
            {
                topicToUpdate.Consumers.Add(consumer);
            }

            var tags = await _dbContext.Tags
                .Where(t => topicDto.TagIds.Contains(t.Id))
                .ToListAsync(cancellationToken);
            topicToUpdate.Tags.Clear();
            foreach (var tag in tags)
            {
                topicToUpdate.Tags.Add(tag);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task ClearViolationsAsync(Guid topicId, CancellationToken cancellationToken)
        {
            var topic = await _dbContext.Topics.FirstOrDefaultAsync(x => x.Id == topicId, cancellationToken);

            if (topic == null)
            {
                throw new EntityNotFoundException($"Topic id {topicId} did not reference a valid topic.");
            }

            topic.ViolationCount = 0;
            topic.LastClearedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveTopicAsync(Guid topicId, CancellationToken cancellationToken)
        {
            var topicToRemove = await _dbContext.Topics
                .FirstOrDefaultAsync(x => x.Id == topicId, cancellationToken);

            if (topicToRemove == null)
            {
                throw new EntityNotFoundException($"Topic id {topicId} did not reference a valid topic.");
            }

            _dbContext.Topics.Remove(topicToRemove);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
