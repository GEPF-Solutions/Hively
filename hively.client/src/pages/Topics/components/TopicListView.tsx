import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import TopicRow from './TopicRow';
import ConfigureTopicModal from '../modals/ConfigureTopicModal';
import AddTopicModal from '../modals/AddTopicModal';
import Button from '../../../components/ui/Button';
import { useDisplayGroups } from '../hooks/useDisplayGroups';
import { findMatchingRules } from '../../../utils/ruleMatch';
import { useToast } from '../../../contexts/ToastContext';
import { topicService } from '../../../services/topicService';
import type { Consumer, Producer, Rule, Schema, Tag, Topic, TopicViewMode } from '../../../types';

const COLUMN_HEADERS = ['Topic', 'Producer', 'Consumers', 'Tags', 'Last Message', 'Compliance'];

interface TopicListViewProps {
  filtered: Topic[];
  viewMode: TopicViewMode;
  onViewModeChange: (mode: TopicViewMode) => void;
  producerById: Map<string, Producer>;
  tagById: Map<string, Tag>;
  producers: Producer[];
  consumers: Consumer[];
  schemas: Schema[];
  tags: Tag[];
  rules: Rule[];
  isAdmin: boolean;
}

export default function TopicListView({
  filtered,
  viewMode,
  onViewModeChange,
  producerById,
  tagById,
  producers,
  consumers,
  schemas,
  tags,
  rules,
  isAdmin,
}: TopicListViewProps) {
  const navigate = useNavigate();
  const toast = useToast();
  const groups = useDisplayGroups(filtered, viewMode);
  const [configuringTopic, setConfiguringTopic] = useState<Topic | null>(null);
  const [addTopicOpen, setAddTopicOpen] = useState(false);

  async function handleQuickApplyRule(topicId: string, ruleId: string) {
    try {
      await topicService.applyRule(topicId, ruleId);
      toast.success('Rule applied.');
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to apply rule');
    }
  }

  return (
    <div className="px-7 py-5">
      <div className="mb-4 flex items-center justify-between">
        <div className="text-[13.5px] text-muted">{filtered.length} topics</div>
        <div className="flex items-center gap-2.5">
          {isAdmin && (
            <Button variant="secondary" size="sm" onClick={() => setAddTopicOpen(true)}>
              + Add Topic
            </Button>
          )}
          <div className="flex gap-0.5 rounded-lg border border-border bg-panel p-0.5">
            <button
              onClick={() => onViewModeChange('hierarchy')}
              className={`rounded-md px-3 py-1.5 text-xs font-medium ${
                viewMode === 'hierarchy' ? 'bg-cyan text-[oklch(0.15_0.02_200)]' : 'text-muted'
              }`}
            >
              Hierarchy
            </button>
            <button
              onClick={() => onViewModeChange('list')}
              className={`rounded-md px-3 py-1.5 text-xs font-medium ${
                viewMode === 'list' ? 'bg-cyan text-[oklch(0.15_0.02_200)]' : 'text-muted'
              }`}
            >
              List
            </button>
          </div>
        </div>
      </div>

      {filtered.length === 0 && (
        <div className="py-16 text-center text-[13.5px] text-muted">No topics match your filters.</div>
      )}

      {groups.map((group) => (
        <div key={group.label} className="mb-5">
          {group.showLabel && <div className="mb-2 pl-1 font-mono text-xs text-muted">{group.label}</div>}
          <div className="overflow-hidden rounded-[10px] border border-border">
            <div className="grid grid-cols-[2.2fr_1fr_0.9fr_1.3fr_1fr_1fr] gap-3 bg-panel px-4 py-2.5 text-[10.5px] font-semibold uppercase tracking-wide text-muted">
              {COLUMN_HEADERS.map((h) => (
                <div key={h}>{h}</div>
              ))}
            </div>
            {group.topics.map((topic) => (
              <TopicRow
                key={topic.id}
                topic={topic}
                producerName={topic.producerId ? (producerById.get(topic.producerId)?.name ?? null) : null}
                consumerCount={topic.consumerIds.length}
                tagPills={topic.tagIds.map((id) => tagById.get(id)).filter((t): t is Tag => Boolean(t))}
                isAdmin={isAdmin}
                matchingRules={topic.tracked ? [] : findMatchingRules(topic.path, rules)}
                onOpen={() => (topic.tracked ? navigate(`/topics/${topic.id}`) : setConfiguringTopic(topic))}
                onQuickApplyRule={(ruleId) => handleQuickApplyRule(topic.id, ruleId)}
              />
            ))}
          </div>
        </div>
      ))}

      {configuringTopic && (
        <ConfigureTopicModal
          topic={configuringTopic}
          producers={producers}
          consumers={consumers}
          schemas={schemas}
          tags={tags}
          onClose={() => setConfiguringTopic(null)}
          onSaved={() => setConfiguringTopic(null)}
        />
      )}

      {addTopicOpen && (
        <AddTopicModal onClose={() => setAddTopicOpen(false)} onCreated={(created) => setConfiguringTopic(created)} />
      )}
    </div>
  );
}
