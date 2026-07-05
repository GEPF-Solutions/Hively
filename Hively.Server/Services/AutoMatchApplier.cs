using Hively.Server.Repository.Abstractions;
using Hively.Server.Services.Abstractions;

namespace Hively.Server.Services
{
    /// <inheritdoc cref="IAutoMatchApplier" />
    public class AutoMatchApplier : IAutoMatchApplier
    {
        private readonly IMatchRepository _matchRepository;
        private readonly ITopicRepository _topicRepository;

        public AutoMatchApplier(IMatchRepository matchRepository, ITopicRepository topicRepository)
        {
            _matchRepository = matchRepository;
            _topicRepository = topicRepository;
        }

        /// <inheritdoc />
        public async Task<bool> TryAutoApplyAsync(Guid topicId, string topicPath, CancellationToken cancellationToken)
        {
            var matches = (await _matchRepository.GetMatchesAsync(cancellationToken)).ToList();

            var hasAutoApplyMatch = matches
                .Where(m => TopicPatternMatcher.MatchTopic(m.Pattern, topicPath))
                .Any(m => m.AutoApply);

            if (!hasAutoApplyMatch)
            {
                return false;
            }

            var resolved = MatchResolver.Resolve(topicPath, matches);
            await _topicRepository.ApplyResolvedAssignmentAsync(
                topicId,
                resolved.ProducerId,
                resolved.SchemaId,
                resolved.ConsumerIds,
                resolved.TagIds,
                tracked: true,
                cancellationToken);

            return true;
        }
    }
}
