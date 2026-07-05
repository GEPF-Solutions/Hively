using Hively.Server.DbModel;
using Hively.Server.Dto;

namespace Hively.Server.Repository.Abstractions
{
    /// <summary>
    /// Repository interface for managing Match entities.
    /// </summary>
    public interface IMatchRepository
    {
        /// <summary>
        /// Retrieves every match, including the private per-topic overrides — the
        /// full set <see cref="Services.MatchResolver"/> must resolve against.
        /// </summary>
        Task<IEnumerable<Match>> GetMatchesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves only admin-authored matches (branch/pattern), excluding the
        /// private per-topic overrides a manual TopicDetail edit creates — this is
        /// what the Manage Matches UI lists, never what a resolve pass runs against.
        /// </summary>
        Task<IEnumerable<Match>> GetMatchesForManagementAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a single match by ID, with its tag/consumer actions loaded.
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when the match is not found.</exception>
        Task<Match> GetMatchAsync(Guid matchId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the private per-topic override for this topic, if a manual
        /// TopicDetail edit has ever created one. Null (not a throw) when it hasn't —
        /// that's the common case for a topic whose fields are entirely pattern-derived.
        /// </summary>
        Task<Match?> GetMatchByTopicIdAsync(Guid topicId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new match. Unknown tag/consumer ids in the request are silently ignored.
        /// </summary>
        Task<Match> InsertMatchAsync(MatchDto match, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing match's pattern, producer/schema actions, and tag/consumer
        /// actions (the action sets are replaced wholesale, not merged).
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when the match is not found.</exception>
        Task<Match> UpdateMatchAsync(MatchDto match, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a match and its tag/consumer actions (ON DELETE CASCADE).
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when the match is not found.</exception>
        Task RemoveMatchAsync(Guid matchId, CancellationToken cancellationToken);
    }
}
