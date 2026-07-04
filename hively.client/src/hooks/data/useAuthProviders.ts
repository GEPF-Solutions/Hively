import { useEffect, useState } from 'react';
import { authService } from '../../services/authService';
import type { AuthProviders } from '../../types';

export function useAuthProviders() {
  const [providers, setProviders] = useState<AuthProviders | null>(null);

  useEffect(() => {
    authService.getProviders().then(setProviders).catch(() => setProviders(null));
  }, []);

  return providers;
}
