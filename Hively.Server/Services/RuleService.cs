using Hively.Server.Dto;
using Hively.Server.Repository.Abstractions;
using Hively.Server.Services.Abstractions;

namespace Hively.Server.Services
{
    /// <summary>
    /// Service for managing Rule entities.
    /// </summary>
    public class RuleService : IRuleService
    {
        private readonly IRuleRepository _ruleRepository;

        public RuleService(IRuleRepository ruleRepository)
        {
            _ruleRepository = ruleRepository;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RuleDto>> GetRulesAsync(CancellationToken cancellationToken)
        {
            var rules = await _ruleRepository.GetRulesAsync(cancellationToken);
            return rules.Select(x => new RuleDto(x));
        }

        /// <inheritdoc />
        public async Task<RuleDto> GetRuleAsync(Guid ruleId, CancellationToken cancellationToken)
        {
            var rule = await _ruleRepository.GetRuleAsync(ruleId, cancellationToken);
            return new RuleDto(rule);
        }

        /// <inheritdoc />
        public async Task<RuleDto> InsertRuleAsync(RuleDto rule, CancellationToken cancellationToken)
        {
            var createdRule = await _ruleRepository.InsertRuleAsync(rule, cancellationToken);
            return new RuleDto(createdRule);
        }

        /// <inheritdoc />
        public async Task<RuleDto> UpdateRuleAsync(RuleDto rule, CancellationToken cancellationToken)
        {
            var updatedRule = await _ruleRepository.UpdateRuleAsync(rule, cancellationToken);
            return new RuleDto(updatedRule);
        }

        /// <inheritdoc />
        public Task RemoveRuleAsync(Guid ruleId, CancellationToken cancellationToken)
        {
            return _ruleRepository.RemoveRuleAsync(ruleId, cancellationToken);
        }
    }
}
