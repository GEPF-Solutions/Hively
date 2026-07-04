using Hively.Server.Dto;

namespace Hively.Server.Services.Abstractions
{
    /// <summary>
    /// Service interface for Topic business logic, including compliance computation.
    /// </summary>
    public interface ITopicService
    {
        /// <summary>
        /// Retrieves all topics, each with compliance computed against its assigned schema.
        /// </summary>
        Task<IEnumerable<TopicDto>> GetTopicsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a single topic by ID, with compliance computed against its assigned schema.
        /// </summary>
        Task<TopicDto> GetTopicAsync(Guid topicId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new topic stub (see <see cref="Repository.Abstractions.ITopicRepository.InsertTopicAsync"/>).
        /// </summary>
        Task<TopicDto> InsertTopicAsync(TopicDto topic, CancellationToken cancellationToken);

        /// <summary>
        /// Updates a topic's tracked/producer/schema/consumer/tag assignment (the "Configure Topic" flow).
        /// </summary>
        Task<TopicDto> UpdateTopicAsync(TopicConfigureDto topic, CancellationToken cancellationToken);

        /// <summary>
        /// Clears the violation counter.
        /// </summary>
        Task<TopicDto> ClearViolationsAsync(Guid topicId, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a topic.
        /// </summary>
        Task RemoveTopicAsync(Guid topicId, CancellationToken cancellationToken);

        /// <summary>
        /// Finds rules whose pattern matches this topic's path, ranked by
        /// specificity (see <see cref="Services.RuleMatcher"/>); the most specific
        /// match is flagged <see cref="Dto.RuleMatchDto.Recommended"/>.
        /// </summary>
        Task<IEnumerable<RuleMatchDto>> FindMatchingRulesAsync(Guid topicId, CancellationToken cancellationToken);

        /// <summary>
        /// Applies a single rule to a topic (the "⚡ Apply rule" one-click action).
        /// </summary>
        Task<TopicDto> ApplyRuleAsync(Guid topicId, Guid ruleId, CancellationToken cancellationToken);

        /// <summary>
        /// Applies a rule to every topic matching its pattern, tracked or not.
        /// Called automatically by <see cref="IRuleService.InsertRuleAsync"/>/
        /// <see cref="IRuleService.UpdateRuleAsync"/> right after a rule is saved, so
        /// an already-tracked topic immediately picks up a change to the rule that
        /// configured it (e.g. a schema added after the fact) instead of only ever
        /// being touched once, at first tracking. Also exposed as the manual
        /// "Apply to N now" button in Manage Rules, for topics that started matching
        /// later without the rule itself being re-saved. Returns the number applied.
        /// </summary>
        Task<int> ApplyRuleToAllMatchingAsync(Guid ruleId, CancellationToken cancellationToken);

        /// <summary>
        /// Looks for a stale, tracked topic that looks like this untracked topic's
        /// "old address" after a physical relocation (see <see cref="Services.RelinkHeuristic"/>).
        /// Returns null when no candidate is found — a suggestion, never a certainty.
        /// </summary>
        Task<RelinkCandidateDto?> FindRelinkCandidateAsync(Guid topicId, CancellationToken cancellationToken);

        /// <summary>
        /// Accepts a relink suggestion: inherits producer/schema/tags/violation
        /// history from the old topic onto this one and retires the old topic.
        /// </summary>
        Task<TopicDto> AcceptRelinkAsync(Guid topicId, Guid oldTopicId, CancellationToken cancellationToken);
    }
}
