import { useState } from 'react';
import Modal from '../../../components/ui/Modal';
import Button from '../../../components/ui/Button';
import Input from '../../../components/ui/Input';
import { ManageList } from '../../../components/shared';
import { useProducers } from '../../../hooks/data/useProducers';
import { useTopics } from '../../../hooks/data/useTopics';
import { producerService } from '../../../services/producerService';
import { useToast } from '../../../contexts/ToastContext';

export default function ManageProducersPanel({ onClose }: { onClose: () => void }) {
  const { producers, refetch } = useProducers();
  const { topics } = useTopics();
  const toast = useToast();
  const [search, setSearch] = useState('');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');

  const filtered = producers.filter((p) => p.name.toLowerCase().includes(search.trim().toLowerCase()));

  function usageCount(producerId: string) {
    return topics.filter((t) => t.producerId === producerId).length;
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
        await producerService.updateProducer({ id: editingId, name: trimmed, description: description.trim() || null });
      } else {
        await producerService.insertProducer({ name: trimmed, description: description.trim() || null });
      }
      cancelEdit();
      refetch();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to save producer');
    }
  }

  async function handleDelete(id: string) {
    try {
      await producerService.deleteProducer(id);
      refetch();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to delete producer');
    }
  }

  return (
    <Modal isOpen onClose={onClose} title="Manage Producers" maxWidth="sm" footer={<Button onClick={onClose}>Done</Button>}>
      <Input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="filter producers…" className="mb-3 font-mono" />

      <ManageList>
        {filtered.map((p) => (
          <div key={p.id} className="flex items-center gap-2.5 rounded-md bg-bg px-2.5 py-2">
            <div className="min-w-0 flex-1">
              <div className="truncate font-mono text-[12.5px] text-text">{p.name}</div>
              {p.description && <div className="mt-0.5 text-[11.5px] text-muted">{p.description}</div>}
              <div className="mt-0.5 text-[10.5px] text-muted/70">used by {usageCount(p.id)} topics</div>
            </div>
            <Button variant="secondary" size="sm" onClick={() => startEdit(p.id, p.name, p.description)}>
              Edit
            </Button>
            <button onClick={() => handleDelete(p.id)} className="text-base leading-none text-muted hover:text-text">
              ×
            </button>
          </div>
        ))}
        {filtered.length === 0 && <div className="px-1 py-1 text-[12.5px] italic text-muted">No producers match.</div>}
      </ManageList>

      <div className="border-t border-border pt-4">
        <div className="mb-1.5 text-[11.5px] font-semibold text-muted">{editingId ? 'Edit producer' : 'New producer'}</div>
        <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="producer name" className="mb-2 font-mono" />
        <textarea
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="description (optional)"
          rows={2}
          className="mb-2.5 w-full resize-y rounded-md border border-border-strong bg-bg px-2.5 py-2 text-xs text-text/90 outline-none placeholder:text-muted focus:border-cyan"
        />
        <div className="flex gap-2">
          <Button variant="primary" className="flex-1" onClick={handleSave}>
            {editingId ? 'Save changes' : 'Add producer'}
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
