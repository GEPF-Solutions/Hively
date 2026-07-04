function parseHistogram(raw: string | null): number[] {
  if (!raw) return new Array(24).fill(0);
  try {
    const parsed = JSON.parse(raw);
    if (Array.isArray(parsed) && parsed.length === 24) return parsed;
  } catch {
    // fall through to the zeroed default below
  }
  return new Array(24).fill(0);
}

function hourLabel(hoursAgo: number): string {
  const d = new Date();
  d.setMinutes(0, 0, 0);
  d.setHours(d.getHours() - hoursAgo);
  const h = d.getHours();
  return (h % 12 === 0 ? 12 : h % 12) + (h < 12 ? 'am' : 'pm');
}

export default function ActivityCard({ activityHistogram }: { activityHistogram: string | null }) {
  const activity = parseHistogram(activityHistogram);
  const max = Math.max(1, ...activity);

  return (
    <div className="rounded-[10px] border border-border bg-panel p-4">
      <div className="mb-2.5 text-[11px] font-semibold uppercase tracking-wide text-muted">
        Activity · msgs/hr, last 24h
      </div>
      <div className="flex h-[52px] items-end gap-[2.5px]">
        {activity.map((count, i) => {
          const hoursAgo = activity.length - 1 - i;
          return (
            <div
              key={i}
              title={`${count} msgs · ${hourLabel(hoursAgo)}`}
              className="flex-1 rounded-sm bg-cyan opacity-75"
              style={{ height: `${Math.max(2, (count / max) * 100)}%` }}
            />
          );
        })}
      </div>
    </div>
  );
}
