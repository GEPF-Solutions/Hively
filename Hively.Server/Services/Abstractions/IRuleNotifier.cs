namespace Hively.Server.Services.Abstractions
{
    /// <summary>
    /// Pushes Rule changes to connected clients over <see cref="Hubs.TopicHub"/>.
    /// Called from the service layer only, after a mutation has committed to the DB.
    /// </summary>
    public interface IRuleNotifier
    {
        /// <summary>Notifies clients that a rule was created, edited, or deleted.</summary>
        Task NotifyRulesChangedAsync(CancellationToken cancellationToken);
    }
}
