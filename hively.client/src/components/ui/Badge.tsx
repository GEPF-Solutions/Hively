import type { ReactNode } from 'react';

type BadgeTone = 'cyan' | 'amber' | 'red' | 'plain' | 'neutral';

interface BadgeProps {
  tone?: BadgeTone;
  children: ReactNode;
  className?: string;
}

/**
 * Outlined, unfilled — transparent background, neutral border, colored text
 * only. A solid tinted fill reads as a clickable button; this reads as a
 * label. `plain` is for badges that need emphasis without being a colored
 * status (e.g. an Admin-role tag), as opposed to `neutral`'s de-emphasis.
 */
const toneClasses: Record<BadgeTone, string> = {
  cyan: 'text-cyan border-border-strong',
  amber: 'text-amber border-border-strong',
  red: 'text-red border-border-strong',
  plain: 'text-text border-border-strong',
  neutral: 'text-muted border-border-strong',
};

export default function Badge({ tone = 'neutral', children, className = '' }: BadgeProps) {
  return (
    <span
      className={`inline-flex items-center rounded-md border px-2.5 py-0.5 text-xs font-semibold uppercase tracking-wide ${toneClasses[tone]} ${className}`}
    >
      {children}
    </span>
  );
}
