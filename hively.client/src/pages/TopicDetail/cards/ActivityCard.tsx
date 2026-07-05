import { useState } from 'react';

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

function Stat({ label, value, sub }: { label: string; value: number; sub?: string }) {
  return (
    <div>
      <div className="text-[10px] uppercase tracking-wide text-muted">{label}</div>
      <div className="text-lg font-semibold text-text">
        {value}
        {sub && <span className="ml-1 text-[11px] font-normal text-muted">{sub}</span>}
      </div>
    </div>
  );
}

export default function ActivityCard({ activityHistogram }: { activityHistogram: string | null }) {
  const activity = parseHistogram(activityHistogram);
  const max = Math.max(1, ...activity);
  const total = activity.reduce((a, b) => a + b, 0);
  const peakIndex = activity.reduce((best, count, i) => (count > activity[best] ? i : best), 0);
  const activeHours = activity.filter((count) => count > 0).length;
  const currentHour = activity[activity.length - 1];
  const [hovered, setHovered] = useState<number | null>(null);

  return (
    <div className="hv-card flex h-full flex-col p-4">
      <div className="mb-3 text-[11px] font-semibold uppercase tracking-wide text-muted">
        Activity · msgs/hr, last 24h
      </div>

      <div className="mb-4 grid grid-cols-4 gap-3">
        <Stat label="Total" value={total} />
        <Stat label="Peak" value={activity[peakIndex]} sub={hourLabel(activity.length - 1 - peakIndex)} />
        <Stat label="This hour" value={currentHour} />
        <Stat label="Active hrs" value={activeHours} sub={`/ ${activity.length}`} />
      </div>

      <div className="flex flex-1 flex-col justify-end">
        <div className="flex h-[64px] items-end gap-[2px] border-b border-border pb-px">
          {activity.map((count, i) => {
            const hoursAgo = activity.length - 1 - i;
            const isCurrent = hoursAgo === 0;
            const isHovered = hovered === i;
            const barHeight = Math.max(2, (count / max) * 100);
            return (
              <button
                key={i}
                type="button"
                onMouseEnter={() => setHovered(i)}
                onMouseLeave={() => setHovered(null)}
                onFocus={() => setHovered(i)}
                onBlur={() => setHovered(null)}
                aria-label={`${count} messages · ${hourLabel(hoursAgo)}`}
                className="relative h-full max-w-[14px] flex-1 border-none bg-transparent p-0 outline-none"
              >
                {/* De-emphasized for history, the brand accent for the current hour — the
                    one bar the eye should land on, not a fade across all 24. */}
                <span
                  className="absolute bottom-0 left-0 block w-full rounded-t-[3px]"
                  style={{
                    height: `${barHeight}%`,
                    background: isCurrent ? 'var(--color-brand)' : 'var(--color-cyan)',
                    opacity: isCurrent ? 1 : 0.4,
                    filter: isHovered && !isCurrent ? 'brightness(1.6)' : undefined,
                  }}
                />
                {isHovered && (
                  <div className="pointer-events-none absolute bottom-[calc(100%+6px)] left-1/2 z-10 -translate-x-1/2 whitespace-nowrap rounded-md border border-border-strong bg-header px-2 py-1 text-[11px] shadow-lg">
                    <span className="font-semibold text-text">{count}</span>{' '}
                    <span className="text-muted">msgs · {hourLabel(hoursAgo)}{isCurrent ? ' (now)' : ''}</span>
                  </div>
                )}
              </button>
            );
          })}
        </div>
      </div>
    </div>
  );
}
