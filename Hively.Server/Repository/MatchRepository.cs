using Hively.Server.DbModel;
using Hively.Server.Dto;
using Hively.Server.Exceptions;
using Hively.Server.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Hively.Server.Repository
{
    /// <summary>
    /// Repository for managing Match entities in the database.
    /// </summary>
    public class MatchRepository : IMatchRepository
    {
        private readonly HivelyContext _dbContext;

        public MatchRepository(HivelyContext dbContext)
        {
            _dbContext = dbContext;
        }

        private IQueryable<Match> MatchesWithActions(bool asNoTracking)
        {
            var query = asNoTracking ? _dbContext.Matches.AsNoTracking() : _dbContext.Matches;
            return query
                .Include(x => x.MatchTagActions)
                .Include(x => x.MatchConsumerActions);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Match>> GetMatchesAsync(CancellationToken cancellationToken)
        {
            return await MatchesWithActions(asNoTracking: true)
                .OrderBy(x => x.Pattern)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Match>> GetMatchesForManagementAsync(CancellationToken cancellationToken)
        {
            return await MatchesWithActions(asNoTracking: true)
                .Where(x => x.TopicId == null)
                .OrderBy(x => x.Pattern)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Match> GetMatchAsync(Guid matchId, CancellationToken cancellationToken)
        {
            var match = await MatchesWithActions(asNoTracking: true)
                .FirstOrDefaultAsync(x => x.Id == matchId, cancellationToken);

            if (match == null)
            {
                throw new EntityNotFoundException($"Match id {matchId} did not reference a valid match.");
            }

            return match;
        }

        /// <inheritdoc />
        public async Task<Match?> GetMatchByTopicIdAsync(Guid topicId, CancellationToken cancellationToken)
        {
            return await MatchesWithActions(asNoTracking: true)
                .FirstOrDefaultAsync(x => x.TopicId == topicId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Match> InsertMatchAsync(MatchDto matchDto, CancellationToken cancellationToken)
        {
            var newMatch = new Match
            {
                Name = matchDto.Name,
                Pattern = matchDto.Pattern,
                TopicId = matchDto.TopicId,
                ProducerId = matchDto.ProducerId,
                ExcludeProducer = matchDto.ExcludeProducer,
                SchemaId = matchDto.SchemaId,
                ExcludeSchema = matchDto.ExcludeSchema,
                AutoApply = matchDto.AutoApply,
                UpdatedAt = DateTime.UtcNow,
                MatchTagActions = await BuildTagActionsAsync(matchDto.TagActions, cancellationToken),
                MatchConsumerActions = await BuildConsumerActionsAsync(matchDto.ConsumerActions, cancellationToken)
            };

            await _dbContext.Matches.AddAsync(newMatch, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return newMatch;
        }

        /// <inheritdoc />
        public async Task<Match> UpdateMatchAsync(MatchDto matchDto, CancellationToken cancellationToken)
        {
            var matchToUpdate = await _dbContext.Matches
                .Include(x => x.MatchTagActions)
                .Include(x => x.MatchConsumerActions)
                .FirstOrDefaultAsync(x => x.Id == matchDto.Id, cancellationToken);

            if (matchToUpdate == null)
            {
                throw new EntityNotFoundException($"Match id {matchDto.Id} did not reference a valid match.");
            }

            matchToUpdate.Name = matchDto.Name;
            matchToUpdate.Pattern = matchDto.Pattern;
            matchToUpdate.TopicId = matchDto.TopicId;
            matchToUpdate.ProducerId = matchDto.ProducerId;
            matchToUpdate.ExcludeProducer = matchDto.ExcludeProducer;
            matchToUpdate.SchemaId = matchDto.SchemaId;
            matchToUpdate.ExcludeSchema = matchDto.ExcludeSchema;
            matchToUpdate.AutoApply = matchDto.AutoApply;
            matchToUpdate.UpdatedAt = DateTime.UtcNow;

            matchToUpdate.MatchTagActions.Clear();
            foreach (var action in await BuildTagActionsAsync(matchDto.TagActions, cancellationToken))
            {
                matchToUpdate.MatchTagActions.Add(action);
            }

            matchToUpdate.MatchConsumerActions.Clear();
            foreach (var action in await BuildConsumerActionsAsync(matchDto.ConsumerActions, cancellationToken))
            {
                matchToUpdate.MatchConsumerActions.Add(action);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return matchToUpdate;
        }

        /// <inheritdoc />
        public async Task RemoveMatchAsync(Guid matchId, CancellationToken cancellationToken)
        {
            var matchToRemove = await _dbContext.Matches
                .FirstOrDefaultAsync(x => x.Id == matchId, cancellationToken);

            if (matchToRemove == null)
            {
                throw new EntityNotFoundException($"Match id {matchId} did not reference a valid match.");
            }

            _dbContext.Matches.Remove(matchToRemove);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task<List<MatchTagAction>> BuildTagActionsAsync(
            List<MatchTagActionDto> actions, CancellationToken cancellationToken)
        {
            var validTagIds = await _dbContext.Tags
                .Where(t => actions.Select(a => a.TagId).Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync(cancellationToken);

            return actions
                .Where(a => validTagIds.Contains(a.TagId))
                .Select(a => new MatchTagAction { TagId = a.TagId, IsExclude = a.IsExclude })
                .ToList();
        }

        private async Task<List<MatchConsumerAction>> BuildConsumerActionsAsync(
            List<MatchConsumerActionDto> actions, CancellationToken cancellationToken)
        {
            var validConsumerIds = await _dbContext.Consumers
                .Where(c => actions.Select(a => a.ConsumerId).Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            return actions
                .Where(a => validConsumerIds.Contains(a.ConsumerId))
                .Select(a => new MatchConsumerAction { ConsumerId = a.ConsumerId, IsExclude = a.IsExclude })
                .ToList();
        }
    }
}
