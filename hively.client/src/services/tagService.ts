import { apiRequest } from './apiService';
import { apiEndpoints } from '../constants/apiEndpoints';
import type { Tag } from '../types';

export const tagService = {
  async getTags(): Promise<Tag[]> {
    return apiRequest<Tag[]>(apiEndpoints.tags.base);
  },

  async getTag(id: string): Promise<Tag> {
    return apiRequest<Tag>(apiEndpoints.tags.byId(id));
  },

  async insertTag(tag: Partial<Tag>): Promise<Tag> {
    return apiRequest<Tag>(apiEndpoints.tags.insert, {
      method: 'PUT',
      body: JSON.stringify(tag),
    });
  },

  async updateTag(tag: Tag): Promise<Tag> {
    return apiRequest<Tag>(apiEndpoints.tags.update, {
      method: 'POST',
      body: JSON.stringify(tag),
    });
  },

  async deleteTag(id: string): Promise<void> {
    await apiRequest(apiEndpoints.tags.delete(id), { method: 'DELETE' });
  },
};
