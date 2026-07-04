import { apiRequest } from './apiService';
import { apiEndpoints } from '../constants/apiEndpoints';
import type { AuthProviders, CurrentUser } from '../types';

export const authService = {
  /** Resolves the signed-in user, or null if the session cookie is missing/expired. */
  async getCurrentUser(): Promise<CurrentUser | null> {
    try {
      return await apiRequest<CurrentUser>(apiEndpoints.auth.me);
    } catch {
      return null;
    }
  },

  /** Which external providers are enabled — the login page uses this to decide which buttons to show. */
  async getProviders(): Promise<AuthProviders> {
    return apiRequest<AuthProviders>(apiEndpoints.auth.providers);
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

  // Basic auth is a same-page form submit, not a redirect — throws (via
  // apiRequest's ApiError) with the backend's plain-text message on failure.
  async loginBasic(username: string, password: string): Promise<CurrentUser> {
    return apiRequest<CurrentUser>(apiEndpoints.auth.loginBasic, {
      method: 'POST',
      body: JSON.stringify({ username, password }),
    });
  },
};
