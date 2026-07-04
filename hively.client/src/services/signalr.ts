import * as signalR from '@microsoft/signalr';
import { apiEndpoints } from '../constants/apiEndpoints';

/**
 * Creates (but does not start) a connection to TopicHub. Callers own the
 * connection lifecycle — see contexts/SignalRContext.tsx for the shared
 * instance — and attach their own `.on(eventName, handler)` listeners
 * matching Hively.Server/Hubs/ITopicHubClient.cs's method names
 * (TopicUntracked/TopicUpdated/TopicRemoved/BrokerStatusChanged).
 */
export function createTopicHubConnection(): signalR.HubConnection {
  return new signalR.HubConnectionBuilder()
    .withUrl(apiEndpoints.hubs.topic)
    .withAutomaticReconnect()
    .build();
}
