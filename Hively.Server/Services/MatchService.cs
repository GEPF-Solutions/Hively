using Hively.Server.Dto;
using Hively.Server.Repository.Abstractions;
using Hively.Server.Services.Abstractions;

namespace Hively.Server.Services
{
    /// <inheritdoc cref="IMatchService" />
    public class MatchService : IMatchService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IMatchNotifier _matchNotifier;
        private readonly ITopicService _topicService;

        public MatchService(IMatchRepository matchRepository, IMatchNotifier matchNotifier, ITopicService topicService)
        {
            _matchRepository = matchRepository;
            _matchNotifier = matchNotifier;
            _topicService = topicService;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MatchDto>> GetMatchesAsync(CancellationToken cancellationToken)
        {
            var matches = await _matchRepository.GetMatchesForManagementAsync(cancellationToken);
            return matches.Select(m => new MatchDto(m));
        }

        /// <inheritdoc />
        public async Task<MatchDto> GetMatchAsync(Guid matchId, CancellationToken cancellationToken)
        {
            var match = await _matchRepository.GetMatchAsync(matchId, cancellationToken);
            return new MatchDto(match);
        }

        /// <inheritdoc />
        public async Task<MatchDto> InsertMatchAsync(MatchDto match, CancellationToken cancellationToken)
        {
            var createdMatch = await _matchRepository.InsertMatchAsync(match, cancellationToken);
            await _topicService.ApplyMatchToAllMatchingAsync(createdMatch.Id, cancellationToken);
            await _matchNotifier.NotifyMatchesChangedAsync(cancellationToken);
            return new MatchDto(createdMatch);
        }

        /// <inheritdoc />
        public async Task<MatchDto> UpdateMatchAsync(MatchDto match, CancellationToken cancellationToken)
        {
            var updatedMatch = await _matchRepository.UpdateMatchAsync(match, cancellationToken);
            await _topicService.ApplyMatchToAllMatchingAsync(updatedMatch.Id, cancellationToken);
            await _matchNotifier.NotifyMatchesChangedAsync(cancellationToken);
            return new MatchDto(updatedMatch);
        }

        /// <inheritdoc />
        public async Task RemoveMatchAsync(Guid matchId, CancellationToken cancellationToken)
        {
            var match = await _matchRepository.GetMatchAsync(matchId, cancellationToken);
            var pattern = match.Pattern;

            await _matchRepository.RemoveMatchAsync(matchId, cancellationToken);
            await _topicService.RecomputeTopicsMatchingPatternAsync(pattern, cancellationToken);
            await _matchNotifier.NotifyMatchesChangedAsync(cancellationToken);
        }
    }
}
