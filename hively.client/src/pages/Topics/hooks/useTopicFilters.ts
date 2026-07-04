import { useMemo, useState } from 'react';
import { matchesSearch } from '../../../utils/searchMatch';
import { levelName, segmentsOf } from '../../../utils/topicPath';
import type { Consumer, Producer, Tag, Topic, TopicViewMode } from '../../../types';

export const UNTRACKED_FILTER = '__untracked__';

interface UseTopicFiltersArgs {
  topics: Topic[];
  producers: Producer[];
  consumers: Consumer[];
  tags: Tag[];
}

/**
 * Owns the Topics list page's client-side filter state (search / tag filter /
 * namespace drill-down path / hierarchy vs. list) — the backend has no
 * filter query params, so all of this runs over the already-fetched topic
 * list (see Design/README.md, Key Behaviors #5 and #7).
 */
export function useTopicFilters({ topics, producers, consumers, tags }: UseTopicFiltersArgs) {
  const [search, setSearch] = useState('');
  const [tagFilter, setTagFilter] = useState<string | null>(null);
  const [folderPath, setFolderPath] = useState<string[]>([]);
  const [viewMode, setViewMode] = useState<TopicViewMode>('hierarchy');

  const producerById = useMemo(() => new Map(producers.map((p) => [p.id, p])), [producers]);
  const consumerById = useMemo(() => new Map(consumers.map((c) => [c.id, c])), [consumers]);
  const tagById = useMemo(() => new Map(tags.map((t) => [t.id, t])), [tags]);

  const untrackedCount = useMemo(() => topics.filter((t) => !t.tracked).length, [topics]);

  const depth = folderPath.length;

  const prefixMatches = useMemo(
    () => topics.filter((t) => folderPath.every((seg, i) => segmentsOf(t.path)[i] === seg)),
    [topics, folderPath],
  );

  const namespaceChildren = useMemo(() => {
    const childrenMap = new Map<string, { name: string; count: number; untracked: number }>();
    for (const t of prefixMatches) {
      const segments = segmentsOf(t.path);
      if (segments.length <= depth) continue;
      const name = segments[depth];
      const existing = childrenMap.get(name) ?? { name, count: 0, untracked: 0 };
      existing.count += 1;
      if (!t.tracked) existing.untracked += 1;
      childrenMap.set(name, existing);
    }
    return Array.from(childrenMap.values())
      .sort((a, b) => a.name.localeCompare(b.name))
      .map((c) => ({ name: c.name, count: c.count, hasUntracked: c.untracked > 0 }));
  }, [prefixMatches, depth]);

  const currentLevelName = levelName(depth);

  function drillInto(name: string) {
    setFolderPath((prev) => [...prev, name]);
  }

  function drillToDepth(index: number) {
    setFolderPath((prev) => prev.slice(0, index));
  }

  function resetFolder() {
    setFolderPath([]);
  }

  const filtered = useMemo(() => {
    function matchesTopic(topic: Topic): boolean {
      if (!folderPath.every((seg, i) => segmentsOf(topic.path)[i] === seg)) return false;

      if (tagFilter === UNTRACKED_FILTER) {
        if (topic.tracked) return false;
      } else if (tagFilter) {
        if (!topic.tagIds.includes(tagFilter)) return false;
      }

      if (search.trim()) {
        const producerName = topic.producerId ? (producerById.get(topic.producerId)?.name ?? '') : '';
        const consumerNames = topic.consumerIds.map((id) => consumerById.get(id)?.name ?? '').join(' ');
        const tagLabels = topic.tagIds.map((id) => tagById.get(id)?.label ?? id).join(' ');
        const extra = `${producerName} ${consumerNames} ${tagLabels}`;
        if (!matchesSearch(search, topic.path, extra)) return false;
      }

      return true;
    }

    return topics.filter(matchesTopic);
  }, [topics, folderPath, tagFilter, search, producerById, consumerById, tagById]);

  return {
    search,
    setSearch,
    tagFilter,
    setTagFilter,
    folderPath,
    drillInto,
    drillToDepth,
    resetFolder,
    hasFolderFilter: folderPath.length > 0,
    viewMode,
    setViewMode,
    untrackedCount,
    namespaceChildren,
    currentLevelName,
    filtered,
    producerById,
    consumerById,
    tagById,
  };
}
