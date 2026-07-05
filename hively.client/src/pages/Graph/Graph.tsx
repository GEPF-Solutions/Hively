import { useCallback, useState } from 'react';
import { useGraphLayout } from './hooks/useGraphLayout';
import { graphNodeStyle, type GraphEntityType } from '../../utils/graphNodeStyle';
import { useTopics } from '../../hooks/data/useTopics';
import { useProducers } from '../../hooks/data/useProducers';
import { useConsumers } from '../../hooks/data/useConsumers';

interface FocalRef {
  type: GraphEntityType;
  id: string;
}

export default function Graph() {
  const { topics } = useTopics();
  const { producers } = useProducers();
  const { consumers } = useConsumers();

  const [search, setSearch] = useState('');
  const [focal, setFocal] = useState<FocalRef | null>(null);
  const [history, setHistory] = useState<FocalRef[]>([]);
  const [zoom, setZoom] = useState(1);

  const setGraphFocal = useCallback(
    (type: GraphEntityType, id: string) => {
      setFocal((prev) => {
        if (prev && (prev.type !== type || prev.id !== id)) {
          setHistory((h) => [...h, prev]);
        }
        return { type, id };
      });
      setSearch('');
    },
    [],
  );

  const graph = useGraphLayout(focal?.type ?? null, focal?.id ?? null, topics, producers, consumers, setGraphFocal);

  function handleBack() {
    setHistory((h) => {
      const next = [...h];
      const prev = next.pop();
      setFocal(prev ?? null);
      return next;
    });
  }

  function handleClear() {
    setFocal(null);
    setHistory([]);
  }

  const options = [
    ...producers.map((p) => ({ type: 'producer' as const, id: p.id, label: p.name })),
    ...consumers.map((c) => ({ type: 'consumer' as const, id: c.id, label: c.name })),
    ...topics.map((t) => ({ type: 'topic' as const, id: t.id, label: t.path })),
  ];
  const showResults = search.trim().length > 0;
  const filteredOptions = showResults ? options.filter((o) => o.label.toLowerCase().includes(search.toLowerCase())) : [];

  return (
    <div className="flex h-full flex-col gap-3.5 overflow-y-auto p-6">
      <div className="flex flex-wrap items-center gap-2.5">
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="search a producer, consumer, or topic…"
          className="min-w-[280px] flex-1 facet-sm facet-border-strong bg-bg px-2.5 py-2 font-mono text-[12.5px] text-text outline-none placeholder:text-muted focus:facet-accent"
        />
        {history.length > 0 && (
          <button onClick={handleBack} className="whitespace-nowrap facet-sm facet-border-strong px-3 py-2 text-[12.5px] text-text/80">
            ← back
          </button>
        )}
        {focal && (
          <button onClick={handleClear} className="whitespace-nowrap facet-sm facet-border-strong px-3 py-2 text-[12.5px] text-text/80">
            clear
          </button>
        )}
      </div>

      {showResults && (
        <div className="hv-card flex max-h-60 flex-col gap-0.5 overflow-y-auto p-1.5">
          {filteredOptions.map((opt) => (
            <button
              key={`${opt.type}-${opt.id}`}
              onClick={() => setGraphFocal(opt.type, opt.id)}
              className="flex items-center gap-2.5 rounded-md px-2.5 py-1.5 text-left"
            >
              <span className="h-2 w-2 flex-shrink-0 rounded-full" style={{ background: graphNodeStyle(opt.type).stroke }} />
              <span className="overflow-hidden text-ellipsis whitespace-nowrap font-mono text-[12.5px] text-text">{opt.label}</span>
              <span className="ml-auto flex-shrink-0 text-[10px] uppercase tracking-wide text-muted">{opt.type}</span>
            </button>
          ))}
          {filteredOptions.length === 0 && <div className="px-2.5 py-1.5 text-[11.5px] italic text-muted">no matches</div>}
        </div>
      )}

      {graph && focal ? (
        <>
          <div className="flex items-center gap-2 font-mono text-xs text-muted">
            <span className="h-2 w-2 rounded-full" style={{ background: graph.focalDotColor }} />
            centered on <span className="font-semibold text-text">{graph.focalLabel}</span> ({focal.type})
          </div>

          <div className="hv-card relative flex min-h-0 flex-1 items-center justify-center overflow-auto p-2.5">
            <div className="facet-md facet-border absolute right-3 top-3 z-10 flex items-center gap-1 bg-bg p-1">
              <button onClick={() => setZoom((z) => Math.max(0.5, +(z - 0.25).toFixed(2)))} className="h-[26px] w-[26px] facet-sm text-text/80">
                −
              </button>
              <button onClick={() => setZoom(1)} className="min-w-[42px] font-mono text-[11px] text-muted">
                {Math.round(zoom * 100)}%
              </button>
              <button onClick={() => setZoom((z) => Math.min(3, +(z + 0.25).toFixed(2)))} className="h-[26px] w-[26px] facet-sm text-text/80">
                +
              </button>
            </div>

            <div
              className="relative h-full w-auto max-w-full flex-shrink-0"
              style={{ aspectRatio: '900 / 560', transform: `scale(${zoom})`, transformOrigin: 'center center' }}
            >
              <svg viewBox="0 0 900 560" className="absolute inset-0 block h-full w-full">
                {graph.edges.map((edge, i) => (
                  <line
                    key={i}
                    x1={edge.x1}
                    y1={edge.y1}
                    x2={edge.x2}
                    y2={edge.y2}
                    stroke={edge.stroke}
                    strokeWidth={1.4}
                    strokeOpacity={edge.opacity}
                  />
                ))}
                {/* Traveling dash overlay showing producer -> topic -> consumer flow direction. */}
                {graph.edges.map((edge, i) => (
                  <line
                    key={`flow-${i}`}
                    className="graph-flow-edge"
                    x1={edge.x1}
                    y1={edge.y1}
                    x2={edge.x2}
                    y2={edge.y2}
                    stroke={edge.stroke}
                    strokeWidth={2}
                    style={{ animationDirection: edge.reversed ? 'reverse' : 'normal' }}
                  />
                ))}
                {graph.nodes.map((node) => (
                  <circle
                    key={node.key}
                    onClick={node.onClick ?? undefined}
                    style={{ cursor: node.cursor }}
                    cx={node.x}
                    cy={node.y}
                    r={node.r}
                    fill={node.fill}
                    stroke={node.stroke}
                    strokeWidth={node.strokeWidth}
                  />
                ))}
              </svg>
              {graph.nodes.map((node) => (
                <div
                  key={node.key}
                  title={node.fullLabel}
                  onClick={node.onClick ?? undefined}
                  className="absolute whitespace-nowrap font-mono"
                  style={{
                    left: node.xPct,
                    top: node.labelYPct,
                    transform: node.labelTransform,
                    fontSize: node.fontSize,
                    color: node.textColor,
                    cursor: node.cursor,
                    pointerEvents: 'auto',
                  }}
                >
                  {node.shortLabel}
                </div>
              ))}
            </div>
          </div>

          <div className="flex gap-4.5 text-[11.5px] text-muted">
            <div className="flex items-center gap-1.5">
              <span className="h-2 w-2 rounded-full" style={{ background: graphNodeStyle('producer').stroke }} />
              producer
            </div>
            <div className="flex items-center gap-1.5">
              <span className="h-2 w-2 rounded-full" style={{ background: graphNodeStyle('topic').stroke }} />
              topic
            </div>
            <div className="flex items-center gap-1.5">
              <span className="h-2 w-2 rounded-full" style={{ background: graphNodeStyle('consumer').stroke }} />
              consumer
            </div>
            <div className="ml-auto text-muted/70">click any node to re-center the graph on it</div>
          </div>
        </>
      ) : (
        <div className="flex flex-1 items-center justify-center text-center text-[13.5px] text-muted">
          Search for a producer, consumer, or topic above to see its connections.
        </div>
      )}
    </div>
  );
}
