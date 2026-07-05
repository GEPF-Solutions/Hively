import Button from '../../../components/ui/Button';
import { relativeTimeFromDate } from '../../../utils/relativeTime';
import type { Topic } from '../../../types';

interface ComplianceCardProps {
  topic: Topic;
  canClear: boolean;
  onClear: () => void;
}

export default function ComplianceCard({ topic, canClear, onClear }: ComplianceCardProps) {
  const violationColor = topic.violationCount > 0 ? 'oklch(0.7 0.18 25)' : 'oklch(0.75 0.13 200)';

  return (
    <div className="flex h-full flex-col rounded-[10px] border border-border bg-panel p-4">
      <div className="mb-2.5 text-[11px] font-semibold uppercase tracking-wide text-muted">Compliance</div>
      <div className="flex flex-1 flex-col justify-center">
        <div className="flex items-center gap-4">
          <div>
            <div className="font-mono text-3xl font-bold" style={{ color: violationColor }}>
              {topic.violationCount}
            </div>
            <div className="text-[11px] text-muted">violations since last cleared</div>
            <div className="mt-0.5 text-[11px] text-muted/80">
              last cleared: {relativeTimeFromDate(topic.lastClearedAt) ?? 'never cleared'}
            </div>
          </div>
          {canClear && topic.violationCount > 0 && (
            <Button variant="secondary" size="sm" className="ml-auto" onClick={onClear}>
              Clear
            </Button>
          )}
        </div>
        {topic.mismatches.length > 0 && (
          <div className="mt-3 flex flex-col gap-1 border-t border-border pt-3">
            <div className="mb-0.5 text-[11px] text-muted">current message fails schema:</div>
            {topic.mismatches.map((m) => (
              <div key={m} className="font-mono text-[11.5px] text-red">
                · {m}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
