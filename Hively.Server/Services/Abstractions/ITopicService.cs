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
        /// Re-resolves and re-tracks every topic matching a Match's pattern (tracked
        /// or not), using the full current Match set. Called automatically by
        /// <see cref="IMatchService.InsertMatchAsync"/>/<see cref="IMatchService.UpdateMatchAsync"/>
        /// right after a match is saved, so an already-tracked topic immediately picks
        /// up a change to whatever matched it (e.g. a schema added after the fact)
        /// instead of only ever being resolved once, at first tracking. No separate
        /// manual trigger exists — re-saving the match (even with no field actually
        /// changed) re-runs this the same way a dedicated button would. Returns the
        /// number of topics touched.
        /// </summary>
        Task<int> ApplyMatchToAllMatchingAsync(Guid matchId, CancellationToken cancellationToken);

        /// <summary>
        /// Re-resolves every currently-tracked topic matching <paramref name="pattern"/>
        /// against the full current Match set, without tracking/untracking anything.
        /// Called by <see cref="IMatchService.RemoveMatchAsync"/> right after a match
        /// is deleted (with its now-gone pattern), so topics that lose their only
        /// setter for a field actually see it clear instead of keeping a stale value
        /// forever. Returns the number of topics touched.
        /// </summary>
        Task<int> RecomputeTopicsMatchingPatternAsync(string pattern, CancellationToken cancellationToken);

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
