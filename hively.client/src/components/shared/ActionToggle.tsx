export type MatchFieldAction = 'inherit' | 'set' | 'exclude';

interface ActionToggleProps {
  value: MatchFieldAction;
  onChange: (value: MatchFieldAction) => void;
}

const OPTIONS: { value: MatchFieldAction; label: string }[] = [
  { value: 'inherit', label: 'Inherit' },
  { value: 'set', label: 'Set' },
  { value: 'exclude', label: 'Exclude' },
];

const ACTIVE_CLASSES: Record<MatchFieldAction, string> = {
  inherit: 'facet-border-strong text-text/90',
  set: 'facet-accent text-brand',
  exclude: 'facet-border-red text-red',
};

/**
 * Three-way toggle gating a single-value match field (producer/schema):
 * Inherit (untouched — the nearest less-specific match decides), Set (pick a
 * value below), or Exclude (explicitly clear it, overriding any less-specific
 * match that sets one). Kept separate from Button's two variants since this
 * is a form control with three mutually-exclusive states, not an action.
 */
export default function ActionToggle({ value, onChange }: ActionToggleProps) {
  return (
    <div className="inline-flex gap-1">
      {OPTIONS.map((opt) => {
        const active = value === opt.value;
        return (
          <button
            key={opt.value}
            type="button"
            onClick={() => onChange(opt.value)}
            className={`facet-sm px-2.5 py-1 text-[11px] font-semibold ${
              active ? ACTIVE_CLASSES[opt.value] : 'facet-border-strong text-muted hover:text-text/80'
            }`}
          >
            {opt.label}
          </button>
        );
      })}
    </div>
  );
}
