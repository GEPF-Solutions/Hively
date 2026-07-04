import { useState } from 'react';
import Modal from '../../../components/ui/Modal';
import Button from '../../../components/ui/Button';
import { SearchableCombobox } from '../../../components/shared';
import type { Producer } from '../../../types';

interface ProducerAssignModalProps {
  path: string;
  producers: Producer[];
  currentProducerId: string | null;
  onClose: () => void;
  onSave: (producerId: string | null) => Promise<void>;
}

export default function ProducerAssignModal({
  path,
  producers,
  currentProducerId,
  onClose,
  onSave,
}: ProducerAssignModalProps) {
  const [producerId, setProducerId] = useState(currentProducerId);
  const [saving, setSaving] = useState(false);

  async function handleSave() {
    setSaving(true);
    try {
      await onSave(producerId);
      onClose();
    } finally {
      setSaving(false);
    }
  }

  return (
    <Modal
      isOpen
      onClose={onClose}
      title="Change Producer"
      maxWidth="sm"
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button variant="primary" onClick={handleSave} isLoading={saving}>
            Save
          </Button>
        </>
      }
    >
      <div className="mb-3.5 font-mono text-xs text-muted">{path}</div>
      <SearchableCombobox
        options={producers.map((p) => ({ id: p.id, label: p.name }))}
        selectedId={producerId}
        onSelect={setProducerId}
        placeholder="type to search producers…"
        noneLabel="Unknown"
        maxHeight={260}
      />
    </Modal>
  );
}
