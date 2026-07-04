import { apiRequest } from './apiService';
import { apiEndpoints } from '../constants/apiEndpoints';
import type { Producer } from '../types';

export const producerService = {
  async getProducers(): Promise<Producer[]> {
    return apiRequest<Producer[]>(apiEndpoints.producers.base);
  },

  async getProducer(id: string): Promise<Producer> {
    return apiRequest<Producer>(apiEndpoints.producers.byId(id));
  },

  async insertProducer(producer: Partial<Producer>): Promise<Producer> {
    return apiRequest<Producer>(apiEndpoints.producers.insert, {
      method: 'PUT',
      body: JSON.stringify(producer),
    });
  },

  async updateProducer(producer: Producer): Promise<Producer> {
    return apiRequest<Producer>(apiEndpoints.producers.update, {
      method: 'POST',
      body: JSON.stringify(producer),
    });
  },

  async deleteProducer(id: string): Promise<void> {
    await apiRequest(apiEndpoints.producers.delete(id), { method: 'DELETE' });
  },
};
