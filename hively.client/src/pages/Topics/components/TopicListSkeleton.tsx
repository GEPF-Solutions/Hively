const ROW_GRID = 'grid-cols-[2.2fr_1fr_0.9fr_1.3fr_1fr_1fr]';

function Bar({ width, height = 'h-3', delay = 0 }: { width: string; height?: string; delay?: number }) {
  return (
    <div
      className={`animate-pulse motion-reduce:animate-none rounded-sm bg-border ${height} ${width}`}
      style={delay ? { animationDelay: `${delay}ms` } : undefined}
    />
  );
}

function SkeletonRow({ delay }: { delay: number }) {
  return (
    <div className={`grid ${ROW_GRID} items-center gap-3 border-t border-border/70 py-2.5 pl-4 pr-4`}>
      <Bar width="w-40" delay={delay} />
      <Bar width="w-20" delay={delay} />
      <Bar width="w-6" delay={delay} />
      <Bar width="w-14" height="h-5" delay={delay} />
      <Bar width="w-16" delay={delay} />
      <Bar width="w-14" height="h-5" delay={delay} />
    </div>
  );
}

// Mirrors the real Hierarchy view: several small groups, each with its own
// breadcrumb-style label and its own bordered header+rows block — not one
// giant flat table, which is what made the first pass feel off.
function SkeletonGroup({ rows, delay }: { rows: number; delay: number }) {
  return (
    <div className="mb-5">
      <div className="mb-2 pl-1">
        <Bar width="w-56" delay={delay} />
      </div>
      <div className="overflow-hidden rounded-[10px] border border-border">
        <div className={`grid ${ROW_GRID} gap-3 bg-panel px-4 py-2.5`}>
          {[2.2, 1, 0.9, 1.3, 1, 1].map((_, i) => (
            <Bar key={i} width="w-12" height="h-2.5" delay={delay} />
          ))}
        </div>
        {[...Array(rows)].map((_, i) => (
          <SkeletonRow key={i} delay={delay + i * 40} />
        ))}
      </div>
    </div>
  );
}

/** Shown while the initial topic list loads — mirrors the real sidebar + list
 * shell so there's no layout jump once data arrives, instead of a bare
 * "Loading…" line. */
export default function TopicListSkeleton() {
  return (
    <div className="flex h-full min-h-0">
      <div className="flex w-[280px] min-w-[280px] flex-col gap-3.5 border-r border-border bg-panel p-3.5">
        <Bar width="w-full" height="h-9" />
        <div className="flex gap-1.5">
          <Bar width="w-10" height="h-6" />
          <Bar width="w-14" height="h-6" />
        </div>
        <div className="mt-2.5">
          <Bar width="w-24" />
        </div>
        <div className="flex flex-col gap-2">
          {[...Array(4)].map((_, i) => (
            <div key={i} className="flex items-center justify-between">
              <Bar width="w-28" delay={i * 60} />
              <Bar width="w-5" />
            </div>
          ))}
        </div>
      </div>

      <div className="min-h-0 flex-1 overflow-y-auto px-7 py-5">
        <div className="mb-4 flex items-center justify-between">
          <Bar width="w-20" />
          <div className="flex items-center gap-3.5">
            <Bar width="w-14" height="h-6" />
            <Bar width="w-16" height="h-4" />
            <Bar width="w-10" height="h-4" />
          </div>
        </div>

        {[2, 1, 3].map((rows, i) => (
          <SkeletonGroup key={i} rows={rows} delay={i * 90} />
        ))}
      </div>
    </div>
  );
}
