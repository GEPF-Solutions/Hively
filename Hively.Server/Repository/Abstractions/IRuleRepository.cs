using Hively.Server.DbModel;
using Hively.Server.Dto;

namespace Hively.Server.Repository.Abstractions
{
    /// <summary>
    /// Repository interface for managing Rule entities.
    /// </summary>
    public interface IRuleRepository
    {
        /// <summary>
        /// Retrieves all rules, with their tag assignments loaded.
        /// </summary>
        Task<IEnumerable<Rule>> GetRulesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a single rule by ID, with its tag assignments loaded.
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when rule is not found.</exception>
        Task<Rule> GetRuleAsync(Guid ruleId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new rule. Unknown tag ids in the request are silently ignored.
        /// </summary>
        Task<Rule> InsertRuleAsync(RuleDto rule, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing rule's pattern, producer, and tag assignments
        /// (the tag set is replaced wholesale, not merged).
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when rule is not found.</exception>
        Task<Rule> UpdateRuleAsync(RuleDto rule, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a rule and its tag assignments (ON DELETE CASCADE on rule_tags).
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when rule is not found.</exception>
        Task RemoveRuleAsync(Guid ruleId, CancellationToken cancellationToken);
    }
}
