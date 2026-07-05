import { useState } from 'react';
import Modal from '../../../components/ui/Modal';
import Button from '../../../components/ui/Button';
import Input from '../../../components/ui/Input';
import { ManageList } from '../../../components/shared';
import { useSchemas } from '../../../hooks/data/useSchemas';
import { useTopics } from '../../../hooks/data/useTopics';
import { schemaService } from '../../../services/schemaService';
import { useToast } from '../../../contexts/ToastContext';

// Definitions can nest (a field's value is either a leaf type name or another
// field->type object to recurse into) — flatten to dot-paths for this one-line
// summary; the actual stored definition keeps its real nested shape.
function flattenFields(value: unknown, prefix: string, out: string[]): void {
  if (typeof value !== 'object' || value === null || Array.isArray(value)) {
    return;
  }
  for (const [key, child] of Object.entries(value as Record<string, unknown>)) {
    const path = prefix ? `${prefix}.${key}` : key;
    if (typeof child === 'string') {
      out.push(`${path}: ${child}`);
    } else {
      flattenFields(child, path, out);
    }
  }
}

function fieldsPreview(definition: string): string {
  try {
    const parsed = JSON.parse(definition);
    const fields: string[] = [];
    flattenFields(parsed, '', fields);
    return fields.join(', ');
  } catch {
    return definition;
  }
}

export default function ManageSchemasPanel({ onClose }: { onClose: () => void }) {
  const { schemas, refetch } = useSchemas();
  const { topics } = useTopics();
  const toast = useToast();
  const [search, setSearch] = useState('');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [name, setName] = useState('');
  const [defText, setDefText] = useState('');
  const [defError, setDefError] = useState<string | null>(null);

  const filtered = schemas.filter((s) => s.name.toLowerCase().includes(search.trim().toLowerCase()));

  function usageCount(schemaId: string) {
    return topics.filter((t) => t.schemaId === schemaId).length;
  }

  function startEdit(id: string, currentName: string, currentDefinition: string) {
    setEditingId(id);
    setName(currentName);
    try {
      setDefText(JSON.stringify(JSON.parse(currentDefinition), null, 2));
    } catch {
      setDefText(currentDefinition);
    }
    setDefError(null);
  }

  function cancelEdit() {
    setEditingId(null);
    setName('');
    setDefText('');
    setDefError(null);
  }

  async function handleSave() {
    const trimmedName = name.trim();
    if (!trimmedName || !defText.trim()) return;

    let parsed: unknown;
    try {
      parsed = JSON.parse(defText);
    } catch {
      setDefError('Invalid JSON — expected e.g. { "value": "number", "unit": "string" }');
      return;
    }
    if (typeof parsed !== 'object' || parsed === null || Array.isArray(parsed)) {
      setDefError('Definition must be an object of field -> type (nest an object to check its sub-fields)');
      return;
    }
    setDefError(null);

    try {
      const definition = JSON.stringify(parsed);
      if (editingId) {
        await schemaService.updateSchema({ id: editingId, name: trimmedName, definition, version: null });
      } else {
        await schemaService.insertSchema({ name: trimmedName, definition });
      }
      cancelEdit();
      refetch();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to save schema');
    }
  }

  async function handleDelete(id: string) {
    try {
      await schemaService.deleteSchema(id);
      refetch();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to delete schema');
    }
  }

  return (
    <Modal isOpen onClose={onClose} title="Manage Schemas" maxWidth="md" footer={<Button onClick={onClose}>Done</Button>}>
      <div className="mb-4 text-xs leading-relaxed text-muted">
        Definitions mirror the payload's shape — replace each value with its expected type:{' '}
        <span className="font-mono text-text/80">"string"</span>,{' '}
        <span className="font-mono text-text/80">"number"</span>,{' '}
        <span className="font-mono text-text/80">"boolean"</span>. Nest an object to check its sub-fields, or use{' '}
        <span className="font-mono text-text/80">"object"</span>/<span className="font-mono text-text/80">"array"</span>/
        <span className="font-mono text-text/80">"null"</span> to require a field exist as that kind without checking
        inside it, or <span className="font-mono text-text/80">"any"</span> for any kind at all. Combine types with{' '}
        <span className="font-mono text-text/80">"|"</span> for a field that's genuinely one of a few (e.g.{' '}
        <span className="font-mono text-text/80">"string|number"</span>). Add{' '}
        <span className="font-mono text-text/80">"?"</span> (e.g. <span className="font-mono text-text/80">"string?"</span>)
        to make a field optional — missing or null is fine, but a present value still has to match.
      </div>

      <Input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="filter schemas…" className="mb-3 font-mono" />

      <ManageList>
        {filtered.map((s) => (
          <div key={s.id} className="flex items-center gap-2.5 rounded-md bg-bg px-2.5 py-2">
            <div className="min-w-0 flex-1">
              <div className="text-[13px] font-semibold text-text">
                {s.name} <span className="font-mono text-[11px] font-normal text-muted">{s.version}</span>
              </div>
              <div className="truncate font-mono text-[11px] text-muted">{fieldsPreview(s.definition)}</div>
              <div className="mt-0.5 text-[10.5px] text-muted/70">used by {usageCount(s.id)} topics</div>
            </div>
            <Button variant="secondary" size="sm" onClick={() => startEdit(s.id, s.name, s.definition)}>
              Edit
            </Button>
            <button onClick={() => handleDelete(s.id)} className="text-base leading-none text-muted hover:text-text">
              ×
            </button>
          </div>
        ))}
        {filtered.length === 0 && <div className="px-1 py-1 text-[12.5px] italic text-muted">No schemas match.</div>}
      </ManageList>

      <div className="border-t border-border pt-4">
        <div className="mb-1.5 text-[11.5px] font-semibold text-muted">{editingId ? 'Edit schema' : 'New schema'}</div>
        <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="schema name" className="mb-2.5" />
        <textarea
          value={defText}
          onChange={(e) => setDefText(e.target.value)}
          placeholder='{ "value": "number", "unit": "string", "meta": { "name": "string" } }'
          rows={7}
          className="mb-2.5 w-full resize-y facet-sm facet-border-strong bg-bg px-2.5 py-2 font-mono text-xs text-cyan outline-none placeholder:text-muted focus:facet-accent"
        />
        {defError && <div className="mb-2.5 text-xs text-red">{defError}</div>}
        <div className="flex gap-2">
          <Button variant="primary" className="flex-1" onClick={handleSave}>
            {editingId ? 'Save new version' : 'Add schema'}
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
