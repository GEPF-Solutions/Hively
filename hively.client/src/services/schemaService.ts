import { apiRequest } from './apiService';
import { apiEndpoints } from '../constants/apiEndpoints';
import type { Schema, SchemaVersion } from '../types';

export const schemaService = {
  async getSchemas(): Promise<Schema[]> {
    return apiRequest<Schema[]>(apiEndpoints.schemas.base);
  },

  async getSchema(id: string): Promise<Schema> {
    return apiRequest<Schema>(apiEndpoints.schemas.byId(id));
  },

  async getSchemaVersions(id: string): Promise<SchemaVersion[]> {
    return apiRequest<SchemaVersion[]>(apiEndpoints.schemas.versions(id));
  },

  async insertSchema(schema: Partial<Schema>): Promise<Schema> {
    return apiRequest<Schema>(apiEndpoints.schemas.insert, {
      method: 'PUT',
      body: JSON.stringify(schema),
    });
  },

  async updateSchema(schema: Schema): Promise<Schema> {
    return apiRequest<Schema>(apiEndpoints.schemas.update, {
      method: 'POST',
      body: JSON.stringify(schema),
    });
  },

  async deleteSchema(id: string): Promise<void> {
    await apiRequest(apiEndpoints.schemas.delete(id), { method: 'DELETE' });
  },
};
