using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Hively.Server.Hubs
{
    /// <summary>
    /// Live-update hub for Topic changes. Clients only listen — there are no
    /// client-invokable methods, so this class body is intentionally empty; all
    /// pushes happen server-side via <see cref="Services.Abstractions.ITopicNotifier"/>.
    /// </summary>
    [Authorize]
    public class TopicHub : Hub<ITopicHubClient>
    {
    }
}
