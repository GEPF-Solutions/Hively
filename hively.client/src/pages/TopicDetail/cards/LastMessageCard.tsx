import Badge from '../../../components/ui/Badge';
import { relativeTimeFromDate } from '../../../utils/relativeTime';
import type { Topic } from '../../../types';

function formatPayload(payload: string | null): string {
  if (!payload) return '(no message received yet)';
  try {
    return JSON.stringify(JSON.parse(payload), null, 2);
  } catch {
    return payload;
  }
}

export default function LastMessageCard({ topic }: { topic: Topic }) {
  return (
    <div className="hv-card p-4">
      <div className="mb-2.5 flex items-center justify-between">
        <div className="text-[11px] font-semibold uppercase tracking-wide text-muted">Last Message</div>
        {topic.retained && (
          <Badge tone="neutral" className="!text-[10.5px]">
            Retained
          </Badge>
        )}
      </div>
      <pre className="max-h-48 overflow-auto rounded-md bg-bg p-3 font-mono text-xs leading-relaxed text-[oklch(0.85_0.02_150)]">
        {formatPayload(topic.lastPayload)}
      </pre>
      <div className="mt-2 text-[11.5px] text-muted">{relativeTimeFromDate(topic.lastSeenAt) ?? 'never seen'}</div>
    </div>
  );
}
