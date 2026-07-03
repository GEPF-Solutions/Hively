using Hively.Server.DbModel;
using Hively.Server.Dto;
using Hively.Server.Repository.Abstractions;
using Hively.Server.Services.Abstractions;

namespace Hively.Server.Services
{
    /// <summary>
    /// Service for managing Topic entities, including compliance computation.
    /// </summary>
    public class TopicService : ITopicService
    {
        private readonly ITopicRepository _topicRepository;
        private readonly IRuleRepository _ruleRepository;

        public TopicService(ITopicRepository topicRepository, IRuleRepository ruleRepository)
        {
            _topicRepository = topicRepository;
            _ruleRepository = ruleRepository;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TopicDto>> GetTopicsAsync(CancellationToken cancellationToken)
        {
            var topics = await _topicRepository.GetTopicsAsync(cancellationToken);
            return topics.Select(BuildDto);
        }

        /// <inheritdoc />
        public async Task<TopicDto> GetTopicAsync(Guid topicId, CancellationToken cancellationToken)
        {
            var topic = await _topicRepository.GetTopicAsync(topicId, cancellationToken);
            return BuildDto(topic);
        }

        /// <inheritdoc />
        public async Task<TopicDto> InsertTopicAsync(TopicDto topic, CancellationToken cancellationToken)
        {
            var createdTopicId = await _topicRepository.InsertTopicAsync(topic, cancellationToken);
            var createdTopic = await _topicRepository.GetTopicAsync(createdTopicId, cancellationToken);
            return BuildDto(createdTopic);
        }

        /// <inheritdoc />
        public async Task<TopicDto> UpdateTopicAsync(TopicConfigureDto topic, CancellationToken cancellationToken)
        {
            await _topicRepository.UpdateTopicAsync(topic, cancellationToken);
            var updatedTopic = await _topicRepository.GetTopicAsync(topic.Id, cancellationToken);
            return BuildDto(updatedTopic);
        }

        /// <inheritdoc />
        public async Task<TopicDto> ClearViolationsAsync(Guid topicId, CancellationToken cancellationToken)
        {
            await _topicRepository.ClearViolationsAsync(topicId, cancellationToken);
            var updatedTopic = await _topicRepository.GetTopicAsync(topicId, cancellationToken);
            return BuildDto(updatedTopic);
        }

        /// <inheritdoc />
        public Task RemoveTopicAsync(Guid topicId, CancellationToken cancellationToken)
        {
            return _topicRepository.RemoveTopicAsync(topicId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RuleMatchDto>> FindMatchingRulesAsync(Guid topicId, CancellationToken cancellationToken)
        {
            var topic = await _topicRepository.GetTopicAsync(topicId, cancellationToken);
            var rules = await _ruleRepository.GetRulesAsync(cancellationToken);
            var matches = RuleMatcher.FindMatchingRules(topic.Path, rules);

            return matches.Select((rule, index) => new RuleMatchDto
            {
                Rule = new RuleDto(rule),
                Specificity = RuleMatcher.RuleSpecificity(rule.Pattern),
                Recommended = index == 0
            });
        }

        /// <inheritdoc />
        public async Task<TopicDto> ApplyRuleAsync(Guid topicId, Guid ruleId, CancellationToken cancellationToken)
        {
            await _topicRepository.ApplyRuleAsync(topicId, ruleId, cancellationToken);
            var updatedTopic = await _topicRepository.GetTopicAsync(topicId, cancellationToken);
            return BuildDto(updatedTopic);
        }

        /// <inheritdoc />
        public async Task<int> ApplyRuleToAllMatchingAsync(Guid ruleId, CancellationToken cancellationToken)
        {
            var rule = await _ruleRepository.GetRuleAsync(ruleId, cancellationToken);
            var topics = await _topicRepository.GetTopicsAsync(cancellationToken);
            var matchingUntracked = topics
                .Where(t => !t.Tracked && TopicPatternMatcher.MatchTopic(rule.Pattern, t.Path))
                .ToList();

            foreach (var topic in matchingUntracked)
            {
                await _topicRepository.ApplyRuleAsync(topic.Id, ruleId, cancellationToken);
            }

            return matchingUntracked.Count;
        }

        /// <inheritdoc />
        public async Task<RelinkCandidateDto?> FindRelinkCandidateAsync(Guid topicId, CancellationToken cancellationToken)
        {
            var target = await _topicRepository.GetTopicAsync(topicId, cancellationToken);
            var topics = await _topicRepository.GetTopicsAsync(cancellationToken);
            var candidate = RelinkHeuristic.FindRelinkCandidate(target, topics, DateTime.UtcNow);

            if (candidate == null)
            {
                return null;
            }

            return new RelinkCandidateDto
            {
                Topic = new TopicDto(candidate),
                SilentForMinutes = (int)(DateTime.UtcNow - candidate.LastSeenAt!.Value).TotalMinutes
            };
        }

        /// <inheritdoc />
        public async Task<TopicDto> AcceptRelinkAsync(Guid topicId, Guid oldTopicId, CancellationToken cancellationToken)
        {
            await _topicRepository.AcceptRelinkAsync(topicId, oldTopicId, cancellationToken);
            var updatedTopic = await _topicRepository.GetTopicAsync(topicId, cancellationToken);
            return BuildDto(updatedTopic);
        }

        private static TopicDto BuildDto(Topic topic)
        {
            var dto = new TopicDto(topic);
            (dto.Compliant, dto.Mismatches) = SchemaComplianceValidator.Validate(topic.Schema?.Definition, dto.LastPayload);
            return dto;
        }
    }
}
