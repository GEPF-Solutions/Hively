import { apiRequest } from './apiService';
import { apiEndpoints } from '../constants/apiEndpoints';
import type { Rule } from '../types';

export const ruleService = {
  async getRules(): Promise<Rule[]> {
    return apiRequest<Rule[]>(apiEndpoints.rules.base);
  },

  async getRule(id: string): Promise<Rule> {
    return apiRequest<Rule>(apiEndpoints.rules.byId(id));
  },

  async insertRule(rule: Partial<Rule>): Promise<Rule> {
    return apiRequest<Rule>(apiEndpoints.rules.insert, {
      method: 'PUT',
      body: JSON.stringify(rule),
    });
  },

  async updateRule(rule: Rule): Promise<Rule> {
    return apiRequest<Rule>(apiEndpoints.rules.update, {
      method: 'POST',
      body: JSON.stringify(rule),
    });
  },

  async deleteRule(id: string): Promise<void> {
    await apiRequest(apiEndpoints.rules.delete(id), { method: 'DELETE' });
  },

  /** Retroactively applies a rule to every currently-untracked topic it matches. Returns the number applied. */
  async applyToAllMatching(id: string): Promise<number> {
    return apiRequest<number>(apiEndpoints.rules.applyToAll(id), { method: 'POST' });
  },
};
