import { useState } from 'react';

export interface PillOption {
  id: string;
  label: string;
}

interface MultiSelectPillsProps {
  options: PillOption[];
  selectedIds: string[];
  onToggle: (id: string) => void;
  placeholder?: string;
}

/** Type-to-search, multi-select pill toggle list — the Consumers picker pattern. */
export default function MultiSelectPills({ options, selectedIds, onToggle, placeholder = 'type to search…' }: MultiSelectPillsProps) {
  const [search, setSearch] = useState('');
  const filtered = options.filter((o) => o.label.toLowerCase().includes(search.trim().toLowerCase()));

  return (
    <div>
      <input
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        placeholder={placeholder}
        className="mb-2 w-full rounded-md border border-border-strong bg-bg px-2.5 py-2 text-xs text-text outline-none placeholder:text-muted focus:border-cyan"
      />
      <div className="flex flex-wrap gap-1.5">
        {filtered.map((opt) => {
          const active = selectedIds.includes(opt.id);
          return (
            <button
              key={opt.id}
              onClick={() => onToggle(opt.id)}
              className="rounded-full border px-2.5 py-1 text-[11px] font-medium"
              style={
                active
                  ? { background: 'oklch(0.27 0.03 220)', color: 'oklch(0.8 0.02 220)', borderColor: 'oklch(0.4 0.04 220)' }
                  : { background: 'transparent', color: 'var(--color-muted)', borderColor: 'var(--color-border-strong)', opacity: 0.6 }
              }
            >
              {opt.label}
            </button>
          );
        })}
        {filtered.length === 0 && <div className="text-[11.5px] italic text-muted">no matches</div>}
      </div>
    </div>
  );
}
