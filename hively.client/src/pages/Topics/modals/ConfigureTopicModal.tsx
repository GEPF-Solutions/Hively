import { useEffect, useState } from 'react';
import Modal from '../../../components/ui/Modal';
import Button from '../../../components/ui/Button';
import { MultiSelectCombobox, SearchableCombobox, TagPill } from '../../../components/shared';
import { topicService } from '../../../services/topicService';
import { producerService } from '../../../services/producerService';
import { consumerService } from '../../../services/consumerService';
import { useProducers } from '../../../hooks/data/useProducers';
import { useConsumers } from '../../../hooks/data/useConsumers';
import { useToast } from '../../../contexts/ToastContext';
import { relativeTimeFromMinutes } from '../../../utils/relativeTime';
import type { RelinkCandidate, Schema, Tag, Topic } from '../../../types';

interface ConfigureTopicModalProps {
  topic: Topic;
  schemas: Schema[];
  tags: Tag[];
  onClose: () => void;
  onSaved: (topic: Topic) => void;
}

export default function ConfigureTopicModal({ topic, schemas, tags, onClose, onSaved }: ConfigureTopicModalProps) {
  const toast = useToast();
  const { producers, refetch: refetchProducers } = useProducers();
  const { consumers, refetch: refetchConsumers } = useConsumers();
  const [producerId, setProducerId] = useState(topic.producerId);
  const [consumerIds, setConsumerIds] = useState(topic.consumerIds);
  const [schemaId, setSchemaId] = useState(topic.schemaId);
  const [tagIds, setTagIds] = useState(topic.tagIds);
  const [relinkCandidate, setRelinkCandidate] = useState<RelinkCandidate | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    topicService.getRelinkCandidate(topic.id).then(setRelinkCandidate).catch(() => {});
  }, [topic.id]);

  function toggleConsumer(id: string) {
    setConsumerIds((prev) => (prev.includes(id) ? prev.filter((c) => c !== id) : [...prev, id]));
  }

  function toggleTag(id: string) {
    setTagIds((prev) => (prev.includes(id) ? prev.filter((t) => t !== id) : [...prev, id]));
  }

  async function handleCreateProducer(name: string) {
    const created = await producerService.insertProducer({ name });
    toast.success(`Created producer "${created.name}".`);
    refetchProducers();
    return { id: created.id, label: created.name };
  }

  async function handleCreateConsumer(name: string) {
    const created = await consumerService.insertConsumer({ name });
    toast.success(`Created consumer "${created.name}".`);
    refetchConsumers();
    return { id: created.id, label: created.name };
  }

  async function handleRelink() {
    if (!relinkCandidate) return;
    try {
      const updated = await topicService.acceptRelink(topic.id, relinkCandidate.topic.id);
      toast.success('Relinked and inherited history.');
      onSaved(updated);
      onClose();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to relink');
    }
  }

  async function handleSave() {
    setSaving(true);
    try {
      const updated = await topicService.updateTopic({
        id: topic.id,
        tracked: true,
        producerId,
        schemaId,
        consumerIds,
        tagIds,
      });
      toast.success('Topic configured.');
      onSaved(updated);
      onClose();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to save topic');
    } finally {
      setSaving(false);
    }
  }

  return (
    <Modal
      isOpen
      onClose={onClose}
      title="Configure Topic"
      maxWidth="lg"
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button variant="primary" onClick={handleSave} isLoading={saving}>
            Mark as Managed
          </Button>
        </>
      }
    >
      <div className="mb-3.5 rounded-md border border-amber/40 bg-amber/15 px-2.5 py-2 font-mono text-[12.5px] text-amber">
        {topic.path}
      </div>

      {relinkCandidate && (
        <div className="mb-3 rounded-lg border border-gold/40 bg-gold/15 p-3">
          <div className="text-[12.5px] leading-snug text-gold">
            🔗 Looks like a relocation of {relinkCandidate.topic.path} (silent{' '}
            {relativeTimeFromMinutes(relinkCandidate.silentForMinutes)}) — inherit its producer, schema, tags &
            history?
          </div>
          <Button variant="primary" size="sm" className="mt-2" onClick={handleRelink}>
            Relink &amp; inherit
          </Button>
        </div>
      )}

      <div className="flex flex-col gap-3.5">
        <div>
          <div className="mb-1.5 text-[11.5px] font-semibold text-muted">Producer</div>
          <SearchableCombobox
            options={producers.map((p) => ({ id: p.id, label: p.name }))}
            selectedId={producerId}
            onSelect={setProducerId}
            placeholder="type to search producers…"
            noneLabel="Unknown"
            onCreate={handleCreateProducer}
          />
        </div>

        <div>
          <div className="mb-1.5 text-[11.5px] font-semibold text-muted">Consumers</div>
          <MultiSelectCombobox
            options={consumers.map((c) => ({ id: c.id, label: c.name }))}
            selectedIds={consumerIds}
            onToggle={toggleConsumer}
            placeholder="type to search consumers…"
            onCreate={handleCreateConsumer}
          />
        </div>

        <div>
          <div className="mb-1.5 text-[11.5px] font-semibold text-muted">Schema</div>
          <SearchableCombobox
            options={schemas.map((s) => ({ id: s.id, label: s.name }))}
            selectedId={schemaId}
            onSelect={setSchemaId}
            placeholder="type to search schemas…"
            noneLabel="No schema"
          />
        </div>

        <div>
          <div className="mb-1.5 text-[11.5px] font-semibold text-muted">Tags</div>
          <div className="flex flex-wrap gap-1.5">
            {tags.map((tag) => (
              <TagPill key={tag.id} tag={tag} active={tagIds.includes(tag.id)} onClick={() => toggleTag(tag.id)} />
            ))}
          </div>
        </div>
      </div>
    </Modal>
  );
}
