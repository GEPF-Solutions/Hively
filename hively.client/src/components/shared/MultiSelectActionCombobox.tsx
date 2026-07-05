import { useState } from 'react';
import type { ComboboxOption } from './SearchableCombobox';

export type ItemAction = 'set' | 'exclude';

interface MultiSelectActionComboboxProps {
  options: ComboboxOption[];
  /** Only ids with an entry are set/excluded — an id absent from the map is untouched (inherited). */
  actions: Record<string, ItemAction>;
  /** Cycles one id: absent -> 'set' -> 'exclude' -> absent. */
  onCycle: (id: string) => void;
  placeholder?: string;
  maxHeight?: number;
  onCreate?: (label: string) => Promise<ComboboxOption>;
}

const STATE_STYLE: Record<ItemAction, { mark: string; className: string }> = {
  set: { mark: '✓', className: 'border-brand bg-brand text-brand-ink' },
  exclude: { mark: '⊘', className: 'border-red bg-transparent text-red' },
};

/**
 * Like MultiSelectCombobox, but each row cycles through three states instead
 * of a plain checkbox — unset (inherit) -> set -> exclude -> unset — backed
 * by one action map instead of two separate selected-id arrays, so an id can
 * never be "set" and "excluded" at the same time by construction.
 */
export default function MultiSelectActionCombobox({
  options,
  actions,
  onCycle,
  placeholder = 'type to search…',
  maxHeight = 160,
  onCreate,
}: MultiSelectActionComboboxProps) {
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
      onCycle(created.id);
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
        {filtered.map((opt) => {
          const action = actions[opt.id];
          const style = action ? STATE_STYLE[action] : null;
          return (
            <button
              key={opt.id}
              onClick={() => onCycle(opt.id)}
              title={action === 'exclude' ? 'Excluded — click to clear' : action === 'set' ? 'Set — click to exclude instead' : 'Inherited — click to set'}
              className="flex items-center gap-2 rounded-md px-2.5 py-1.5 text-left"
              style={{ background: action === 'set' ? 'var(--color-brand-wash)' : action === 'exclude' ? 'oklch(0.30 0.08 25)' : 'transparent' }}
            >
              <span
                className={`flex h-3 w-3 flex-shrink-0 items-center justify-center rounded-[3px] border-[1.5px] text-[9px] leading-none ${
                  style ? style.className : 'border-muted'
                }`}
              >
                {style?.mark}
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
