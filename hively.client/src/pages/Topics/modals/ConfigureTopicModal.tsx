import { useEffect, useState } from 'react';
import Modal from '../../../components/ui/Modal';
import Button from '../../../components/ui/Button';
import { MultiSelectPills, SearchableCombobox, TagPill } from '../../../components/shared';
import { topicService } from '../../../services/topicService';
import { useToast } from '../../../contexts/ToastContext';
import { relativeTimeFromMinutes } from '../../../utils/relativeTime';
import type { Consumer, Producer, RelinkCandidate, RuleMatch, Schema, Tag, Topic } from '../../../types';

interface ConfigureTopicModalProps {
  topic: Topic;
  producers: Producer[];
  consumers: Consumer[];
  schemas: Schema[];
  tags: Tag[];
  onClose: () => void;
  onSaved: (topic: Topic) => void;
}

export default function ConfigureTopicModal({
  topic,
  producers,
  consumers,
  schemas,
  tags,
  onClose,
  onSaved,
}: ConfigureTopicModalProps) {
  const toast = useToast();
  const [producerId, setProducerId] = useState(topic.producerId);
  const [consumerIds, setConsumerIds] = useState(topic.consumerIds);
  const [schemaId, setSchemaId] = useState(topic.schemaId);
  const [tagIds, setTagIds] = useState(topic.tagIds);
  const [matchingRules, setMatchingRules] = useState<RuleMatch[]>([]);
  const [relinkCandidate, setRelinkCandidate] = useState<RelinkCandidate | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    topicService.getMatchingRules(topic.id).then(setMatchingRules).catch(() => {});
    topicService.getRelinkCandidate(topic.id).then(setRelinkCandidate).catch(() => {});
  }, [topic.id]);

  function toggleConsumer(id: string) {
    setConsumerIds((prev) => (prev.includes(id) ? prev.filter((c) => c !== id) : [...prev, id]));
  }

  function toggleTag(id: string) {
    setTagIds((prev) => (prev.includes(id) ? prev.filter((t) => t !== id) : [...prev, id]));
  }

  async function handleApplyRule(ruleId: string) {
    try {
      const updated = await topicService.applyRule(topic.id, ruleId);
      toast.success('Rule applied.');
      onSaved(updated);
      onClose();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to apply rule');
    }
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

  const hasSingleMatch = matchingRules.length === 1;
  const hasConflict = matchingRules.length > 1;

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
        <div className="mb-3 rounded-lg border border-cyan/40 bg-cyan/15 p-3">
          <div className="text-[12.5px] leading-snug text-cyan">
            🔗 Looks like a relocation of {relinkCandidate.topic.path} (silent{' '}
            {relativeTimeFromMinutes(relinkCandidate.silentForMinutes)}) — inherit its producer, schema, tags &
            history?
          </div>
          <Button variant="primary" size="sm" className="mt-2" onClick={handleRelink}>
            Relink &amp; inherit
          </Button>
        </div>
      )}

      {hasSingleMatch && (
        <div className="mb-3.5 rounded-lg border border-amber/40 bg-amber/20 p-3">
          <div className="text-[12.5px] leading-snug text-amber">
            ⚡ Matches rule <span className="font-mono">{matchingRules[0].rule.pattern}</span>
          </div>
          <Button variant="primary" size="sm" className="mt-2" onClick={() => handleApplyRule(matchingRules[0].rule.id)}>
            Apply rule
          </Button>
        </div>
      )}

      {hasConflict && (
        <div className="mb-3.5 rounded-lg border border-red/40 bg-red/15 p-3">
          <div className="mb-1.5 text-[12.5px] font-semibold text-red">⚠ Multiple rules match this topic — pick one:</div>
          <div className="flex flex-col gap-1.5">
            {matchingRules.map((m) => (
              <div key={m.rule.id} className="flex items-center justify-between gap-2 rounded-md bg-red/10 px-2.5 py-1.5">
                <div className="min-w-0">
                  <div className="overflow-hidden text-ellipsis whitespace-nowrap font-mono text-[11.5px] text-text/90">
                    {m.rule.pattern}
                    {m.recommended && <span className="ml-1 font-semibold text-cyan">· most specific</span>}
                  </div>
                </div>
                <Button variant="primary" size="sm" onClick={() => handleApplyRule(m.rule.id)}>
                  Use this
                </Button>
              </div>
            ))}
          </div>
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
          />
        </div>

        <div>
          <div className="mb-1.5 text-[11.5px] font-semibold text-muted">Consumers</div>
          <MultiSelectPills
            options={consumers.map((c) => ({ id: c.id, label: c.name }))}
            selectedIds={consumerIds}
            onToggle={toggleConsumer}
            placeholder="type to search consumers…"
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
