namespace Hively.Server.Services.Abstractions
{
    /// <summary>
    /// Auto-tracks and resolves a newly-untracked topic the moment at least one
    /// applicable Match has opted in via <see cref="DbModel.Match.AutoApply"/> —
    /// shared between <see cref="ITopicService.InsertTopicAsync"/> (the manual
    /// stub-insert endpoint) and <see cref="Ingestion.ITopicIngestionService"/>
    /// (MQTT first-sighting), so both paths behave the same way. Unlike the old
    /// Rule system's "sole match" restriction, this fires as soon as any opted-in
    /// match covers the path — resolution across every applicable match (not just
    /// the AutoApply ones) is always automatic, so there's no "conflict" case left
    /// to wait on.
    /// </summary>
    public interface IAutoMatchApplier
    {
        /// <summary>
        /// Attempts to auto-apply. Returns true if at least one applicable match had
        /// AutoApply set (so the caller knows to treat the topic as tracked/updated
        /// rather than newly-untracked).
        /// </summary>
        Task<bool> TryAutoApplyAsync(Guid topicId, string topicPath, CancellationToken cancellationToken);
    }
}
