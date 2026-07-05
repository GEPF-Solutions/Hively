import { useState } from 'react';
import Modal from '../../../components/ui/Modal';
import Button from '../../../components/ui/Button';
import Badge from '../../../components/ui/Badge';
import Input from '../../../components/ui/Input';
import {
  ActionToggle,
  HiveWatermark,
  ManageList,
  MultiSelectActionCombobox,
  SearchableCombobox,
  TagPill,
} from '../../../components/shared';
import type { ItemAction, MatchFieldAction } from '../../../components/shared';
import { useMatches } from '../../../hooks/data/useMatches';
import { useProducers } from '../../../hooks/data/useProducers';
import { useConsumers } from '../../../hooks/data/useConsumers';
import { useSchemas } from '../../../hooks/data/useSchemas';
import { useTags } from '../../../hooks/data/useTags';
import { matchService } from '../../../services/matchService';
import { producerService } from '../../../services/producerService';
import { consumerService } from '../../../services/consumerService';
import { useToast } from '../../../contexts/ToastContext';
import type { Match } from '../../../types';

function cycle(actions: Record<string, ItemAction>, id: string): Record<string, ItemAction> {
  const next = { ...actions };
  if (!next[id]) {
    next[id] = 'set';
  } else if (next[id] === 'set') {
    next[id] = 'exclude';
  } else {
    delete next[id];
  }
  return next;
}

function actionsToPayload(actions: Record<string, ItemAction>, key: 'tagId' | 'consumerId') {
  return Object.entries(actions).map(([id, action]) => ({ [key]: id, isExclude: action === 'exclude' }));
}

interface ManageMatchesPanelProps {
  onClose: () => void;
  /** Pre-fills the pattern field — set when opened as "Configure this branch" from the namespace sidebar. */
  initialPattern?: string;
}

