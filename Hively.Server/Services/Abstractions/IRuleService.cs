using Hively.Server.Dto;

namespace Hively.Server.Services.Abstractions
{
    /// <summary>
    /// Service interface for Rule business logic.
    /// </summary>
    public interface IRuleService
    {
        /// <summary>
        /// Retrieves all rules.
        /// </summary>
        Task<IEnumerable<RuleDto>> GetRulesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a single rule by ID.
        /// </summary>
        Task<RuleDto> GetRuleAsync(Guid ruleId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new rule.
        /// </summary>
        Task<RuleDto> InsertRuleAsync(RuleDto rule, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing rule.
        /// </summary>
        Task<RuleDto> UpdateRuleAsync(RuleDto rule, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a rule.
        /// </summary>
        Task RemoveRuleAsync(Guid ruleId, CancellationToken cancellationToken);
    }
}
