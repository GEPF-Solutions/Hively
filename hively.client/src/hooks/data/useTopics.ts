import { useCallback, useEffect, useState } from 'react';
import { topicService } from '../../services/topicService';
import { useSignalR } from '../../contexts/SignalRContext';
import type { Topic } from '../../types';

export function useTopics() {
  const [topics, setTopics] = useState<Topic[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { topicHubConnection } = useSignalR();

  const refetch = useCallback(async () => {
    try {
      setLoading(true);
      const data = await topicService.getTopics();
      setTopics(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch topics');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    refetch();
  }, [refetch]);

  // Live updates from MQTT ingestion / other admins' mutations — no polling.
  useEffect(() => {
    if (!topicHubConnection) return;

    const upsert = (topic: Topic) => {
      setTopics((prev) => {
        const index = prev.findIndex((t) => t.id === topic.id);
        if (index === -1) return [...prev, topic];
        const next = [...prev];
        next[index] = topic;
        return next;
      });
    };
    const remove = (topicId: string) => {
      setTopics((prev) => prev.filter((t) => t.id !== topicId));
    };

    topicHubConnection.on('TopicUntracked', upsert);
    topicHubConnection.on('TopicUpdated', upsert);
    topicHubConnection.on('TopicRemoved', remove);

    return () => {
      topicHubConnection.off('TopicUntracked', upsert);
      topicHubConnection.off('TopicUpdated', upsert);
      topicHubConnection.off('TopicRemoved', remove);
    };
  }, [topicHubConnection]);

  return { topics, loading, error, refetch };
}
