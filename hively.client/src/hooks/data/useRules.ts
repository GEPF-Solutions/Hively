import { useCallback, useEffect, useState } from 'react';
import { ruleService } from '../../services/ruleService';
import { useSignalR } from '../../contexts/SignalRContext';
import type { Rule } from '../../types';

export function useRules() {
  const [rules, setRules] = useState<Rule[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { topicHubConnection } = useSignalR();

  const refetch = useCallback(async () => {
    try {
      setLoading(true);
      const data = await ruleService.getRules();
      setRules(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch rules');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    refetch();
  }, [refetch]);

  // Rules are created/edited from the Manage panel, which may be a completely
  // separate component instance (and hook call) than whatever's showing the
  // Topics list's "Apply rule" quick-action — without this, that list only
  // ever sees the rules that existed when it first mounted.
  useEffect(() => {
    if (!topicHubConnection) return;

    topicHubConnection.on('RulesChanged', refetch);
    return () => {
      topicHubConnection.off('RulesChanged', refetch);
    };
  }, [topicHubConnection, refetch]);

  return { rules, loading, error, refetch };
}
