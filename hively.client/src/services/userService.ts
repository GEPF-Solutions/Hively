import { apiRequest } from './apiService';
import { apiEndpoints } from '../constants/apiEndpoints';
import type { User, UserRole } from '../types';

export const userService = {
  async getUsers(): Promise<User[]> {
    return apiRequest<User[]>(apiEndpoints.users.base);
  },

  async updateRole(userId: string, role: UserRole): Promise<User> {
    return apiRequest<User>(apiEndpoints.users.role(userId), {
      method: 'POST',
      body: JSON.stringify({ role }),
    });
  },
};
