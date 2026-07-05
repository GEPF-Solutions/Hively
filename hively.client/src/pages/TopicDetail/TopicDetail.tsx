import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import Button from '../../components/ui/Button';
import Badge from '../../components/ui/Badge';
import { ConfirmModal, TagPill } from '../../components/shared';
import ProducerCard from './cards/ProducerCard';
import ConsumerCard from './cards/ConsumerCard';
import SchemaCard from './cards/SchemaCard';
import LastMessageCard from './cards/LastMessageCard';
import ComplianceCard from './cards/ComplianceCard';
import ActivityCard from './cards/ActivityCard';
import RelatedTopicsCard from './cards/RelatedTopicsCard';
import ProducerAssignModal from './modals/ProducerAssignModal';
import SchemaAssignModal from './modals/SchemaAssignModal';
import ConfigureTopicModal from '../Topics/modals/ConfigureTopicModal';
import { useTopic } from '../../hooks/data/useTopic';
import { useTopics } from '../../hooks/data/useTopics';
import { useProducers } from '../../hooks/data/useProducers';
import { useConsumers } from '../../hooks/data/useConsumers';
import { useSchemas } from '../../hooks/data/useSchemas';
import { useTags } from '../../hooks/data/useTags';
import { useAuth } from '../../contexts/AuthContext';
import { useToast } from '../../contexts/ToastContext';
import { topicService } from '../../services/topicService';
import { segmentsOf } from '../../utils/topicPath';
import type { TopicConfigure } from '../../types';

