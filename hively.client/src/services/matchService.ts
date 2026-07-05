import { apiRequest } from './apiService';
import { apiEndpoints } from '../constants/apiEndpoints';
import type { Match } from '../types';

export const matchService = {
  async getMatches(): Promise<Match[]> {
    return apiRequest<Match[]>(apiEndpoints.matches.base);
  },

  async getMatch(id: string): Promise<Match> {
    return apiRequest<Match>(apiEndpoints.matches.byId(id));
  },

  async insertMatch(match: Partial<Match>): Promise<Match> {
    return apiRequest<Match>(apiEndpoints.matches.insert, {
      method: 'PUT',
      body: JSON.stringify(match),
    });
  },

  async updateMatch(match: Match): Promise<Match> {
    return apiRequest<Match>(apiEndpoints.matches.update, {
      method: 'POST',
      body: JSON.stringify(match),
    });
  },

  async deleteMatch(id: string): Promise<void> {
    await apiRequest(apiEndpoints.matches.delete(id), { method: 'DELETE' });
  },
};
