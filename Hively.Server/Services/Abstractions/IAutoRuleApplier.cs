namespace Hively.Server.Services.Abstractions
{
    /// <summary>
    /// Applies a Rule automatically when a newly-untracked topic matches exactly
    /// one rule overall, and that rule has opted in via <see cref="DbModel.Rule.AutoApply"/>
    /// — shared between <see cref="ITopicService.InsertTopicAsync"/> (the manual
    /// stub-insert endpoint) and <see cref="Ingestion.ITopicIngestionService"/>
    /// (MQTT first-sighting), so both paths behave the same way. Never touches a
    /// topic with zero or multiple matches (Design/README.md's rule-conflict
    /// behavior for multiple matches is unchanged either way).
    /// </summary>
    public interface IAutoRuleApplier
    {
        /// <summary>
        /// Attempts to auto-apply the sole matching rule to the given (untracked)
        /// topic. Returns true if a rule was applied (so the caller knows to treat
        /// the topic as tracked/updated rather than newly-untracked).
        /// </summary>
        Task<bool> TryAutoApplyAsync(Guid topicId, string topicPath, CancellationToken cancellationToken);
    }
}
