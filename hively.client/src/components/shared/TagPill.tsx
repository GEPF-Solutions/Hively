import { tagColor } from '../../utils/tagColor';
import type { Tag } from '../../types';

interface TagPillProps {
  tag: Tag;
  onRemove?: () => void;
  onClick?: () => void;
  active?: boolean;
}

export default function TagPill({ tag, onRemove, onClick, active = true }: TagPillProps) {
  const { color, bg, borderColor } = tagColor(tag.hue);

  return (
    <span
      onClick={onClick}
      className={`inline-flex items-center gap-1.5 rounded-full border px-2.5 py-0.5 text-xs font-medium ${onClick ? 'cursor-pointer' : ''}`}
      style={active ? { color, background: bg, borderColor } : { color: 'var(--color-muted)', borderColor: 'var(--color-border-strong)' }}
    >
      {tag.label}
      {onRemove && (
        <button
          onClick={(e) => {
            e.stopPropagation();
            onRemove();
          }}
          aria-label={`Remove ${tag.label}`}
          className="leading-none text-current opacity-70 hover:opacity-100 bg-transparent border-none cursor-pointer"
        >
          ×
        </button>
      )}
    </span>
  );
}
