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
        /// Creates a new rule, then immediately applies it to every currently
        /// matching topic (tracked or not) via <see cref="ITopicService.ApplyRuleToAllMatchingAsync"/>.
        /// </summary>
        Task<RuleDto> InsertRuleAsync(RuleDto rule, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing rule, then immediately re-applies it to every
        /// currently matching topic (tracked or not) via <see cref="ITopicService.ApplyRuleToAllMatchingAsync"/>
        /// — this is how an already-configured topic picks up a change to the rule
        /// that configured it (e.g. a schema added after the fact), with no separate
        /// action needed beyond saving the rule.
        /// </summary>
        Task<RuleDto> UpdateRuleAsync(RuleDto rule, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a rule.
        /// </summary>
        Task RemoveRuleAsync(Guid ruleId, CancellationToken cancellationToken);
    }
}
