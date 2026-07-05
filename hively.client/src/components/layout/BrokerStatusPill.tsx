import { useState } from 'react';
import { useMqttStatus } from '../../hooks/data/useMqttStatus';
import { relativeTimeFromDate } from '../../utils/relativeTime';

export default function BrokerStatusPill() {
  const status = useMqttStatus();
  const [open, setOpen] = useState(false);

  const connected = status?.connected ?? false;
  const dotColor = connected ? 'oklch(0.72 0.16 150)' : 'oklch(0.7 0.18 25)';

  return (
    <div className="relative">
      <button
        onClick={() => setOpen((v) => !v)}
        className="flex items-center gap-1.5 rounded-full border border-border-strong px-2.5 py-1"
      >
        <span className="h-1.5 w-1.5 rounded-full" style={{ background: status ? dotColor : 'var(--color-muted)' }} />
        <span className="font-mono text-[11.5px] text-muted">
          {status ? (connected ? 'connected' : 'disconnected') : 'checking…'}
        </span>
      </button>

      {open && (
        <>
          <div className="fixed inset-0 z-40" onClick={() => setOpen(false)} />
          <div className="absolute right-0 top-[calc(100%+6px)] z-50 min-w-[220px] rounded-lg border border-border-strong bg-panel p-3 text-xs shadow-2xl">
            <div className="mb-2 flex items-center gap-1.5">
              <span className="h-2 w-2 rounded-full" style={{ background: status ? dotColor : 'var(--color-muted)' }} />
              <span className="font-semibold text-text/90">MQTT broker {connected ? 'connected' : 'disconnected'}</span>
            </div>
            {status ? (
              <div className="flex flex-col gap-1 text-muted">
                <div className="font-mono">
                  {status.host}:{status.port}
                </div>
                {connected && status.connectedAt && <div>connected {relativeTimeFromDate(status.connectedAt)}</div>}
                {!connected && status.lastDisconnectReason && <div>last error: {status.lastDisconnectReason}</div>}
              </div>
            ) : (
              <div className="text-muted">Loading broker status…</div>
            )}
          </div>
        </>
      )}
    </div>
  );
}
