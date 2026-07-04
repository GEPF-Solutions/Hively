import { useState } from 'react';
import Modal from '../../../components/ui/Modal';
import Button from '../../../components/ui/Button';
import { SearchableCombobox } from '../../../components/shared';
import type { Schema } from '../../../types';

interface SchemaAssignModalProps {
  path: string;
  schemas: Schema[];
  currentSchemaId: string | null;
  onClose: () => void;
  onSave: (schemaId: string | null) => Promise<void>;
}

export default function SchemaAssignModal({ path, schemas, currentSchemaId, onClose, onSave }: SchemaAssignModalProps) {
  const [schemaId, setSchemaId] = useState(currentSchemaId);
  const [saving, setSaving] = useState(false);

  async function handleSave() {
    setSaving(true);
    try {
      await onSave(schemaId);
      onClose();
    } finally {
      setSaving(false);
    }
  }

  return (
    <Modal
      isOpen
      onClose={onClose}
      title="Assign Schema"
      maxWidth="sm"
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button variant="primary" onClick={handleSave} isLoading={saving}>
            Assign
          </Button>
        </>
      }
    >
      <div className="mb-3.5 font-mono text-xs text-muted">{path}</div>
      <SearchableCombobox
        options={schemas.map((s) => ({ id: s.id, label: s.name }))}
        selectedId={schemaId}
        onSelect={setSchemaId}
        placeholder="type to search schemas…"
        noneLabel="No schema"
        maxHeight={260}
      />
    </Modal>
  );
}
