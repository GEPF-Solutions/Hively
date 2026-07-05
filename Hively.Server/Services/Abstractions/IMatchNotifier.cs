namespace Hively.Server.Services.Abstractions
{
    /// <summary>
    /// Pushes the <see cref="Hubs.ITopicHubClient.MatchesChanged"/> SignalR event
    /// after a Match mutation commits.
    /// </summary>
    public interface IMatchNotifier
    {
        Task NotifyMatchesChangedAsync(CancellationToken cancellationToken);
    }
}
