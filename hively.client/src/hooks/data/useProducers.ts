import { useCallback, useEffect, useState } from 'react';
import { producerService } from '../../services/producerService';
import type { Producer } from '../../types';

export function useProducers() {
  const [producers, setProducers] = useState<Producer[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refetch = useCallback(async () => {
    try {
      setLoading(true);
      const data = await producerService.getProducers();
      setProducers(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch producers');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    refetch();
  }, [refetch]);

  return { producers, loading, error, refetch };
}
