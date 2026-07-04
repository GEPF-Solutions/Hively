import { ComplianceBadge, TagPill } from '../../../components/shared';
import { relativeTimeFromDate } from '../../../utils/relativeTime';
import type { RuleMatch, Tag, Topic } from '../../../types';

const ROW_GRID = 'grid-cols-[2.2fr_1fr_0.9fr_1.3fr_1fr_1fr]';

interface TopicRowProps {
  topic: Topic;
  producerName: string | null;
  consumerCount: number;
  tagPills: Tag[];
  isAdmin: boolean;
  matchingRules: RuleMatch[];
  onOpen: () => void;
  onQuickApplyRule: (ruleId: string) => void;
}

export default function TopicRow({
  topic,
  producerName,
  consumerCount,
  tagPills,
  isAdmin,
  matchingRules,
  onOpen,
  onQuickApplyRule,
}: TopicRowProps) {
  const lastSeen = relativeTimeFromDate(topic.lastSeenAt) ?? 'never';

  if (!topic.tracked) {
    const hasMatchingRule = matchingRules.length === 1;
    const hasRuleConflict = matchingRules.length > 1;

    return (
      <div
        className={`grid ${ROW_GRID} items-center gap-3 border-t border-border/70 bg-amber/10 py-2.5 pl-4 pr-4`}
        style={{ borderLeft: '3px solid oklch(0.6 0.12 80)' }}
      >
        <span className="overflow-hidden text-ellipsis whitespace-nowrap font-mono text-[12.5px] text-amber">
          {topic.path}
        </span>
        <span className="w-fit rounded-full bg-amber/30 px-2 py-0.5 text-[10.5px] font-semibold text-amber">
          UNTRACKED
        </span>
        <span className="text-xs text-amber/70">—</span>
        <span className="text-xs text-amber/70">—</span>
        <span className="whitespace-nowrap text-xs text-amber/80">{lastSeen}</span>
        <span className="flex items-center justify-start gap-1.5">
          {isAdmin ? (
            <>
              {hasMatchingRule && (
                <button
                  onClick={() => onQuickApplyRule(matchingRules[0].rule.id)}
                  className="whitespace-nowrap rounded-md border border-amber/60 px-2.5 py-1.5 text-[11.5px] font-medium text-amber"
                >
                  ⚡ Apply rule
                </button>
              )}
              {hasRuleConflict && (
                <span
                  title="Multiple rules match this topic — open Configure to choose"
                  className="whitespace-nowrap rounded-md bg-red/20 px-2 py-1 text-[11px] font-semibold text-red"
                >
                  ⚠ {matchingRules.length} rules match
                </span>
              )}
              <button
                onClick={onOpen}
                className="whitespace-nowrap rounded-md bg-amber px-3 py-1.5 text-xs font-medium text-[oklch(0.18_0.02_80)]"
              >
                Configure →
              </button>
            </>
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
      className={`grid ${ROW_GRID} w-full items-center gap-3 border-t border-border/70 bg-[oklch(0.185_0.014_254)] py-2.5 pl-4 pr-4 text-left hover:bg-border/20`}
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
