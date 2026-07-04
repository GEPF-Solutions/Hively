import { useState } from 'react';
import ManageTagsPanel from './panels/ManageTagsPanel';
import ManageSchemasPanel from './panels/ManageSchemasPanel';
import ManageRulesPanel from './panels/ManageRulesPanel';
import ManageProducersPanel from './panels/ManageProducersPanel';
import ManageConsumersPanel from './panels/ManageConsumersPanel';

type PanelName = 'tags' | 'schemas' | 'rules' | 'producers' | 'consumers';

const MENU_ITEMS: { key: PanelName; label: string }[] = [
  { key: 'tags', label: 'Tags' },
  { key: 'schemas', label: 'Schemas' },
  { key: 'rules', label: 'Rules' },
  { key: 'producers', label: 'Producers' },
  { key: 'consumers', label: 'Consumers' },
];

export default function ManageMenu() {
  const [menuOpen, setMenuOpen] = useState(false);
  const [openPanel, setOpenPanel] = useState<PanelName | null>(null);

  return (
    <div className="relative">
      <button
        onClick={() => setMenuOpen((v) => !v)}
        className="flex items-center gap-1.5 rounded-md border border-border-strong px-3 py-1.5 text-[13px] text-text/90"
      >
        Manage <span className="text-[10px]">▾</span>
      </button>

      {menuOpen && (
        <>
          <div className="fixed inset-0 z-40" onClick={() => setMenuOpen(false)} />
          <div className="absolute right-0 top-[calc(100%+6px)] z-50 min-w-[170px] rounded-lg border border-border-strong bg-panel p-1.5 shadow-2xl">
            {MENU_ITEMS.map((item) => (
              <button
                key={item.key}
                onClick={() => {
                  setOpenPanel(item.key);
                  setMenuOpen(false);
                }}
                className="block w-full rounded-md px-2.5 py-2 text-left text-[13px] text-text/90 hover:bg-border/30"
              >
                {item.label}
              </button>
            ))}
          </div>
        </>
      )}

      {openPanel === 'tags' && <ManageTagsPanel onClose={() => setOpenPanel(null)} />}
      {openPanel === 'schemas' && <ManageSchemasPanel onClose={() => setOpenPanel(null)} />}
      {openPanel === 'rules' && <ManageRulesPanel onClose={() => setOpenPanel(null)} />}
      {openPanel === 'producers' && <ManageProducersPanel onClose={() => setOpenPanel(null)} />}
      {openPanel === 'consumers' && <ManageConsumersPanel onClose={() => setOpenPanel(null)} />}
    </div>
  );
}
