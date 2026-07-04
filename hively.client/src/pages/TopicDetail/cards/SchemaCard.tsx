import { useEffect, useState } from 'react';
import Button from '../../../components/ui/Button';
import { schemaService } from '../../../services/schemaService';
import type { Schema, SchemaVersion } from '../../../types';

interface SchemaCardProps {
  schema: Schema | null;
  canChange: boolean;
  onChangeClick: () => void;
}

function formatDefinition(definition: string): string {
  try {
    return JSON.stringify(JSON.parse(definition), null, 2);
  } catch {
    return definition;
  }
}

export default function SchemaCard({ schema, canChange, onChangeClick }: SchemaCardProps) {
  const [history, setHistory] = useState<SchemaVersion[]>([]);

  useEffect(() => {
    if (!schema) {
      setHistory([]);
      return;
    }
    schemaService.getSchemaVersions(schema.id).then(setHistory).catch(() => setHistory([]));
  }, [schema]);

  return (
    <div className="rounded-[10px] border border-border bg-panel p-4">
      <div className="mb-2.5 flex items-center justify-between">
        <div className="text-[11px] font-semibold uppercase tracking-wide text-muted">Schema</div>
        <div className="flex items-center gap-2.5">
          {schema?.version && <span className="font-mono text-[11px] text-muted">{schema.version}</span>}
          {canChange && (
            <Button variant="secondary" size="sm" onClick={onChangeClick}>
              {schema ? 'Change' : 'Assign'}
            </Button>
          )}
        </div>
      </div>

      {schema ? (
        <>
          <div className="mb-2.5 text-[12.5px] text-text/80">{schema.name}</div>
          <pre className="overflow-x-auto rounded-md bg-bg p-3 font-mono text-xs leading-relaxed text-cyan">
            {formatDefinition(schema.definition)}
          </pre>
          <div className="mt-3 flex max-h-24 flex-col gap-1.5 overflow-y-auto pr-1">
            {history.map((h, i) => (
              <div key={h.id} className="flex items-center gap-2 text-[11.5px] text-muted">
                <span className="h-1.5 w-1.5 rounded-full" style={{ background: i === 0 ? 'oklch(0.75 0.13 200)' : 'oklch(0.45 0.012 254)' }} />
                <span className="font-mono">{h.version}</span> · {new Date(h.createdAt).toLocaleDateString()}
                {i === 0 && <span className="font-semibold text-cyan">current</span>}
              </div>
            ))}
          </div>
        </>
      ) : (
        <div className="text-[12.5px] italic text-muted">no schema assigned — compliance is unknown</div>
      )}
    </div>
  );
}
