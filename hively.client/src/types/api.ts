// Shapes mirroring backend DTOs (Hively.Server/Dto/*.cs). Keep in sync by hand —
// there is no shared-schema codegen between the two projects.

export interface Producer {
  id: string;
  name: string;
  description: string | null;
}

export interface Consumer {
  id: string;
  name: string;
  description: string | null;
}

export interface Tag {
  id: string;
  label: string;
  hue: number | null;
}

export interface Schema {
  id: string;
  name: string;
  /** Field -> type map as a JSON string, e.g. {"value":"number","unit":"string"}. */
  definition: string;
  version: string | null;
}

export interface SchemaVersion {
  id: string;
  version: string;
  definition: string;
  createdAt: string;
}

export interface Rule {
  id: string;
  pattern: string;
  producerId: string | null;
  tagIds: string[];
}

export interface RuleMatch {
  rule: Rule;
  specificity: number;
  recommended: boolean;
}

export interface Topic {
  id: string;
  path: string;
  tracked: boolean;
  producerId: string | null;
  schemaId: string | null;
  lastPayload: string | null;
  lastSeenAt: string | null;
  retained: boolean;
  violationCount: number;
  lastClearedAt: string | null;
  /** 24 hourly message-count buckets, as a JSON array string. */
  activityHistogram: string | null;
  retiredAt: string | null;
  mergedIntoTopicId: string | null;
  consumerIds: string[];
  tagIds: string[];
  /** Computed on read — null if no schema assigned. */
  compliant: boolean | null;
  mismatches: string[];
}

/** Request shape for the "Configure Topic" action — see TopicConfigureDto. */
export interface TopicConfigure {
  id: string;
  tracked: boolean;
  producerId: string | null;
  schemaId: string | null;
  consumerIds: string[];
  tagIds: string[];
}

export interface RelinkCandidate {
  topic: Topic;
  silentForMinutes: number;
}

export type UserRole = 'Admin' | 'Viewer';

export interface User {
  id: string;
  email: string;
  role: UserRole;
  createdAt: string;
}

export interface CurrentUser {
  email: string;
  role: UserRole;
}
