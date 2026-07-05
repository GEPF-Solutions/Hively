using Hively.Server.Hubs;
using Hively.Server.Services.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace Hively.Server.Services
{
    /// <inheritdoc cref="IMatchNotifier" />
    public class MatchNotifier : IMatchNotifier
    {
        private readonly IHubContext<TopicHub, ITopicHubClient> _hubContext;

        public MatchNotifier(IHubContext<TopicHub, ITopicHubClient> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <inheritdoc />
        public Task NotifyMatchesChangedAsync(CancellationToken cancellationToken)
        {
            return _hubContext.Clients.All.MatchesChanged(cancellationToken);
        }
    }
}
