using Hively.Server.Dto;

namespace Hively.Server.Services.Abstractions
{
    /// <summary>
    /// Service interface for Match business logic.
    /// </summary>
    public interface IMatchService
    {
        /// <summary>
        /// Retrieves all admin-authored matches (excludes private per-topic overrides).
        /// </summary>
        Task<IEnumerable<MatchDto>> GetMatchesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a single match by ID.
        /// </summary>
        Task<MatchDto> GetMatchAsync(Guid matchId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new match, then immediately re-resolves every currently
        /// matching topic (tracked or not) via <see cref="ITopicService.ApplyMatchToAllMatchingAsync"/>.
        /// </summary>
        Task<MatchDto> InsertMatchAsync(MatchDto match, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing match, then immediately re-resolves every currently
        /// matching topic (tracked or not) via <see cref="ITopicService.ApplyMatchToAllMatchingAsync"/>
        /// — this is how an already-configured topic picks up a change to the match
        /// that configured it, with no separate action needed beyond saving.
        /// </summary>
        Task<MatchDto> UpdateMatchAsync(MatchDto match, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a match, then re-resolves every currently-tracked topic that
        /// matched its (now-gone) pattern via <see cref="ITopicService.RecomputeTopicsMatchingPatternAsync"/>,
        /// so a topic that loses its only setter for a field actually sees it clear.
        /// </summary>
        Task RemoveMatchAsync(Guid matchId, CancellationToken cancellationToken);
    }
}
