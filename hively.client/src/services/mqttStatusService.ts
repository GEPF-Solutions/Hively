import { apiRequest } from './apiService';
import { apiEndpoints } from '../constants/apiEndpoints';
import type { MqttStatus } from '../types';

export const mqttStatusService = {
  async getStatus(): Promise<MqttStatus> {
    return apiRequest<MqttStatus>(apiEndpoints.mqttStatus.base);
  },
};
