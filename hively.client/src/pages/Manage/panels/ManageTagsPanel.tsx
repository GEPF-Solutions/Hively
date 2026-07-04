import { useState } from 'react';
import Modal from '../../../components/ui/Modal';
import Button from '../../../components/ui/Button';
import Input from '../../../components/ui/Input';
import { TagPill } from '../../../components/shared';
import { tagColor } from '../../../utils/tagColor';
import { useTags } from '../../../hooks/data/useTags';
import { tagService } from '../../../services/tagService';
import { useToast } from '../../../contexts/ToastContext';

// Fixed swatch of preset hues offered when creating a tag, plus neutral (null = gray).
const COLOR_SWATCHES: (number | null)[] = [null, 200, 25, 300, 240, 150, 80, 320];

function slugify(label: string): string {
  return label
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '');
}

export default function ManageTagsPanel({ onClose }: { onClose: () => void }) {
  const { tags, refetch } = useTags();
  const toast = useToast();
  const [newLabel, setNewLabel] = useState('');
  const [newHue, setNewHue] = useState<number | null>(COLOR_SWATCHES[1]);

  async function handleAdd() {
    const label = newLabel.trim();
    if (!label) return;
    try {
      await tagService.insertTag({ id: slugify(label), label, hue: newHue });
      setNewLabel('');
      refetch();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to add tag');
    }
  }

  async function handleDelete(id: string) {
    try {
      await tagService.deleteTag(id);
      refetch();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to delete tag');
    }
  }

  return (
    <Modal isOpen onClose={onClose} title="Manage Tags" maxWidth="sm" footer={<Button onClick={onClose}>Done</Button>}>
      <div className="mb-5 flex max-h-60 flex-col gap-2 overflow-y-auto">
        {tags.map((tag) => (
          <div key={tag.id} className="flex items-center gap-2.5 rounded-md bg-bg px-2.5 py-2">
            <TagPill tag={tag} />
            <div className="flex-1" />
            <button onClick={() => handleDelete(tag.id)} className="text-base leading-none text-muted hover:text-text">
              ×
            </button>
          </div>
        ))}
      </div>

      <div className="border-t border-border pt-4">
        <div className="mb-1.5 text-[11.5px] font-semibold text-muted">New tag</div>
        <Input value={newLabel} onChange={(e) => setNewLabel(e.target.value)} placeholder="label" className="mb-2.5" />
        <div className="mb-3.5 flex gap-1.5">
          {COLOR_SWATCHES.map((hue, i) => {
            const c = tagColor(hue);
            const selected = newHue === hue;
            return (
              <button
                key={i}
                onClick={() => setNewHue(hue)}
                className="h-6 w-6 rounded-full"
                style={{ background: c.bg, border: `2px solid ${selected ? c.color : c.borderColor}` }}
              />
            );
          })}
        </div>
        <Button variant="primary" className="w-full" onClick={handleAdd}>
          Add tag
        </Button>
      </div>
    </Modal>
  );
}
