import { Route, Routes } from 'react-router-dom';
import NamespaceSidebar from './components/NamespaceSidebar';
import TopicListView from './components/TopicListView';
import TopicListSkeleton from './components/TopicListSkeleton';
import { useTopicFilters } from './hooks/useTopicFilters';
import { useTopics } from '../../hooks/data/useTopics';
import { useProducers } from '../../hooks/data/useProducers';
import { useConsumers } from '../../hooks/data/useConsumers';
import { useTags } from '../../hooks/data/useTags';
import { useSchemas } from '../../hooks/data/useSchemas';
import { useRules } from '../../hooks/data/useRules';
import { useAuth } from '../../contexts/AuthContext';
import TopicDetail from '../TopicDetail';

function TopicCatalog() {
  const { topics, loading } = useTopics();
  const { producers } = useProducers();
  const { consumers } = useConsumers();
  const { tags } = useTags();
  const { schemas } = useSchemas();
  const { rules } = useRules();
  const { isAdmin } = useAuth();

  const filters = useTopicFilters({ topics, producers, consumers, tags });

  if (loading) {
    return <TopicListSkeleton />;
  }

  return (
    <div className="flex h-full min-h-0">
      <NamespaceSidebar
        search={filters.search}
        onSearchChange={filters.setSearch}
        tags={tags}
        tagFilter={filters.tagFilter}
        onTagFilterChange={filters.setTagFilter}
        untrackedCount={filters.untrackedCount}
        folderPath={filters.folderPath}
        onDrillTo={filters.drillToDepth}
        hasFolderFilter={filters.hasFolderFilter}
        onResetFolder={filters.resetFolder}
        currentLevelName={filters.currentLevelName}
        namespaceChildren={filters.namespaceChildren}
        onDrillInto={filters.drillInto}
      />
      <div className="min-h-0 flex-1 overflow-y-auto">
        <TopicListView
          filtered={filters.filtered}
          viewMode={filters.viewMode}
          onViewModeChange={filters.setViewMode}
          producerById={filters.producerById}
          tagById={filters.tagById}
          schemas={schemas}
          tags={tags}
          rules={rules}
          isAdmin={isAdmin}
        />
      </div>
    </div>
  );
}

export default function Topics() {
  return (
    <Routes>
      <Route index element={<TopicCatalog />} />
      <Route path=":topicId" element={<TopicDetail />} />
    </Routes>
  );
}
