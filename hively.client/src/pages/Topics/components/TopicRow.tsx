import Button from '../../../components/ui/Button';
import { ComplianceBadge, TagPill } from '../../../components/shared';
import { relativeTimeFromDate } from '../../../utils/relativeTime';
import type { Tag, Topic } from '../../../types';

const ROW_GRID = 'grid-cols-[2.2fr_1fr_0.9fr_1.3fr_1fr_1fr]';

interface TopicRowProps {
  topic: Topic;
  producerName: string | null;
  consumerCount: number;
  tagPills: Tag[];
  isAdmin: boolean;
  onOpen: () => void;
}

export default function TopicRow({ topic, producerName, consumerCount, tagPills, isAdmin, onOpen }: TopicRowProps) {
  const lastSeen = relativeTimeFromDate(topic.lastSeenAt) ?? 'never';

  if (!topic.tracked) {
    return (
      <div
        className={`grid ${ROW_GRID} items-center gap-3 border-t border-border/70 bg-amber/10 py-2.5 pl-4 pr-4`}
        style={{ borderLeft: '3px solid oklch(0.6 0.12 80)' }}
      >
        <span className="overflow-hidden text-ellipsis whitespace-nowrap font-mono text-[12.5px] text-amber">
          {topic.path}
        </span>
        <span className="w-fit rounded-md border border-border-strong px-2 py-0.5 text-[10.5px] font-semibold text-amber">
          UNTRACKED
        </span>
        <span className="text-xs text-amber/70">—</span>
        <span className="text-xs text-amber/70">—</span>
        <span className="whitespace-nowrap text-xs text-amber/80">{lastSeen}</span>
        <span className="flex items-center justify-start gap-1.5">
          {isAdmin ? (
            <Button variant="primary" size="sm" onClick={onOpen}>
              Configure →
            </Button>
          ) : (
            <span className="text-[11px] text-amber/70">view only</span>
          )}
        </span>
      </div>
    );
  }

  return (
    <button
      onClick={onOpen}
      className={`grid ${ROW_GRID} w-full items-center gap-3 border-t border-border/70 bg-[oklch(0.155_0_0)] py-2.5 pl-4 pr-4 text-left hover:bg-border/20`}
    >
      <span className="overflow-hidden text-ellipsis whitespace-nowrap font-mono text-[12.5px] text-text">
        {topic.path}
      </span>
      <span className="overflow-hidden text-ellipsis whitespace-nowrap text-[12.5px] text-text/70">
        {producerName ?? '—'}
      </span>
      <span className="text-[12.5px] text-muted">{consumerCount}</span>
      <span className="flex flex-wrap gap-1">
        {tagPills.map((tag) => (
          <TagPill key={tag.id} tag={tag} />
        ))}
      </span>
      <span className="text-xs text-muted">{lastSeen}</span>
      <span>
        <ComplianceBadge topic={topic} />
      </span>
    </button>
  );
}
