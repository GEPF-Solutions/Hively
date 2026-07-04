import { useMemo } from 'react';
import { segmentsOf } from '../../../utils/topicPath';
import type { Topic, TopicViewMode } from '../../../types';

// Hierarchy view groups by path through the "cell" level (see Design/README.md,
// Key Behaviors #7) — enterprise/region/country/site/area/line/cell = 7 segments.
const CELL_GROUP_DEPTH = 7;

export interface TopicGroup {
  label: string;
  showLabel: boolean;
  topics: Topic[];
}

export function useDisplayGroups(filtered: Topic[], viewMode: TopicViewMode): TopicGroup[] {
  return useMemo(() => {
    if (viewMode === 'list') {
      const sorted = [...filtered].sort((a, b) => a.path.localeCompare(b.path));
      return [{ label: '', showLabel: false, topics: sorted }];
    }

    const bySiteLine = new Map<string, Topic[]>();
    for (const topic of filtered) {
      const key = segmentsOf(topic.path).slice(0, CELL_GROUP_DEPTH).join('/');
      const group = bySiteLine.get(key) ?? [];
      group.push(topic);
      bySiteLine.set(key, group);
    }
    return Array.from(bySiteLine.keys())
      .sort()
      .map((key) => ({ label: key.split('/').join(' / '), showLabel: true, topics: bySiteLine.get(key)! }));
  }, [filtered, viewMode]);
}
