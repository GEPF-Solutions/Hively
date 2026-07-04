import { apiRequest } from './apiService';
import { apiEndpoints } from '../constants/apiEndpoints';
import type { CurrentUser } from '../types';

export const authService = {
  /** Resolves the signed-in user, or null if the session cookie is missing/expired. */
  async getCurrentUser(): Promise<CurrentUser | null> {
    try {
      return await apiRequest<CurrentUser>(apiEndpoints.auth.me);
    } catch {
      return null;
    }
  },

  async logout(): Promise<void> {
    await apiRequest(apiEndpoints.auth.logout, { method: 'POST' });
  },

  // Login is a full-page redirect into the OIDC challenge, not a fetch call —
  // the browser needs to leave the SPA to complete the Google/Entra handshake.
  googleLoginUrl(returnUrl: string): string {
    return apiEndpoints.auth.loginGoogle(returnUrl);
  },

  entraLoginUrl(returnUrl: string): string {
    return apiEndpoints.auth.loginEntra(returnUrl);
  },
};
