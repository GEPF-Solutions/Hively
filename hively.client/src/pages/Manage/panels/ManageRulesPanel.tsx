import { useState } from 'react';
import Modal from '../../../components/ui/Modal';
import Button from '../../../components/ui/Button';
import Badge from '../../../components/ui/Badge';
import Input from '../../../components/ui/Input';
import { ManageList, SearchableCombobox, TagPill } from '../../../components/shared';
import { useRules } from '../../../hooks/data/useRules';
import { useProducers } from '../../../hooks/data/useProducers';
import { useSchemas } from '../../../hooks/data/useSchemas';
import { useTags } from '../../../hooks/data/useTags';
import { useTopics } from '../../../hooks/data/useTopics';
import { ruleService } from '../../../services/ruleService';
import { producerService } from '../../../services/producerService';
import { useToast } from '../../../contexts/ToastContext';
import { matchTopicPattern } from '../../../utils/searchMatch';
import type { Rule } from '../../../types';

export default function ManageRulesPanel({ onClose }: { onClose: () => void }) {
  const { rules, refetch } = useRules();
  const { producers, refetch: refetchProducers } = useProducers();
  const { schemas } = useSchemas();
  const { tags } = useTags();
  const { topics, refetch: refetchTopics } = useTopics();
  const toast = useToast();

  const [search, setSearch] = useState('');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [name, setName] = useState('');
  const [pattern, setPattern] = useState('');
  const [producerId, setProducerId] = useState<string | null>(null);
  const [schemaId, setSchemaId] = useState<string | null>(null);
  const [tagIds, setTagIds] = useState<string[]>([]);
  const [autoApply, setAutoApply] = useState(false);

  const producerById = new Map(producers.map((p) => [p.id, p]));
  const schemaById = new Map(schemas.map((s) => [s.id, s]));
  const tagById = new Map(tags.map((t) => [t.id, t]));
  const filtered = rules.filter((r) => {
    const q = search.trim().toLowerCase();
    return (r.name ?? '').toLowerCase().includes(q) || r.pattern.toLowerCase().includes(q);
  });

  function matchCount(rulePattern: string) {
    return topics.filter((t) => matchTopicPattern(rulePattern, t.path)).length;
  }

  function toggleTag(id: string) {
    setTagIds((prev) => (prev.includes(id) ? prev.filter((t) => t !== id) : [...prev, id]));
  }

  async function handleCreateProducer(producerName: string) {
    const created = await producerService.insertProducer({ name: producerName });
    toast.success(`Created producer "${created.name}".`);
    refetchProducers();
    return { id: created.id, label: created.name };
  }

  function startEdit(rule: Rule) {
    setEditingId(rule.id);
    setName(rule.name ?? '');
    setPattern(rule.pattern);
    setProducerId(rule.producerId);
    setSchemaId(rule.schemaId);
    setTagIds(rule.tagIds);
    setAutoApply(rule.autoApply);
  }

  function cancelEdit() {
    setEditingId(null);
    setName('');
    setPattern('');
    setProducerId(null);
    setSchemaId(null);
    setTagIds([]);
    setAutoApply(false);
  }

  async function handleSave() {
    if (!pattern.trim()) return;
    try {
      if (editingId) {
        await ruleService.updateRule({ id: editingId, name: name.trim() || null, pattern: pattern.trim(), producerId, schemaId, tagIds, autoApply });
      } else {
        await ruleService.insertRule({ name: name.trim() || null, pattern: pattern.trim(), producerId, schemaId, tagIds, autoApply });
      }
      cancelEdit();
      refetch();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to save rule');
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
        Auto-assign a producer, schema &amp; tags to topics matching an MQTT-style pattern (+ = one level, # = rest).
        Use for bulk-managing many topics from the same producer, or to survive a namespace reshuffle — consumers
        aren't assignable here since who consumes a topic doesn't follow from its pattern the way producer/schema do,
        so that stays a manual per-topic call. A rule marked{' '}
        <Badge tone="cyan" className="!text-[9px]">
          auto
        </Badge>{' '}
        applies itself the moment it's the only rule matching a newly-untracked topic. Saving a rule (new or edited)
        immediately re-applies it to every topic already matching its pattern, tracked or not — so adding a schema to
        a rule later, say, pushes onto topics it already configured without any extra step. "Apply to N now" is there
        for topics that started matching later, without the rule itself being touched again.
      </div>

      <Input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="filter rules…" className="mb-3" />

      <ManageList>
        {filtered.map((rule) => {
          const count = matchCount(rule.pattern);
          return (
            <div key={rule.id} className="flex items-center gap-2.5 rounded-md bg-bg px-2.5 py-2.5">
              <div className="min-w-0 flex-1">
                <div className="flex items-center gap-1.5">
                  <div className="truncate text-[13px] font-semibold text-text">{rule.name ?? rule.pattern}</div>
                  {rule.autoApply && <Badge tone="cyan">auto</Badge>}
                </div>
                {rule.name && <div className="truncate font-mono text-[11px] text-muted">{rule.pattern}</div>}
                <div className="mt-0.5 text-[11.5px] text-muted">
                  producer: {rule.producerId ? (producerById.get(rule.producerId)?.name ?? 'Unknown') : 'Unknown'}
                </div>
                <div className="text-[11.5px] text-muted">
                  schema: {rule.schemaId ? (schemaById.get(rule.schemaId)?.name ?? 'Unknown') : 'None'}
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
              <Button variant="secondary" size="sm" onClick={() => startEdit(rule)}>
                Edit
              </Button>
              <button onClick={() => handleDelete(rule.id)} className="text-base leading-none text-muted hover:text-text">
                ×
              </button>
            </div>
          );
        })}
        {filtered.length === 0 && <div className="px-1 py-1 text-[12.5px] italic text-muted">No rules match.</div>}
      </ManageList>

      <div className="border-t border-border pt-4">
        <div className="mb-1.5 text-[11.5px] font-semibold text-muted">{editingId ? 'Edit rule' : 'New rule'}</div>
        <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="name (optional)" className="mb-2.5" />
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
            onCreate={handleCreateProducer}
          />
        </div>
        <div className="mb-2.5">
          <SearchableCombobox
            options={schemas.map((s) => ({ id: s.id, label: s.name }))}
            selectedId={schemaId}
            onSelect={setSchemaId}
            placeholder="type to search schemas…"
            noneLabel="None"
            maxHeight={120}
          />
        </div>
        <div className="mb-3 flex flex-wrap gap-1.5">
          {tags.map((tag) => (
            <TagPill key={tag.id} tag={tag} active={tagIds.includes(tag.id)} onClick={() => toggleTag(tag.id)} />
          ))}
        </div>
        <label className="mb-3 flex items-start gap-2.5 text-xs text-muted">
          <input
            type="checkbox"
            checked={autoApply}
            onChange={(e) => setAutoApply(e.target.checked)}
            className="mt-0.5 accent-cyan"
          />
          <span>
            <span className="font-medium text-text/90">Auto-apply</span> when this is the only rule matching a
            newly-untracked topic. Multiple matches always require a manual pick regardless.
          </span>
        </label>
        <div className="flex gap-2">
          <Button variant="primary" className="flex-1" onClick={handleSave}>
            {editingId ? 'Save changes' : 'Add rule'}
          </Button>
          {editingId && (
            <Button variant="secondary" onClick={cancelEdit}>
              Cancel
            </Button>
          )}
        </div>
      </div>
    </Modal>
  );
}
