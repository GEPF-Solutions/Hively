import { useCallback, useEffect, useState } from 'react';
import { schemaService } from '../../services/schemaService';
import type { Schema } from '../../types';

export function useSchemas() {
  const [schemas, setSchemas] = useState<Schema[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refetch = useCallback(async () => {
    try {
      setLoading(true);
      const data = await schemaService.getSchemas();
      setSchemas(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch schemas');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    refetch();
  }, [refetch]);

  return { schemas, loading, error, refetch };
}
