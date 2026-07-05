import { useState } from 'react';
import Modal from '../../../components/ui/Modal';
import Button from '../../../components/ui/Button';
import Input from '../../../components/ui/Input';
import { ManageList } from '../../../components/shared';
import { useConsumers } from '../../../hooks/data/useConsumers';
import { useTopics } from '../../../hooks/data/useTopics';
import { consumerService } from '../../../services/consumerService';
import { useToast } from '../../../contexts/ToastContext';

export default function ManageConsumersPanel({ onClose }: { onClose: () => void }) {
  const { consumers, refetch } = useConsumers();
  const { topics } = useTopics();
  const toast = useToast();
  const [search, setSearch] = useState('');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');

  const filtered = consumers.filter((c) => c.name.toLowerCase().includes(search.trim().toLowerCase()));

  function usageCount(consumerId: string) {
    return topics.filter((t) => t.consumerIds.includes(consumerId)).length;
  }

  function startEdit(id: string, currentName: string, currentDescription: string | null) {
    setEditingId(id);
    setName(currentName);
    setDescription(currentDescription ?? '');
  }

  function cancelEdit() {
    setEditingId(null);
    setName('');
    setDescription('');
  }

  async function handleSave() {
    const trimmed = name.trim();
    if (!trimmed) return;
    try {
      if (editingId) {
        await consumerService.updateConsumer({ id: editingId, name: trimmed, description: description.trim() || null });
      } else {
        await consumerService.insertConsumer({ name: trimmed, description: description.trim() || null });
      }
      cancelEdit();
      refetch();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to save consumer');
    }
  }

  async function handleDelete(id: string) {
    try {
      await consumerService.deleteConsumer(id);
      refetch();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to delete consumer');
    }
  }

  return (
    <Modal isOpen onClose={onClose} title="Manage Consumers" maxWidth="sm" footer={<Button onClick={onClose}>Done</Button>}>
      <Input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="filter consumers…" className="mb-3" />

      <ManageList>
        {filtered.map((c) => (
          <div key={c.id} className="flex items-center gap-2.5 rounded-md bg-bg px-2.5 py-2">
            <div className="min-w-0 flex-1">
              <div className="truncate text-[12.5px] text-text">{c.name}</div>
              {c.description && <div className="mt-0.5 text-[11.5px] text-muted">{c.description}</div>}
              <div className="mt-0.5 text-[10.5px] text-muted/70">used by {usageCount(c.id)} topics</div>
            </div>
            <Button variant="secondary" size="sm" onClick={() => startEdit(c.id, c.name, c.description)}>
              Edit
            </Button>
            <button onClick={() => handleDelete(c.id)} className="text-base leading-none text-muted hover:text-text">
              ×
            </button>
          </div>
        ))}
        {filtered.length === 0 && <div className="px-1 py-1 text-[12.5px] italic text-muted">No consumers match.</div>}
      </ManageList>

      <div className="border-t border-border pt-4">
        <div className="mb-1.5 text-[11.5px] font-semibold text-muted">{editingId ? 'Edit consumer' : 'New consumer'}</div>
        <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="consumer name" className="mb-2" />
        <textarea
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="description (optional)"
          rows={2}
          className="mb-2.5 w-full resize-y facet-sm facet-border-strong bg-bg px-2.5 py-2 text-xs text-text/90 outline-none placeholder:text-muted focus:facet-accent"
        />
        <div className="flex gap-2">
          <Button variant="primary" className="flex-1" onClick={handleSave}>
            {editingId ? 'Save changes' : 'Add consumer'}
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
