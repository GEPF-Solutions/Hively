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
        public async Task<Topic?> FindTopicByPathAsync(string path, CancellationToken cancellationToken)
        {
            return await TopicsWithRelations(asNoTracking: true)
                .FirstOrDefaultAsync(x => x.Path == path, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Guid> InsertTopicAsync(TopicDto topicDto, CancellationToken cancellationToken)
        {
            var newTopic = new Topic
            {
                Path = topicDto.Path
            };

            await _dbContext.Topics.AddAsync(newTopic, cancellationToken);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueViolation())
            {
                throw new DuplicateEntityException($"A topic with path '{topicDto.Path}' already exists.");
            }

            return newTopic.Id;
        }

        /// <inheritdoc />
        public async Task ApplyResolvedAssignmentAsync(
            Guid topicId,
            Guid? producerId,
            Guid? schemaId,
            List<Guid> consumerIds,
            List<string> tagIds,
            bool? tracked,
            CancellationToken cancellationToken)
        {
            var topicToUpdate = await TopicsWithRelations(asNoTracking: false)
                .FirstOrDefaultAsync(x => x.Id == topicId, cancellationToken);

            if (topicToUpdate == null)
            {
                throw new EntityNotFoundException($"Topic id {topicId} did not reference a valid topic.");
            }

            if (tracked.HasValue)
            {
                topicToUpdate.Tracked = tracked.Value;
            }

            topicToUpdate.ProducerId = producerId;
            topicToUpdate.SchemaId = schemaId;

            var consumers = await _dbContext.Consumers
                .Where(c => consumerIds.Contains(c.Id))
                .ToListAsync(cancellationToken);
            topicToUpdate.Consumers.Clear();
            foreach (var consumer in consumers)
            {
                topicToUpdate.Consumers.Add(consumer);
            }

            var tags = await _dbContext.Tags
                .Where(t => tagIds.Contains(t.Id))
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

        /// <inheritdoc />
        public async Task AcceptRelinkAsync(Guid topicId, Guid oldTopicId, CancellationToken cancellationToken)
        {
            var topic = await TopicsWithRelations(asNoTracking: false)
                .FirstOrDefaultAsync(x => x.Id == topicId, cancellationToken);
            if (topic == null)
            {
                throw new EntityNotFoundException($"Topic id {topicId} did not reference a valid topic.");
            }

            var oldTopic = await TopicsWithRelations(asNoTracking: false)
                .FirstOrDefaultAsync(x => x.Id == oldTopicId, cancellationToken);
            if (oldTopic == null)
            {
                throw new EntityNotFoundException($"Topic id {oldTopicId} did not reference a valid topic.");
            }

            // Producer/schema/tags are NOT copied here — the service layer re-keys
            // oldTopic's private Match (if any) onto this topic's id/path and
            // re-resolves via ApplyResolvedAssignmentAsync, which also naturally
            // picks up any branch/pattern match now covering the new path.
            topic.Tracked = true;
            topic.ViolationCount = oldTopic.ViolationCount;
            topic.LastClearedAt = oldTopic.LastClearedAt;

            oldTopic.RetiredAt = DateTime.UtcNow;
            oldTopic.MergedIntoTopicId = topic.Id;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task RecordIngestedMessageAsync(
            Guid topicId,
            string payloadJson,
            DateTime seenAtUtc,
            bool retained,
            int violationCount,
            string activityHistogramJson,
            CancellationToken cancellationToken)
        {
            var topic = await _dbContext.Topics.FirstOrDefaultAsync(x => x.Id == topicId, cancellationToken);
            if (topic == null)
            {
                throw new EntityNotFoundException($"Topic id {topicId} did not reference a valid topic.");
            }

            topic.LastPayload = payloadJson;
            topic.LastSeenAt = seenAtUtc;
            topic.Retained = retained;
            topic.ViolationCount = violationCount;
            topic.ActivityHistogram = activityHistogramJson;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
