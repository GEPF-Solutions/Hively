import Button from '../../../components/ui/Button';
import type { Producer } from '../../../types';

interface ProducerCardProps {
  producer: Producer | null;
  canChange: boolean;
  onChangeClick: () => void;
}

export default function ProducerCard({ producer, canChange, onChangeClick }: ProducerCardProps) {
  return (
    <div className="flex h-full flex-col rounded-[10px] border border-border bg-panel p-4">
      <div className="mb-2.5 flex items-center justify-between">
        <div className="text-[11px] font-semibold uppercase tracking-wide text-muted">Producer</div>
        {canChange && (
          <Button variant="secondary" size="sm" onClick={onChangeClick}>
            Change
          </Button>
        )}
      </div>
      <div className="flex flex-1 flex-col justify-center">
        <div className="font-mono text-sm font-semibold text-text">{producer?.name ?? 'Unknown'}</div>
        {producer?.description && <div className="mt-1 text-xs leading-relaxed text-muted">{producer.description}</div>}
      </div>
    </div>
  );
}
