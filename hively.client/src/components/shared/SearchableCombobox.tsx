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
  /**
   * When provided, shows a "+ Create '<search>'" option whenever the search
   * text has no exact-label match — lets an admin add a producer/consumer
   * on the spot instead of backing out to the Manage panel. Must resolve to
   * the newly created option; the combobox selects it and clears the search.
   */
  onCreate?: (label: string) => Promise<ComboboxOption>;
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
  onCreate,
}: SearchableComboboxProps) {
  const [search, setSearch] = useState('');
  const [creating, setCreating] = useState(false);

  const trimmed = search.trim();
  const filtered = options.filter((o) => o.label.toLowerCase().includes(trimmed.toLowerCase()));
  const hasExactMatch = options.some((o) => o.label.toLowerCase() === trimmed.toLowerCase());
  const showCreate = Boolean(onCreate) && trimmed.length > 0 && !hasExactMatch;

  async function handleCreate() {
    if (!onCreate || creating) return;
    setCreating(true);
    try {
      const created = await onCreate(trimmed);
      onSelect(created.id);
      setSearch('');
    } finally {
      setCreating(false);
    }
  }

  return (
    <div>
      <input
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        placeholder={placeholder}
        className="w-full rounded-t-md border border-border-strong bg-bg px-2.5 py-2 font-mono text-xs text-text outline-none placeholder:text-muted focus:border-brand"
      />
      <div className="flex flex-col gap-1 overflow-y-auto rounded-b-md border border-t-0 border-border-strong p-1.5" style={{ maxHeight }}>
        {showCreate && (
          <button
            onClick={handleCreate}
            disabled={creating}
            className="flex items-center gap-2 rounded-md border border-dashed border-brand/50 px-2.5 py-1.5 text-left text-brand disabled:opacity-60"
          >
            <span className="text-[12.5px]">{creating ? 'Creating…' : `+ Create "${trimmed}"`}</span>
          </button>
        )}
        {noneLabel && (
          <button
            onClick={() => onSelect(null)}
            className="flex items-center gap-2 rounded-md px-2.5 py-1.5 text-left"
            style={{ background: selectedId === null ? 'var(--color-brand-wash)' : 'transparent' }}
          >
            <span
              className="h-3 w-3 flex-shrink-0 rounded-full border-[1.5px] border-muted"
              style={{ background: selectedId === null ? 'var(--color-brand)' : 'transparent' }}
            />
            <span className="text-[12.5px] text-text/90">{noneLabel}</span>
          </button>
        )}
        {filtered.map((opt) => (
          <button
            key={opt.id}
            onClick={() => onSelect(opt.id)}
            className="flex items-center gap-2 rounded-md px-2.5 py-1.5 text-left"
            style={{ background: selectedId === opt.id ? 'var(--color-brand-wash)' : 'transparent' }}
          >
            <span
              className="h-3 w-3 flex-shrink-0 rounded-full border-[1.5px] border-muted"
              style={{ background: selectedId === opt.id ? 'var(--color-brand)' : 'transparent' }}
            />
            <span className="text-[12.5px] text-text/90">{opt.label}</span>
          </button>
        ))}
        {filtered.length === 0 && !noneLabel && !showCreate && (
          <div className="px-2.5 py-1.5 text-[11.5px] italic text-muted">no matches</div>
        )}
      </div>
    </div>
  );
}
