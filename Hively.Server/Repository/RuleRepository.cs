using Hively.Server.DbModel;
using Hively.Server.Dto;
using Hively.Server.Exceptions;
using Hively.Server.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Hively.Server.Repository
{
    /// <summary>
    /// Repository for managing Rule entities in the database.
    /// </summary>
    public class RuleRepository : IRuleRepository
    {
        private readonly HivelyContext _dbContext;

        public RuleRepository(HivelyContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Rule>> GetRulesAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Rules.AsNoTracking()
                .Include(x => x.Tags)
                .OrderBy(x => x.Pattern)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Rule> GetRuleAsync(Guid ruleId, CancellationToken cancellationToken)
        {
            var rule = await _dbContext.Rules.AsNoTracking()
                .Include(x => x.Tags)
                .FirstOrDefaultAsync(x => x.Id == ruleId, cancellationToken);

            if (rule == null)
            {
                throw new EntityNotFoundException($"Rule id {ruleId} did not reference a valid rule.");
            }

            return rule;
        }

        /// <inheritdoc />
        public async Task<Rule> InsertRuleAsync(RuleDto ruleDto, CancellationToken cancellationToken)
        {
            var tags = await _dbContext.Tags
                .Where(t => ruleDto.TagIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

            var newRule = new Rule
            {
                Name = ruleDto.Name,
                Pattern = ruleDto.Pattern,
                ProducerId = ruleDto.ProducerId,
                SchemaId = ruleDto.SchemaId,
                AutoApply = ruleDto.AutoApply,
                Tags = tags
            };

            await _dbContext.Rules.AddAsync(newRule, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return newRule;
        }

        /// <inheritdoc />
        public async Task<Rule> UpdateRuleAsync(RuleDto ruleDto, CancellationToken cancellationToken)
        {
            var ruleToUpdate = await _dbContext.Rules
                .Include(x => x.Tags)
                .FirstOrDefaultAsync(x => x.Id == ruleDto.Id, cancellationToken);

            if (ruleToUpdate == null)
            {
                throw new EntityNotFoundException($"Rule id {ruleDto.Id} did not reference a valid rule.");
            }

            ruleToUpdate.Name = ruleDto.Name;
            ruleToUpdate.Pattern = ruleDto.Pattern;
            ruleToUpdate.ProducerId = ruleDto.ProducerId;
            ruleToUpdate.SchemaId = ruleDto.SchemaId;
            ruleToUpdate.AutoApply = ruleDto.AutoApply;

            var tags = await _dbContext.Tags
                .Where(t => ruleDto.TagIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

            ruleToUpdate.Tags.Clear();
            foreach (var tag in tags)
            {
                ruleToUpdate.Tags.Add(tag);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ruleToUpdate;
        }

        /// <inheritdoc />
        public async Task RemoveRuleAsync(Guid ruleId, CancellationToken cancellationToken)
        {
            var ruleToRemove = await _dbContext.Rules
                .FirstOrDefaultAsync(x => x.Id == ruleId, cancellationToken);

            if (ruleToRemove == null)
            {
                throw new EntityNotFoundException($"Rule id {ruleId} did not reference a valid rule.");
            }

            _dbContext.Rules.Remove(ruleToRemove);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