export default function ManageMatchesPanel({ onClose, initialPattern }: ManageMatchesPanelProps) {
  const { matches, refetch } = useMatches();
  const { producers, refetch: refetchProducers } = useProducers();
  const { consumers, refetch: refetchConsumers } = useConsumers();
  const { schemas } = useSchemas();
  const { tags } = useTags();
  const toast = useToast();

  const [search, setSearch] = useState('');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [name, setName] = useState('');
  const [pattern, setPattern] = useState(initialPattern ?? '');
  const [producerAction, setProducerAction] = useState<MatchFieldAction>('inherit');
  const [producerId, setProducerId] = useState<string | null>(null);
  const [schemaAction, setSchemaAction] = useState<MatchFieldAction>('inherit');
  const [schemaId, setSchemaId] = useState<string | null>(null);
  const [tagActions, setTagActions] = useState<Record<string, ItemAction>>({});
  const [consumerActions, setConsumerActions] = useState<Record<string, ItemAction>>({});
  const [autoApply, setAutoApply] = useState(false);

  const producerById = new Map(producers.map((p) => [p.id, p]));
  const schemaById = new Map(schemas.map((s) => [s.id, s]));
  const tagById = new Map(tags.map((t) => [t.id, t]));
  const filtered = matches.filter((m) => {
    const q = search.trim().toLowerCase();
    return (m.name ?? '').toLowerCase().includes(q) || m.pattern.toLowerCase().includes(q);
  });

  async function handleCreateProducer(producerName: string) {
    const created = await producerService.insertProducer({ name: producerName });
    toast.success(`Created producer "${created.name}".`);
    refetchProducers();
    return { id: created.id, label: created.name };
  }

  async function handleCreateConsumer(consumerName: string) {
    const created = await consumerService.insertConsumer({ name: consumerName });
    toast.success(`Created consumer "${created.name}".`);
    refetchConsumers();
    return { id: created.id, label: created.name };
  }

  function startEdit(match: Match) {
    setEditingId(match.id);
    setName(match.name ?? '');
    setPattern(match.pattern);
    setProducerAction(match.excludeProducer ? 'exclude' : match.producerId ? 'set' : 'inherit');
    setProducerId(match.producerId);
    setSchemaAction(match.excludeSchema ? 'exclude' : match.schemaId ? 'set' : 'inherit');
    setSchemaId(match.schemaId);
    setTagActions(Object.fromEntries(match.tagActions.map((a) => [a.tagId, a.isExclude ? 'exclude' : 'set'])));
    setConsumerActions(Object.fromEntries(match.consumerActions.map((a) => [a.consumerId, a.isExclude ? 'exclude' : 'set'])));
    setAutoApply(match.autoApply);
  }

  function cancelEdit() {
    setEditingId(null);
    setName('');
    setPattern(initialPattern ?? '');
    setProducerAction('inherit');
    setProducerId(null);
    setSchemaAction('inherit');
    setSchemaId(null);
    setTagActions({});
    setConsumerActions({});
    setAutoApply(false);
  }

  async function handleSave() {
    if (!pattern.trim()) return;
    const payload = {
      name: name.trim() || null,
      pattern: pattern.trim(),
      producerId: producerAction === 'set' ? producerId : null,
      excludeProducer: producerAction === 'exclude',
      schemaId: schemaAction === 'set' ? schemaId : null,
      excludeSchema: schemaAction === 'exclude',
      tagActions: actionsToPayload(tagActions, 'tagId'),
      consumerActions: actionsToPayload(consumerActions, 'consumerId'),
      autoApply,
    };
    try {
      if (editingId) {
        await matchService.updateMatch({ ...payload, id: editingId, topicId: null, updatedAt: '' });
      } else {
        await matchService.insertMatch(payload);
      }
      cancelEdit();
      refetch();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to save match');
    }
  }

  async function handleDelete(id: string) {
    try {
      await matchService.deleteMatch(id);
      refetch();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to delete match');
    }
  }

  function summarizeMatch(match: Match): string {
    const parts: string[] = [];
    if (match.excludeProducer) parts.push('⊘ producer');
    else if (match.producerId) parts.push(`producer: ${producerById.get(match.producerId)?.name ?? 'Unknown'}`);
    if (match.excludeSchema) parts.push('⊘ schema');
    else if (match.schemaId) parts.push(`schema: ${schemaById.get(match.schemaId)?.name ?? 'Unknown'}`);
    const excludedConsumers = match.consumerActions.filter((a) => a.isExclude).length;
    if (excludedConsumers > 0) parts.push(`⊘ ${excludedConsumers} consumer${excludedConsumers > 1 ? 's' : ''}`);
    return parts.join(' · ');
  }

  return (
    <Modal
      isOpen
      onClose={onClose}
      title="Manage Matches"
      maxWidth="lg"
      footer={<Button onClick={onClose}>Done</Button>}
    >
      <div className="mb-4 text-xs leading-relaxed text-muted">
        Assign a producer, schema, consumers &amp; tags to topics matching an MQTT-style pattern (+ = one level,
        # = rest) — the same pattern you get by browsing a branch in the namespace sidebar and choosing "Configure
        this branch", or by typing one here directly for a cross-cutting case. Each field can be left to{' '}
        <span className="font-medium text-text/90">inherit</span> from a less-specific match, explicitly{' '}
        <span className="font-medium text-gold">set</span>, or explicitly{' '}
        <span className="font-medium text-red">excluded</span> — when two matches disagree on the same field or
        value, the more specific pattern always wins, automatically, with nothing to pick. A match marked{' '}
        <Badge tone="cyan" className="!text-[9px]">
          auto
        </Badge>{' '}
        tracks and resolves a newly-untracked topic the moment it covers that topic's path. Saving (new or edited)
        immediately re-applies it to every topic already matching its pattern, tracked or not.
      </div>

      <Input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="filter matches…" className="mb-3" />

      <ManageList>
        {filtered.map((match) => (
          <div key={match.id} className="flex items-center gap-2.5 rounded-md bg-bg px-2.5 py-2.5">
            <div className="min-w-0 flex-1">
              <div className="flex items-center gap-1.5">
                <div className={`min-w-0 flex-1 truncate text-[13px] font-semibold text-text ${match.name ? '' : 'font-mono'}`}>
                  {match.name ?? match.pattern}
                </div>
                {match.autoApply && <Badge tone="cyan" className="shrink-0">auto</Badge>}
              </div>
              {match.name && <div className="truncate font-mono text-[11px] text-muted">{match.pattern}</div>}
              <div className="mt-0.5 text-[11.5px] text-muted">{summarizeMatch(match) || 'no fields set'}</div>
              <div className="mt-1.5 flex flex-wrap gap-1">
                {match.tagActions
                  .filter((a) => !a.isExclude)
                  .map((a) => {
                    const tag = tagById.get(a.tagId);
                    return tag ? <TagPill key={a.tagId} tag={tag} /> : null;
                  })}
                {match.tagActions
                  .filter((a) => a.isExclude)
                  .map((a) => (
                    <span key={a.tagId} className="rounded-md border border-red px-2 py-0.5 font-mono text-[11px] text-red">
                      ⊘ {tagById.get(a.tagId)?.label ?? a.tagId}
                    </span>
                  ))}
              </div>
            </div>
            <Button variant="secondary" size="sm" onClick={() => startEdit(match)}>
              Edit
            </Button>
            <button onClick={() => handleDelete(match.id)} className="text-base leading-none text-muted hover:text-text">
              ×
            </button>
          </div>
        ))}
        {filtered.length === 0 && (
          <div className="relative overflow-hidden rounded-md px-3 py-6 text-center">
            <HiveWatermark size={160} />
            <div className="relative text-[12.5px] italic text-muted">No matches match.</div>
          </div>
        )}
      </ManageList>

      <div className="border-t border-border pt-4">
        <div className="mb-1.5 text-[11.5px] font-semibold text-muted">{editingId ? 'Edit match' : 'New match'}</div>
        <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="name (optional)" className="mb-2.5" />
        <Input
          value={pattern}
          onChange={(e) => setPattern(e.target.value)}
          placeholder="acme/+/+/+/+/+/press-01/#"
          className="mb-3 font-mono"
        />

        <div className="mb-3">
          <div className="mb-1 flex items-center justify-between">
            <div className="text-[11.5px] font-semibold text-muted">Producer</div>
            <ActionToggle value={producerAction} onChange={setProducerAction} />
          </div>
          {producerAction === 'set' && (
            <SearchableCombobox
              options={producers.map((p) => ({ id: p.id, label: p.name }))}
              selectedId={producerId}
              onSelect={setProducerId}
              placeholder="type to search producers…"
              maxHeight={120}
              onCreate={handleCreateProducer}
            />
          )}
        </div>

        <div className="mb-3">
          <div className="mb-1 flex items-center justify-between">
            <div className="text-[11.5px] font-semibold text-muted">Schema</div>
            <ActionToggle value={schemaAction} onChange={setSchemaAction} />
          </div>
          {schemaAction === 'set' && (
            <SearchableCombobox
              options={schemas.map((s) => ({ id: s.id, label: s.name }))}
              selectedId={schemaId}
              onSelect={setSchemaId}
              placeholder="type to search schemas…"
              maxHeight={120}
            />
          )}
        </div>

        <div className="mb-3">
          <div className="mb-1 text-[11.5px] font-semibold text-muted">Consumers</div>
          <MultiSelectActionCombobox
            options={consumers.map((c) => ({ id: c.id, label: c.name }))}
            actions={consumerActions}
            onCycle={(id) => setConsumerActions((prev) => cycle(prev, id))}
            placeholder="type to search consumers…"
            maxHeight={120}
            onCreate={handleCreateConsumer}
          />
        </div>

        <div className="mb-3">
          <div className="mb-1 text-[11.5px] font-semibold text-muted">Tags</div>
          <MultiSelectActionCombobox
            options={tags.map((t) => ({ id: t.id, label: t.label }))}
            actions={tagActions}
            onCycle={(id) => setTagActions((prev) => cycle(prev, id))}
            placeholder="type to search tags…"
            maxHeight={120}
          />
        </div>

        <label className="mb-3 flex items-start gap-2.5 text-xs text-muted">
          <input
            type="checkbox"
            checked={autoApply}
            onChange={(e) => setAutoApply(e.target.checked)}
            className="mt-0.5 accent-gold"
          />
          <span>
            <span className="font-medium text-text/90">Auto-apply</span> when this pattern covers a
            newly-untracked topic. Resolution always combines with every other applicable match automatically —
            nothing to pick.
          </span>
        </label>
        <div className="flex gap-2">
          <Button variant="primary" className="flex-1" onClick={handleSave}>
            {editingId ? 'Save changes' : 'Add match'}
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
