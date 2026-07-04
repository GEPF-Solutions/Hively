using Hively.Server.Dto;
using Hively.Server.Hubs;
using Hively.Server.Services.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace Hively.Server.Services
{
    /// <summary>
    /// Pushes Topic changes to connected clients over <see cref="TopicHub"/>.
    /// </summary>
    public class TopicNotifier : ITopicNotifier
    {
        private readonly IHubContext<TopicHub, ITopicHubClient> _hubContext;

        public TopicNotifier(IHubContext<TopicHub, ITopicHubClient> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <inheritdoc />
        public Task NotifyTopicUntrackedAsync(TopicDto topic, CancellationToken cancellationToken)
        {
            return _hubContext.Clients.All.TopicUntracked(topic, cancellationToken);
        }

        /// <inheritdoc />
        public Task NotifyTopicUpdatedAsync(TopicDto topic, CancellationToken cancellationToken)
        {
            return _hubContext.Clients.All.TopicUpdated(topic, cancellationToken);
        }

        /// <inheritdoc />
        public Task NotifyTopicRemovedAsync(Guid topicId, CancellationToken cancellationToken)
        {
            return _hubContext.Clients.All.TopicRemoved(topicId, cancellationToken);
        }
    }
}
