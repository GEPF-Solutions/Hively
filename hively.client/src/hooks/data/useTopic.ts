import { useCallback, useEffect, useState } from 'react';
import { topicService } from '../../services/topicService';
import { useSignalR } from '../../contexts/SignalRContext';
import type { Topic } from '../../types';

export function useTopic(topicId: string | undefined) {
  const [topic, setTopic] = useState<Topic | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { topicHubConnection } = useSignalR();

  const refetch = useCallback(async () => {
    if (!topicId) return;
    try {
      setLoading(true);
      const data = await topicService.getTopic(topicId);
      setTopic(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch topic');
    } finally {
      setLoading(false);
    }
  }, [topicId]);

  useEffect(() => {
    refetch();
  }, [refetch]);

  useEffect(() => {
    if (!topicHubConnection || !topicId) return;

    const onUpdated = (updated: Topic) => {
      if (updated.id === topicId) setTopic(updated);
    };
    const onRemoved = (removedId: string) => {
      if (removedId === topicId) setTopic(null);
    };

    topicHubConnection.on('TopicUpdated', onUpdated);
    topicHubConnection.on('TopicRemoved', onRemoved);

    return () => {
      topicHubConnection.off('TopicUpdated', onUpdated);
      topicHubConnection.off('TopicRemoved', onRemoved);
    };
  }, [topicHubConnection, topicId]);

  return { topic, loading, error, refetch };
}
