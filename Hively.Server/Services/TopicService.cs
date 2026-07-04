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
        private readonly ITopicNotifier _topicNotifier;

        public TopicService(ITopicRepository topicRepository, IRuleRepository ruleRepository, ITopicNotifier topicNotifier)
        {
            _topicRepository = topicRepository;
            _ruleRepository = ruleRepository;
            _topicNotifier = topicNotifier;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TopicDto>> GetTopicsAsync(CancellationToken cancellationToken)
        {
            var topics = await _topicRepository.GetTopicsAsync(cancellationToken);
            return topics.Select(TopicDtoBuilder.Build);
        }

        /// <inheritdoc />
        public async Task<TopicDto> GetTopicAsync(Guid topicId, CancellationToken cancellationToken)
        {
            var topic = await _topicRepository.GetTopicAsync(topicId, cancellationToken);
            return TopicDtoBuilder.Build(topic);
        }

        /// <inheritdoc />
        public async Task<TopicDto> InsertTopicAsync(TopicDto topic, CancellationToken cancellationToken)
        {
            var createdTopicId = await _topicRepository.InsertTopicAsync(topic, cancellationToken);
            var createdTopic = await _topicRepository.GetTopicAsync(createdTopicId, cancellationToken);
            var dto = TopicDtoBuilder.Build(createdTopic);

            if (dto.Tracked)
            {
                await _topicNotifier.NotifyTopicUpdatedAsync(dto, cancellationToken);
            }
            else
            {
                await _topicNotifier.NotifyTopicUntrackedAsync(dto, cancellationToken);
            }

            return dto;
        }

        /// <inheritdoc />
        public async Task<TopicDto> UpdateTopicAsync(TopicConfigureDto topic, CancellationToken cancellationToken)
        {
            await _topicRepository.UpdateTopicAsync(topic, cancellationToken);
            var updatedTopic = await _topicRepository.GetTopicAsync(topic.Id, cancellationToken);
            var dto = TopicDtoBuilder.Build(updatedTopic);
            await _topicNotifier.NotifyTopicUpdatedAsync(dto, cancellationToken);
            return dto;
        }

        /// <inheritdoc />
        public async Task<TopicDto> ClearViolationsAsync(Guid topicId, CancellationToken cancellationToken)
        {
            await _topicRepository.ClearViolationsAsync(topicId, cancellationToken);
            var updatedTopic = await _topicRepository.GetTopicAsync(topicId, cancellationToken);
            var dto = TopicDtoBuilder.Build(updatedTopic);
            await _topicNotifier.NotifyTopicUpdatedAsync(dto, cancellationToken);
            return dto;
        }

        /// <inheritdoc />
        public async Task RemoveTopicAsync(Guid topicId, CancellationToken cancellationToken)
        {
            await _topicRepository.RemoveTopicAsync(topicId, cancellationToken);
            await _topicNotifier.NotifyTopicRemovedAsync(topicId, cancellationToken);
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
            var dto = TopicDtoBuilder.Build(updatedTopic);
            await _topicNotifier.NotifyTopicUpdatedAsync(dto, cancellationToken);
            return dto;
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
                var updatedTopic = await _topicRepository.GetTopicAsync(topic.Id, cancellationToken);
                await _topicNotifier.NotifyTopicUpdatedAsync(TopicDtoBuilder.Build(updatedTopic), cancellationToken);
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
            var dto = TopicDtoBuilder.Build(updatedTopic);
            await _topicNotifier.NotifyTopicUpdatedAsync(dto, cancellationToken);

            // The old topic was retired (RetiredAt/MergedIntoTopicId set) as part of the
            // same mutation — push it too so any view still showing it stops looking stale.
            var oldTopic = await _topicRepository.GetTopicAsync(oldTopicId, cancellationToken);
            await _topicNotifier.NotifyTopicUpdatedAsync(TopicDtoBuilder.Build(oldTopic), cancellationToken);

            return dto;
        }
    }
}