export default function TopicDetail() {
  const { topicId } = useParams<{ topicId: string }>();
  const navigate = useNavigate();
  const toast = useToast();
  const { isAdmin } = useAuth();

  const { topic, refetch } = useTopic(topicId);
  const { topics: allTopics } = useTopics();
  const { producers } = useProducers();
  const { consumers } = useConsumers();
  const { schemas } = useSchemas();
  const { tags } = useTags();

  const [changingProducer, setChangingProducer] = useState(false);
  const [changingSchema, setChangingSchema] = useState(false);
  const [tagPickerOpen, setTagPickerOpen] = useState(false);
  const [configuring, setConfiguring] = useState(false);
  const [confirmingDelete, setConfirmingDelete] = useState(false);
  const [deleting, setDeleting] = useState(false);

  if (!topic) {
    return <div className="p-6 text-muted">Loading topic…</div>;
  }

  const producer = topic.producerId ? (producers.find((p) => p.id === topic.producerId) ?? null) : null;
  const schema = topic.schemaId ? (schemas.find((s) => s.id === topic.schemaId) ?? null) : null;
  const topicConsumers = consumers.filter((c) => topic.consumerIds.includes(c.id));
  const topicTags = tags.filter((t) => topic.tagIds.includes(t.id));
  const leaf = segmentsOf(topic.path).at(-1);

  async function saveTopic(partial: Partial<TopicConfigure>) {
    if (!topic) return;
    try {
      await topicService.updateTopic({
        id: topic.id,
        tracked: topic.tracked,
        producerId: topic.producerId,
        schemaId: topic.schemaId,
        consumerIds: topic.consumerIds,
        tagIds: topic.tagIds,
        ...partial,
      });
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to update topic');
    }
  }

  function toggleConsumer(consumerId: string) {
    if (!topic) return;
    const next = topic.consumerIds.includes(consumerId)
      ? topic.consumerIds.filter((id) => id !== consumerId)
      : [...topic.consumerIds, consumerId];
    saveTopic({ consumerIds: next });
  }

  function toggleTag(tagId: string) {
    if (!topic) return;
    const next = topic.tagIds.includes(tagId) ? topic.tagIds.filter((id) => id !== tagId) : [...topic.tagIds, tagId];
    saveTopic({ tagIds: next });
  }

  async function handleClearViolations() {
    try {
      await topicService.clearViolations(topic!.id);
      toast.success('Violation counter cleared.');
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to clear violations');
    }
  }

  async function handleDelete() {
    setDeleting(true);
    try {
      await topicService.deleteTopic(topic!.id);
      toast.success('Topic deleted.');
      navigate('/topics');
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to delete topic');
      setDeleting(false);
    }
  }

  return (
    <div className="h-full overflow-y-auto px-8 py-6">
      <div className="mb-4 flex items-center justify-between">
        <Button variant="secondary" size="sm" onClick={() => navigate('/topics')}>
          ← Back to topics
        </Button>
        {isAdmin && (
          <Button variant="secondary" size="sm" className="!text-red !border-red/40 hover:!bg-red/10" onClick={() => setConfirmingDelete(true)}>
            Delete Topic
          </Button>
        )}
      </div>

      {!topic.tracked && (
        <div className="facet-md facet-border-amber mb-4 bg-amber/15 px-3.5 py-2.5 text-[12.5px] text-amber">
          This topic is untracked — seen on the broker but not yet catalogued.
          {isAdmin && (
            <Button variant="primary" size="sm" className="ml-3" onClick={() => setConfiguring(true)}>
              Configure →
            </Button>
          )}
        </div>
      )}

      <div className="mb-1.5 flex flex-wrap items-baseline gap-3.5">
        <div className="text-2xl font-semibold text-text">{leaf}</div>
        {topic.compliant === true && <Badge tone="cyan">Compliant</Badge>}
        {topic.compliant === false && <Badge tone="red">{topic.violationCount} violations</Badge>}
        {topic.compliant === null && <Badge tone="neutral">No schema — unknown</Badge>}
      </div>
      <div className="mb-4 font-mono text-[13px] text-muted">{topic.path}</div>

      <div className="mb-6 flex flex-wrap items-center gap-2">
        {topicTags.map((tag) => (
          <TagPill key={tag.id} tag={tag} onRemove={isAdmin ? () => toggleTag(tag.id) : undefined} />
        ))}
        {isAdmin && (
          <button
            onClick={() => setTagPickerOpen((v) => !v)}
            className="rounded-md border border-dashed border-border-strong px-2.5 py-1 text-[11.5px] text-muted"
          >
            + tag
          </button>
        )}
      </div>

      {tagPickerOpen && (
        <div className="hv-card -mt-4 mb-6 flex flex-wrap gap-1.5 p-2.5">
          {tags.map((tag) => (
            <TagPill key={tag.id} tag={tag} active={topic.tagIds.includes(tag.id)} onClick={() => toggleTag(tag.id)} />
          ))}
        </div>
      )}

      <div className="grid grid-cols-2 gap-4 xl:grid-cols-3">
        <ProducerCard producer={producer} canChange={isAdmin} onChangeClick={() => setChangingProducer(true)} />
        <ConsumerCard consumers={topicConsumers} canEdit={isAdmin} onToggle={toggleConsumer} />
        <ComplianceCard topic={topic} canClear={isAdmin} onClear={handleClearViolations} />
        <LastMessageCard topic={topic} />
        <SchemaCard schema={schema} canChange={isAdmin} onChangeClick={() => setChangingSchema(true)} />
        <ActivityCard activityHistogram={topic.activityHistogram} />
        <RelatedTopicsCard topic={topic} allTopics={allTopics} />
      </div>

      {changingProducer && (
        <ProducerAssignModal
          path={topic.path}
          currentProducerId={topic.producerId}
          onClose={() => setChangingProducer(false)}
          onSave={(producerId) => saveTopic({ producerId })}
        />
      )}

      {changingSchema && (
        <SchemaAssignModal
          path={topic.path}
          schemas={schemas}
          currentSchemaId={topic.schemaId}
          onClose={() => setChangingSchema(false)}
          onSave={(schemaId) => saveTopic({ schemaId })}
        />
      )}

      {configuring && (
        <ConfigureTopicModal
          topic={topic}
          schemas={schemas}
          tags={tags}
          onClose={() => setConfiguring(false)}
          onSaved={() => {
            setConfiguring(false);
            refetch();
          }}
        />
      )}

      {confirmingDelete && (
        <ConfirmModal
          title="Delete Topic"
          message={`Delete "${topic.path}"? This permanently removes its message history, violation count, and assignments. This can't be undone.`}
          confirmLabel="Delete"
          confirming={deleting}
          onConfirm={handleDelete}
          onClose={() => setConfirmingDelete(false)}
        />
      )}
    </div>
  );
}
