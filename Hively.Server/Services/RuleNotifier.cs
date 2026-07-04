using Hively.Server.Hubs;
using Hively.Server.Services.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace Hively.Server.Services
{
    /// <summary>
    /// Pushes Rule changes to connected clients over <see cref="TopicHub"/>.
    /// </summary>
    public class RuleNotifier : IRuleNotifier
    {
        private readonly IHubContext<TopicHub, ITopicHubClient> _hubContext;

        public RuleNotifier(IHubContext<TopicHub, ITopicHubClient> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <inheritdoc />
        public Task NotifyRulesChangedAsync(CancellationToken cancellationToken)
        {
            return _hubContext.Clients.All.RulesChanged(cancellationToken);
        }
    }
}
