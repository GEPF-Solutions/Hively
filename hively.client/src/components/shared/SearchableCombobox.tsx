import { useState } from 'react';

export interface ComboboxOption {
  id: string;
  label: string;
}

interface SearchableComboboxProps {
  options: ComboboxOption[];
  selectedId: string | null;
  onSelect: (id: string | null) => void;
  placeholder?: string;
  /** Pinned option shown above the filtered list regardless of the search text, e.g. "Unknown" / "No schema". */
  noneLabel?: string;
  maxHeight?: number;
}

/**
 * Type-to-search, single-select list with a radio-style dot indicator — the
 * producer/schema picker pattern used across ConfigureTopicModal,
 * SchemaAssignModal and ProducerAssignModal (see Design/README.md's note on
 * extracting this once).
 */
export default function SearchableCombobox({
  options,
  selectedId,
  onSelect,
  placeholder = 'type to search…',
  noneLabel,
  maxHeight = 160,
}: SearchableComboboxProps) {
  const [search, setSearch] = useState('');

  const filtered = options.filter((o) => o.label.toLowerCase().includes(search.trim().toLowerCase()));

  return (
    <div>
      <input
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        placeholder={placeholder}
        className="w-full rounded-t-md border border-border-strong bg-bg px-2.5 py-2 font-mono text-xs text-text outline-none placeholder:text-muted focus:border-cyan"
      />
      <div className="flex flex-col gap-1 overflow-y-auto rounded-b-md border border-t-0 border-border-strong p-1.5" style={{ maxHeight }}>
        {noneLabel && (
          <button
            onClick={() => onSelect(null)}
            className="flex items-center gap-2 rounded-md px-2.5 py-1.5 text-left"
            style={{ background: selectedId === null ? 'oklch(0.27 0.06 200)' : 'transparent' }}
          >
            <span
              className="h-3 w-3 flex-shrink-0 rounded-full border-[1.5px] border-muted"
              style={{ background: selectedId === null ? 'oklch(0.75 0.13 200)' : 'transparent' }}
            />
            <span className="text-[12.5px] text-text/90">{noneLabel}</span>
          </button>
        )}
        {filtered.map((opt) => (
          <button
            key={opt.id}
            onClick={() => onSelect(opt.id)}
            className="flex items-center gap-2 rounded-md px-2.5 py-1.5 text-left"
            style={{ background: selectedId === opt.id ? 'oklch(0.27 0.06 200)' : 'transparent' }}
          >
            <span
              className="h-3 w-3 flex-shrink-0 rounded-full border-[1.5px] border-muted"
              style={{ background: selectedId === opt.id ? 'oklch(0.75 0.13 200)' : 'transparent' }}
            />
            <span className="text-[12.5px] text-text/90">{opt.label}</span>
          </button>
        ))}
        {filtered.length === 0 && !noneLabel && (
          <div className="px-2.5 py-1.5 text-[11.5px] italic text-muted">no matches</div>
        )}
      </div>
    </div>
  );
}
