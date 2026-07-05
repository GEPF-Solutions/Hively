import { useCallback, useEffect, useState } from 'react';
import { matchService } from '../../services/matchService';
import { useSignalR } from '../../contexts/SignalRContext';
import type { Match } from '../../types';

export function useMatches() {
  const [matches, setMatches] = useState<Match[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { topicHubConnection } = useSignalR();

  const refetch = useCallback(async () => {
    try {
      setLoading(true);
      const data = await matchService.getMatches();
      setMatches(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch matches');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    refetch();
  }, [refetch]);

  // Matches are created/edited from the Manage panel, which may be a completely
  // separate component instance (and hook call) than any other view showing
  // match-derived state — without this, that view only ever sees the matches
  // that existed when it first mounted.
  useEffect(() => {
    if (!topicHubConnection) return;

    topicHubConnection.on('MatchesChanged', refetch);
    return () => {
      topicHubConnection.off('MatchesChanged', refetch);
    };
  }, [topicHubConnection, refetch]);

  return { matches, loading, error, refetch };
}
