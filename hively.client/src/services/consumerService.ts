import { apiRequest } from './apiService';
import { apiEndpoints } from '../constants/apiEndpoints';
import type { Consumer } from '../types';

export const consumerService = {
  async getConsumers(): Promise<Consumer[]> {
    return apiRequest<Consumer[]>(apiEndpoints.consumers.base);
  },

  async getConsumer(id: string): Promise<Consumer> {
    return apiRequest<Consumer>(apiEndpoints.consumers.byId(id));
  },

  async insertConsumer(consumer: Partial<Consumer>): Promise<Consumer> {
    return apiRequest<Consumer>(apiEndpoints.consumers.insert, {
      method: 'PUT',
      body: JSON.stringify(consumer),
    });
  },

  async updateConsumer(consumer: Consumer): Promise<Consumer> {
    return apiRequest<Consumer>(apiEndpoints.consumers.update, {
      method: 'POST',
      body: JSON.stringify(consumer),
    });
  },

  async deleteConsumer(id: string): Promise<void> {
    await apiRequest(apiEndpoints.consumers.delete(id), { method: 'DELETE' });
  },
};
