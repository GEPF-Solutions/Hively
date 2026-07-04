import * as signalR from '@microsoft/signalr';
import { apiEndpoints } from '../constants/apiEndpoints';
import type { Topic } from '../types';

/** Strongly-typed handlers matching Hively.Server/Hubs/ITopicHubClient.cs. */
export interface TopicHubHandlers {
  onTopicUntracked?: (topic: Topic) => void;
  onTopicUpdated?: (topic: Topic) => void;
  onTopicRemoved?: (topicId: string) => void;
}

/**
 * Creates (but does not start) a connection to TopicHub. Callers own the
 * connection lifecycle — see contexts/SignalRContext.tsx for the shared instance.
 */
export function createTopicHubConnection(): signalR.HubConnection {
  return new signalR.HubConnectionBuilder()
    .withUrl(apiEndpoints.hubs.topic)
    .withAutomaticReconnect()
    .build();
}

export function registerTopicHubHandlers(connection: signalR.HubConnection, handlers: TopicHubHandlers): void {
  if (handlers.onTopicUntracked) connection.on('TopicUntracked', handlers.onTopicUntracked);
  if (handlers.onTopicUpdated) connection.on('TopicUpdated', handlers.onTopicUpdated);
  if (handlers.onTopicRemoved) connection.on('TopicRemoved', handlers.onTopicRemoved);
}
