using Hively.Server.DbModel;
using Hively.Server.Repository.Abstractions;
using Hively.Server.Services.Abstractions;

namespace Hively.Server.Services
{
    /// <inheritdoc cref="IAutoRuleApplier" />
    public class AutoRuleApplier : IAutoRuleApplier
    {
        private readonly IRuleRepository _ruleRepository;
        private readonly ITopicRepository _topicRepository;
        private readonly ITopicNotifier _topicNotifier;

        public AutoRuleApplier(IRuleRepository ruleRepository, ITopicRepository topicRepository, ITopicNotifier topicNotifier)
        {
            _ruleRepository = ruleRepository;
            _topicRepository = topicRepository;
            _topicNotifier = topicNotifier;
        }

        /// <inheritdoc />
        public async Task<bool> TryAutoApplyAsync(Guid topicId, string topicPath, CancellationToken cancellationToken)
        {
            var rules = await _ruleRepository.GetRulesAsync(cancellationToken);
            var rule = FindSoleAutoApplyRule(topicPath, rules);
            if (rule == null)
            {
                return false;
            }

            await _topicRepository.ApplyRuleAsync(topicId, rule.Id, cancellationToken);
            return true;
        }

        /// <inheritdoc />
        public async Task<int> SweepUntrackedTopicsAsync(CancellationToken cancellationToken)
        {
            var rules = (await _ruleRepository.GetRulesAsync(cancellationToken)).ToList();
            var topics = await _topicRepository.GetTopicsAsync(cancellationToken);
            var untracked = topics.Where(t => !t.Tracked).ToList();

            var appliedCount = 0;
            foreach (var topic in untracked)
            {
                var rule = FindSoleAutoApplyRule(topic.Path, rules);
                if (rule == null)
                {
                    continue;
                }

                await _topicRepository.ApplyRuleAsync(topic.Id, rule.Id, cancellationToken);
                var updatedTopic = await _topicRepository.GetTopicAsync(topic.Id, cancellationToken);
                await _topicNotifier.NotifyTopicUpdatedAsync(TopicDtoBuilder.Build(updatedTopic), cancellationToken);
                appliedCount++;
            }

            return appliedCount;
        }

        // Auto-apply only kicks in when this is the *only* matching rule — same
        // "single match" concept the manual Configure flow already surfaces
        // (⚡ Apply rule vs. ⚠ N rules match) — and that one rule has opted in.
        private static Rule? FindSoleAutoApplyRule(string topicPath, IEnumerable<Rule> rules)
        {
            var matches = RuleMatcher.FindMatchingRules(topicPath, rules);
            return matches.Count == 1 && matches[0].AutoApply ? matches[0] : null;
        }
    }
}
