import { useState } from 'react';

export interface ComboboxOption {
  id: string;
  label: string;
}

interface MultiSelectComboboxProps {
  options: ComboboxOption[];
  selectedIds: string[];
  onToggle: (id: string) => void;
  placeholder?: string;
  maxHeight?: number;
  /**
   * When provided, shows a "+ Create '<search>'" option whenever the search
   * text has no exact-label match — lets an admin add a consumer on the spot
   * instead of backing out to the Manage panel. Must resolve to the newly
   * created option; it's selected immediately and the search clears.
   */
  onCreate?: (label: string) => Promise<ComboboxOption>;
}

/**
 * Type-to-search, multi-select list with a checkbox indicator — the same
 * list chrome as SearchableCombobox (used for single-select producer/schema
 * pickers), but for fields that take any number of values (consumers).
 */
export default function MultiSelectCombobox({
  options,
  selectedIds,
  onToggle,
  placeholder = 'type to search…',
  maxHeight = 160,
  onCreate,
}: MultiSelectComboboxProps) {
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
      onToggle(created.id);
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
        className="w-full rounded-t-md border border-border-strong bg-bg px-2.5 py-2 font-mono text-xs text-text outline-none placeholder:text-muted focus:border-cyan"
      />
      <div className="flex flex-col gap-1 overflow-y-auto rounded-b-md border border-t-0 border-border-strong p-1.5" style={{ maxHeight }}>
        {showCreate && (
          <button
            onClick={handleCreate}
            disabled={creating}
            className="flex items-center gap-2 rounded-md border border-dashed border-cyan/50 px-2.5 py-1.5 text-left text-cyan disabled:opacity-60"
          >
            <span className="text-[12.5px]">{creating ? 'Creating…' : `+ Create "${trimmed}"`}</span>
          </button>
        )}
        {filtered.map((opt) => {
          const checked = selectedIds.includes(opt.id);
          return (
            <button
              key={opt.id}
              onClick={() => onToggle(opt.id)}
              className="flex items-center gap-2 rounded-md px-2.5 py-1.5 text-left"
              style={{ background: checked ? 'oklch(0.27 0.06 200)' : 'transparent' }}
            >
              <span
                className="flex h-3 w-3 flex-shrink-0 items-center justify-center rounded-[3px] border-[1.5px] border-muted text-[9px] leading-none"
                style={
                  checked
                    ? { background: 'oklch(0.75 0.13 200)', borderColor: 'oklch(0.75 0.13 200)', color: 'oklch(0.15 0.02 200)' }
                    : undefined
                }
              >
                {checked && '✓'}
              </span>
              <span className="text-[12.5px] text-text/90">{opt.label}</span>
            </button>
          );
        })}
        {filtered.length === 0 && !showCreate && (
          <div className="px-2.5 py-1.5 text-[11.5px] italic text-muted">no matches</div>
        )}
      </div>
    </div>
  );
}
