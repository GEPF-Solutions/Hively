/**
 * API endpoint paths used throughout the app. Centralized to avoid magic
 * strings and to make it obvious when a frontend call doesn't match a
 * backend route (see Hively.Server/Controllers).
 */
export const apiEndpoints = {
  auth: {
    loginGoogle: (returnUrl: string) => `/api/auth/login/google?returnUrl=${encodeURIComponent(returnUrl)}`,
    loginEntra: (returnUrl: string) => `/api/auth/login/entra?returnUrl=${encodeURIComponent(returnUrl)}`,
    loginBasic: '/api/auth/login/basic',
    logout: '/api/auth/logout',
    me: '/api/auth/me',
    providers: '/api/auth/providers',
  },

  users: {
    base: '/api/user',
    role: (userId: string) => `/api/user/${userId}/role`,
  },

  topics: {
    base: '/api/topic',
    byId: (id: string) => `/api/topic/${id}`,
    insert: '/api/topic/insert',
    update: '/api/topic/update',
    delete: (id: string) => `/api/topic/delete?topicId=${id}`,
    clearViolations: (id: string) => `/api/topic/${id}/clear-violations`,
    relinkCandidate: (id: string) => `/api/topic/${id}/relink-candidate`,
    acceptRelink: (id: string, oldTopicId: string) => `/api/topic/${id}/accept-relink/${oldTopicId}`,
  },

  producers: {
    base: '/api/producer',
    byId: (id: string) => `/api/producer/${id}`,
    insert: '/api/producer/insert',
    update: '/api/producer/update',
    delete: (id: string) => `/api/producer/delete?producerId=${id}`,
  },

  consumers: {
    base: '/api/consumer',
    byId: (id: string) => `/api/consumer/${id}`,
    insert: '/api/consumer/insert',
    update: '/api/consumer/update',
    delete: (id: string) => `/api/consumer/delete?consumerId=${id}`,
  },

  schemas: {
    base: '/api/schema',
    byId: (id: string) => `/api/schema/${id}`,
    versions: (id: string) => `/api/schema/${id}/versions`,
    insert: '/api/schema/insert',
    update: '/api/schema/update',
    delete: (id: string) => `/api/schema/delete?schemaId=${id}`,
  },

  tags: {
    base: '/api/tag',
    byId: (id: string) => `/api/tag/${id}`,
    insert: '/api/tag/insert',
    update: '/api/tag/update',
    delete: (id: string) => `/api/tag/delete?tagId=${id}`,
  },

  matches: {
    base: '/api/match',
    byId: (id: string) => `/api/match/${id}`,
    insert: '/api/match/insert',
    update: '/api/match/update',
    delete: (id: string) => `/api/match/delete?matchId=${id}`,
  },

  hubs: {
    topic: '/hubs/topic',
  },

  mqttStatus: {
    base: '/api/mqtt-status',
  },
} as const;
