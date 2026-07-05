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
        private readonly IMatchRepository _matchRepository;
        private readonly ITopicNotifier _topicNotifier;
        private readonly IAutoMatchApplier _autoMatchApplier;

        public TopicService(
            ITopicRepository topicRepository,
            IMatchRepository matchRepository,
            ITopicNotifier topicNotifier,
            IAutoMatchApplier autoMatchApplier)
        {
            _topicRepository = topicRepository;
            _matchRepository = matchRepository;
            _topicNotifier = topicNotifier;
            _autoMatchApplier = autoMatchApplier;
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

            if (!topic.Tracked)
            {
                await _autoMatchApplier.TryAutoApplyAsync(createdTopicId, topic.Path, cancellationToken);
            }

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
            var currentTopic = await _topicRepository.GetTopicAsync(topic.Id, cancellationToken);
            var existingPrivateMatch = await _matchRepository.GetMatchByTopicIdAsync(topic.Id, cancellationToken);

            var privateMatch = BuildPrivateMatch(currentTopic, topic, existingPrivateMatch);
            if (existingPrivateMatch == null)
            {
                await _matchRepository.InsertMatchAsync(privateMatch, cancellationToken);
            }
            else
            {
                privateMatch.Id = existingPrivateMatch.Id;
                await _matchRepository.UpdateMatchAsync(privateMatch, cancellationToken);
            }

            var allMatches = await _matchRepository.GetMatchesAsync(cancellationToken);
            var resolved = MatchResolver.Resolve(currentTopic.Path, allMatches);

            await _topicRepository.ApplyResolvedAssignmentAsync(
                topic.Id,
                resolved.ProducerId,
                resolved.SchemaId,
                resolved.ConsumerIds,
                resolved.TagIds,
                tracked: topic.Tracked,
                cancellationToken);

            var updatedTopic = await _topicRepository.GetTopicAsync(topic.Id, cancellationToken);
            var dto = TopicDtoBuilder.Build(updatedTopic);
            await _topicNotifier.NotifyTopicUpdatedAsync(dto, cancellationToken);
            return dto;
        }

        // A manual edit is always a hard pin, expressed as that topic's own private
        // Match (Pattern = its literal path, so it can only ever match itself):
        // producer/schema submitted non-null -> Set, null -> Exclude (there's no
        // existing UI affordance for "give control back to patterns", so nothing is
        // lost by never emitting "untouched" here). Tags/consumers: diff the
        // submitted full set against the topic's current cached set — newly-present
        // ids -> Set, newly-absent ids -> Exclude, unchanged ids keep whatever
        // action the private match already had (or no entry, if a pattern-derived
        // value was never manually touched) — never blindly exclude everything
        // unselected, which would permanently block future pattern contributions.
        private static MatchDto BuildPrivateMatch(Topic currentTopic, TopicConfigureDto topic, Match? existingPrivateMatch)
        {
            var dto = existingPrivateMatch != null
                ? new MatchDto(existingPrivateMatch)
                : new MatchDto();

            dto.Pattern = currentTopic.Path;
            dto.TopicId = currentTopic.Id;
            dto.ProducerId = topic.ProducerId;
            dto.ExcludeProducer = topic.ProducerId == null;
            dto.SchemaId = topic.SchemaId;
            dto.ExcludeSchema = topic.SchemaId == null;

            var currentTagIds = currentTopic.Tags.Select(t => t.Id).ToHashSet();
            var submittedTagIds = topic.TagIds.ToHashSet();
            var tagActions = dto.TagActions.ToDictionary(a => a.TagId);
            foreach (var tagId in submittedTagIds.Except(currentTagIds))
            {
                tagActions[tagId] = new MatchTagActionDto { TagId = tagId, IsExclude = false };
            }
            foreach (var tagId in currentTagIds.Except(submittedTagIds))
            {
                tagActions[tagId] = new MatchTagActionDto { TagId = tagId, IsExclude = true };
            }
            dto.TagActions = tagActions.Values.ToList();

            var currentConsumerIds = currentTopic.Consumers.Select(c => c.Id).ToHashSet();
            var submittedConsumerIds = topic.ConsumerIds.ToHashSet();
            var consumerActions = dto.ConsumerActions.ToDictionary(a => a.ConsumerId);
            foreach (var consumerId in submittedConsumerIds.Except(currentConsumerIds))
            {
                consumerActions[consumerId] = new MatchConsumerActionDto { ConsumerId = consumerId, IsExclude = false };
            }
            foreach (var consumerId in currentConsumerIds.Except(submittedConsumerIds))
            {
                consumerActions[consumerId] = new MatchConsumerActionDto { ConsumerId = consumerId, IsExclude = true };
            }
            dto.ConsumerActions = consumerActions.Values.ToList();

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
        public async Task<int> ApplyMatchToAllMatchingAsync(Guid matchId, CancellationToken cancellationToken)
        {
            var match = await _matchRepository.GetMatchAsync(matchId, cancellationToken);
            return await ReapplyMatchesToTopicsAsync(match.Pattern, forceTrack: true, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> RecomputeTopicsMatchingPatternAsync(string pattern, CancellationToken cancellationToken)
        {
            return await ReapplyMatchesToTopicsAsync(pattern, forceTrack: false, cancellationToken);
        }

        private async Task<int> ReapplyMatchesToTopicsAsync(string pattern, bool forceTrack, CancellationToken cancellationToken)
        {
            var allTopics = await _topicRepository.GetTopicsAsync(cancellationToken);
            var candidates = allTopics.Where(t => TopicPatternMatcher.MatchTopic(pattern, t.Path));
            if (!forceTrack)
            {
                // A deletion never tracks/untracks anything — only revisit topics
                // that were already tracked (and so could have a stale resolved value).
                candidates = candidates.Where(t => t.Tracked);
            }

            var candidateList = candidates.ToList();
            if (candidateList.Count == 0)
            {
                return 0;
            }

            var allMatches = await _matchRepository.GetMatchesAsync(cancellationToken);

            foreach (var topic in candidateList)
            {
                var resolved = MatchResolver.Resolve(topic.Path, allMatches);
                await _topicRepository.ApplyResolvedAssignmentAsync(
                    topic.Id,
                    resolved.ProducerId,
                    resolved.SchemaId,
                    resolved.ConsumerIds,
                    resolved.TagIds,
                    tracked: forceTrack ? true : (bool?)null,
                    cancellationToken);

                var updatedTopic = await _topicRepository.GetTopicAsync(topic.Id, cancellationToken);
                await _topicNotifier.NotifyTopicUpdatedAsync(TopicDtoBuilder.Build(updatedTopic), cancellationToken);
            }

            return candidateList.Count;
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
            var newTopic = await _topicRepository.GetTopicAsync(topicId, cancellationToken);

            // If the old topic had a private per-topic override, re-key it onto the
            // new topic's id/path rather than copying its fields — this naturally
            // also picks up any branch/pattern match that already covers the new
            // path, which a blind field-copy would ignore.
            var oldPrivateMatch = await _matchRepository.GetMatchByTopicIdAsync(oldTopicId, cancellationToken);
            if (oldPrivateMatch != null)
            {
                var rekeyed = new MatchDto(oldPrivateMatch)
                {
                    TopicId = topicId,
                    Pattern = newTopic.Path
                };
                await _matchRepository.UpdateMatchAsync(rekeyed, cancellationToken);
            }

            await _topicRepository.AcceptRelinkAsync(topicId, oldTopicId, cancellationToken);

            var allMatches = await _matchRepository.GetMatchesAsync(cancellationToken);
            var resolved = MatchResolver.Resolve(newTopic.Path, allMatches);
            await _topicRepository.ApplyResolvedAssignmentAsync(
                topicId,
                resolved.ProducerId,
                resolved.SchemaId,
                resolved.ConsumerIds,
                resolved.TagIds,
                tracked: null,
                cancellationToken);

            var updatedTopic = await _topicRepository.GetTopicAsync(topicId, cancellationToken);
            var dto = TopicDtoBuilder.Build(updatedTopic);
            await _topicNotifier.NotifyTopicUpdatedAsync(dto, cancellationToken);

            // The old topic was retired (RetiredAt/MergedIntoTopicId) as part of the
            // same mutation — push it too so any view still showing it stops looking stale.
            var oldTopic = await _topicRepository.GetTopicAsync(oldTopicId, cancellationToken);
            await _topicNotifier.NotifyTopicUpdatedAsync(TopicDtoBuilder.Build(oldTopic), cancellationToken);

            return dto;
        }
    }
}
