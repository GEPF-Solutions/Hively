import { apiRequest } from './apiService';
import { apiEndpoints } from '../constants/apiEndpoints';
import type { RelinkCandidate, Topic, TopicConfigure } from '../types';

export const topicService = {
  async getTopics(): Promise<Topic[]> {
    return apiRequest<Topic[]>(apiEndpoints.topics.base);
  },

  async getTopic(id: string): Promise<Topic> {
    return apiRequest<Topic>(apiEndpoints.topics.byId(id));
  },

  async insertTopic(topic: Partial<Topic>): Promise<Topic> {
    return apiRequest<Topic>(apiEndpoints.topics.insert, {
      method: 'PUT',
      body: JSON.stringify(topic),
    });
  },

  async updateTopic(topic: TopicConfigure): Promise<Topic> {
    return apiRequest<Topic>(apiEndpoints.topics.update, {
      method: 'POST',
      body: JSON.stringify(topic),
    });
  },

  async clearViolations(id: string): Promise<Topic> {
    return apiRequest<Topic>(apiEndpoints.topics.clearViolations(id), { method: 'POST' });
  },

  async deleteTopic(id: string): Promise<void> {
    await apiRequest(apiEndpoints.topics.delete(id), { method: 'DELETE' });
  },

  async getRelinkCandidate(id: string): Promise<RelinkCandidate | null> {
    return apiRequest<RelinkCandidate | null>(apiEndpoints.topics.relinkCandidate(id));
  },

  async acceptRelink(id: string, oldTopicId: string): Promise<Topic> {
    return apiRequest<Topic>(apiEndpoints.topics.acceptRelink(id, oldTopicId), { method: 'POST' });
  },
};
