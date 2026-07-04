import { useState } from 'react';
import Modal from '../../../components/ui/Modal';
import Button from '../../../components/ui/Button';
import Input from '../../../components/ui/Input';
import { SearchableCombobox, TagPill } from '../../../components/shared';
import { useRules } from '../../../hooks/data/useRules';
import { useProducers } from '../../../hooks/data/useProducers';
import { useTags } from '../../../hooks/data/useTags';
import { useTopics } from '../../../hooks/data/useTopics';
import { ruleService } from '../../../services/ruleService';
import { useToast } from '../../../contexts/ToastContext';
import { matchTopicPattern } from '../../../utils/searchMatch';

export default function ManageRulesPanel({ onClose }: { onClose: () => void }) {
  const { rules, refetch } = useRules();
  const { producers } = useProducers();
  const { tags } = useTags();
  const { topics, refetch: refetchTopics } = useTopics();
  const toast = useToast();

  const [pattern, setPattern] = useState('');
  const [producerId, setProducerId] = useState<string | null>(null);
  const [tagIds, setTagIds] = useState<string[]>([]);

  const producerById = new Map(producers.map((p) => [p.id, p]));
  const tagById = new Map(tags.map((t) => [t.id, t]));
  const untrackedTopics = topics.filter((t) => !t.tracked);

  function matchCount(rulePattern: string) {
    return untrackedTopics.filter((t) => matchTopicPattern(rulePattern, t.path)).length;
  }

  function toggleTag(id: string) {
    setTagIds((prev) => (prev.includes(id) ? prev.filter((t) => t !== id) : [...prev, id]));
  }

  async function handleAdd() {
    if (!pattern.trim()) return;
    try {
      await ruleService.insertRule({ pattern: pattern.trim(), producerId, tagIds });
      setPattern('');
      setProducerId(null);
      setTagIds([]);
      refetch();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to add rule');
    }
  }

  async function handleDelete(id: string) {
    try {
      await ruleService.deleteRule(id);
      refetch();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to delete rule');
    }
  }

  async function handleApplyToAll(id: string) {
    try {
      const count = await ruleService.applyToAllMatching(id);
      toast.success(`Applied to ${count} topic${count === 1 ? '' : 's'}.`);
      refetchTopics();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to apply rule');
    }
  }

  return (
    <Modal isOpen onClose={onClose} title="Manage Rules" maxWidth="lg" footer={<Button onClick={onClose}>Done</Button>}>
      <div className="mb-4 text-xs leading-relaxed text-muted">
        Auto-assign producer &amp; tags to topics matching an MQTT-style pattern (+ = one level, # = rest). Use for
        bulk-managing many topics from the same producer, or to survive a namespace reshuffle.
      </div>

      <div className="mb-5 flex max-h-56 flex-col gap-2 overflow-y-auto">
        {rules.map((rule) => {
          const count = matchCount(rule.pattern);
          return (
            <div key={rule.id} className="flex items-center gap-2.5 rounded-md bg-bg px-2.5 py-2.5">
              <div className="min-w-0 flex-1">
                <div className="truncate font-mono text-[12.5px] text-text">{rule.pattern}</div>
                <div className="mt-0.5 text-[11.5px] text-muted">
                  producer: {rule.producerId ? (producerById.get(rule.producerId)?.name ?? 'Unknown') : 'Unknown'}
                </div>
                <div className="mt-1.5 flex flex-wrap gap-1">
                  {rule.tagIds.map((id) => {
                    const tag = tagById.get(id);
                    return tag ? <TagPill key={id} tag={tag} /> : null;
                  })}
                </div>
              </div>
              {count > 0 && (
                <Button variant="primary" size="sm" onClick={() => handleApplyToAll(rule.id)}>
                  Apply to {count} now
                </Button>
              )}
              <button onClick={() => handleDelete(rule.id)} className="text-base leading-none text-muted hover:text-text">
                ×
              </button>
            </div>
          );
        })}
      </div>

      <div className="border-t border-border pt-4">
        <div className="mb-1.5 text-[11.5px] font-semibold text-muted">New rule</div>
        <Input
          value={pattern}
          onChange={(e) => setPattern(e.target.value)}
          placeholder="acme/+/+/+/+/+/press-01/#"
          className="mb-2.5 font-mono"
        />
        <div className="mb-2.5">
          <SearchableCombobox
            options={producers.map((p) => ({ id: p.id, label: p.name }))}
            selectedId={producerId}
            onSelect={setProducerId}
            placeholder="type to search producers…"
            noneLabel="Unknown"
            maxHeight={120}
          />
        </div>
        <div className="mb-3 flex flex-wrap gap-1.5">
          {tags.map((tag) => (
            <TagPill key={tag.id} tag={tag} active={tagIds.includes(tag.id)} onClick={() => toggleTag(tag.id)} />
          ))}
        </div>
        <Button variant="primary" className="w-full" onClick={handleAdd}>
          Add rule
        </Button>
      </div>
    </Modal>
  );
}
