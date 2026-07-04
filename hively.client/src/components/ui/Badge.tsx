import type { ReactNode } from 'react';

type BadgeTone = 'cyan' | 'amber' | 'red' | 'neutral';

interface BadgeProps {
  tone?: BadgeTone;
  children: ReactNode;
  className?: string;
}

const toneClasses: Record<BadgeTone, string> = {
  cyan: 'text-cyan bg-cyan/15 border-cyan/40',
  amber: 'text-amber bg-amber/15 border-amber/40',
  red: 'text-red bg-red/15 border-red/40',
  neutral: 'text-muted bg-border/30 border-border-strong',
};

export default function Badge({ tone = 'neutral', children, className = '' }: BadgeProps) {
  return (
    <span
      className={`inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold uppercase tracking-wide ${toneClasses[tone]} ${className}`}
    >
      {children}
    </span>
  );
}
