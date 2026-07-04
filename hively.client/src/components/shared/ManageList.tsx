import type { ReactNode } from 'react';

/**
 * Shared scrollable list wrapper for the Manage panels (Tags/Schemas/Rules/
 * Producers/Consumers/Users) — one place to fix "same height, same gap,
 * scrollbar doesn't overlap row actions" instead of six near-identical divs
 * drifting apart over time. The right padding is what keeps the scrollbar
 * clear of each row's trailing Edit/Delete button.
 */
export default function ManageList({ children }: { children: ReactNode }) {
  return <div className="mb-5 flex max-h-60 flex-col gap-1.5 overflow-y-auto pr-1.5">{children}</div>;
}
