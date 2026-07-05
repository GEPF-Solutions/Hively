import { useState } from 'react';
import Button from '../../../components/ui/Button';
import { MultiSelectCombobox } from '../../../components/shared';
import { useConsumers } from '../../../hooks/data/useConsumers';
import { consumerService } from '../../../services/consumerService';
import { useToast } from '../../../contexts/ToastContext';
import type { Consumer } from '../../../types';

interface ConsumerCardProps {
  consumers: Consumer[];
  canEdit: boolean;
  onToggle: (consumerId: string) => void;
}

export default function ConsumerCard({ consumers, canEdit, onToggle }: ConsumerCardProps) {
  const { consumers: allConsumers, refetch } = useConsumers();
  const toast = useToast();
  const [pickerOpen, setPickerOpen] = useState(false);

  async function handleCreateConsumer(name: string) {
    const created = await consumerService.insertConsumer({ name });
    toast.success(`Created consumer "${created.name}".`);
    refetch();
    return { id: created.id, label: created.name };
  }

  return (
    <div className="hv-card p-4">
      <div className="mb-2.5 flex items-center justify-between">
        <div className="text-[11px] font-semibold uppercase tracking-wide text-muted">
          Consumers ({consumers.length})
        </div>
        {canEdit && (
          <Button variant="secondary" size="sm" onClick={() => setPickerOpen((v) => !v)}>
            + consumer
          </Button>
        )}
      </div>

      <div
        className="max-h-32 overflow-y-auto"
        style={
          consumers.length > 0
            ? {
                maskImage: 'linear-gradient(to bottom, black calc(100% - 14px), transparent 100%)',
                WebkitMaskImage: 'linear-gradient(to bottom, black calc(100% - 14px), transparent 100%)',
              }
            : undefined
        }
      >
        {consumers.map((c) => (
          <div key={c.id} title={c.description ?? undefined} className="flex items-center gap-2 py-0.5 text-sm">
            <span className="h-1.5 w-1.5 flex-shrink-0 rounded-full bg-cyan" />
            <span className="truncate">{c.name}</span>
          </div>
        ))}
        {consumers.length === 0 && <div className="text-[12.5px] italic text-muted">no known consumers</div>}
      </div>

      {pickerOpen && (
        <div className="mt-2.5 border-t border-border pt-2.5">
          <MultiSelectCombobox
            options={allConsumers.map((c) => ({ id: c.id, label: c.name }))}
            selectedIds={consumers.map((c) => c.id)}
            onToggle={onToggle}
            placeholder="type to search consumers…"
            onCreate={handleCreateConsumer}
          />
        </div>
      )}
    </div>
  );
}
