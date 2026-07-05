import { tagColor } from '../../../utils/tagColor';
import { UNTRACKED_FILTER } from '../hooks/useTopicFilters';
import type { Tag } from '../../../types';

interface NamespaceChild {
  name: string;
  count: number;
  hasUntracked: boolean;
}

interface NamespaceSidebarProps {
  search: string;
  onSearchChange: (value: string) => void;
  tags: Tag[];
  tagFilter: string | null;
  onTagFilterChange: (tagId: string | null) => void;
  untrackedCount: number;
  folderPath: string[];
  onDrillTo: (index: number) => void;
  hasFolderFilter: boolean;
  onResetFolder: () => void;
  currentLevelName: string;
  namespaceChildren: NamespaceChild[];
  onDrillInto: (name: string) => void;
  isAdmin: boolean;
  /** Opens Manage Matches pre-seeded with `[...folderPath, '#'].join('/')`. */
  onConfigureBranch: () => void;
}

export default function NamespaceSidebar({
  search,
  onSearchChange,
  tags,
  tagFilter,
  onTagFilterChange,
  untrackedCount,
  folderPath,
  onDrillTo,
  hasFolderFilter,
  onResetFolder,
  currentLevelName,
  namespaceChildren,
  onDrillInto,
  isAdmin,
  onConfigureBranch,
}: NamespaceSidebarProps) {
  return (
    <div className="flex w-[280px] min-w-[280px] flex-col overflow-hidden border-r border-border bg-panel">
      <div className="flex flex-col gap-2.5 border-b border-border/70 p-3.5 pb-2.5">
        <input
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
          placeholder="search topics, producers, tags…"
          className="w-full facet-sm facet-border-strong bg-bg px-2.5 py-2 font-mono text-[12.5px] text-text outline-none placeholder:text-muted focus:facet-accent"
        />
        <div className="flex flex-wrap items-center gap-1.5">
          <button
            onClick={() => onTagFilterChange(null)}
            className={`whitespace-nowrap rounded-md border px-2.5 py-1 text-[11px] font-medium ${
              !tagFilter ? 'border-brand/50 text-brand' : 'border-border-strong text-muted'
            }`}
          >
            all
          </button>
          {tags.map((tag) => {
            const active = tagFilter === tag.id;
            const c = tagColor(tag.hue);
            return (
              <button
                key={tag.id}
                onClick={() => onTagFilterChange(active ? null : tag.id)}
                className="whitespace-nowrap rounded-md border border-border-strong px-2.5 py-1 text-[11px] font-medium"
                style={{ color: active ? c.color : 'var(--color-muted)' }}
              >
                {tag.label}
              </button>
            );
          })}
        </div>
      </div>

      {untrackedCount > 0 && (
        <button
          onClick={() => onTagFilterChange(UNTRACKED_FILTER)}
          className="facet-md facet-border-amber mx-3.5 mt-3 bg-amber/20 p-2.5 text-left"
        >
          <div className="text-[12.5px] font-semibold text-amber">⚠ {untrackedCount} untracked topics</div>
          <div className="mt-0.5 text-[11px] text-amber/80">seen on broker, not yet catalogued</div>
        </button>
      )}

      <div className="flex items-center justify-between px-3.5 pb-1.5 pt-3.5">
        <div className="text-[10.5px] font-semibold uppercase tracking-wide text-muted">Namespace</div>
        {hasFolderFilter && (
          <button onClick={onResetFolder} className="text-[11px] text-brand">
            reset
          </button>
        )}
      </div>

      <div className="flex flex-wrap items-center gap-0.5 px-3.5 pb-2 font-mono text-[11.5px]">
        {folderPath.map((seg, i) => (
          <span key={i} className="flex items-center gap-0.5">
            <button onClick={() => onDrillTo(i)} className="px-0.5 py-0.5 font-medium text-brand">
              {seg}
            </button>
            <span className="text-muted">/</span>
          </span>
        ))}
      </div>

      {isAdmin && folderPath.length > 0 && (
        <div className="px-3.5 pb-2.5">
          <button
            onClick={onConfigureBranch}
            className="w-full facet-sm facet-border-strong px-2.5 py-1.5 text-[11.5px] font-medium text-text/90 hover:facet-accent hover:text-brand"
          >
            Configure {folderPath.join('/')}/…
          </button>
        </div>
      )}

      <div className="flex-1 overflow-y-auto px-2 pb-3.5">
        <div className="px-2.5 pb-1.5 pt-1 text-[10px] uppercase tracking-wide text-muted/80">{currentLevelName}</div>
        {namespaceChildren.map((child) => (
          <button
            key={child.name}
            onClick={() => onDrillInto(child.name)}
            className="mb-0.5 flex w-full items-center justify-between rounded-md px-2.5 py-1.5 text-left hover:bg-border/30"
          >
            <span className="overflow-hidden text-ellipsis whitespace-nowrap font-mono text-[12.5px] text-text/90">
              {child.name}
            </span>
            <span className="flex flex-shrink-0 items-center gap-1.5">
              {child.hasUntracked && <span className="h-1.5 w-1.5 rounded-full bg-amber" />}
              <span className="text-[10.5px] text-muted">{child.count}</span>
            </span>
          </button>
        ))}
      </div>
    </div>
  );
}
