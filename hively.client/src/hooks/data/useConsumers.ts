import { useCallback, useEffect, useState } from 'react';
import { consumerService } from '../../services/consumerService';
import type { Consumer } from '../../types';

export function useConsumers() {
  const [consumers, setConsumers] = useState<Consumer[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refetch = useCallback(async () => {
    try {
      setLoading(true);
      const data = await consumerService.getConsumers();
      setConsumers(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch consumers');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    refetch();
  }, [refetch]);

  return { consumers, loading, error, refetch };
}
