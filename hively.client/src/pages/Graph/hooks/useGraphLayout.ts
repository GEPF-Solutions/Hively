import { useMemo } from 'react';
import { graphNodeStyle, type GraphEntityType } from '../../../utils/graphNodeStyle';
import { segmentsOf } from '../../../utils/topicPath';
import type { Consumer, Producer, Topic } from '../../../types';

const W = 900;
const H = 560;
const CX = W / 2;
const CY = H / 2;
const R1 = 165;
const R2 = 255;

export interface GraphNode {
  key: string;
  x: number;
  y: number;
  r: number;
  fill: string;
  stroke: string;
  strokeWidth: number;
  textColor: string;
  fontSize: number;
  fullLabel: string;
  shortLabel: string;
  xPct: string;
  labelYPct: string;
  labelTransform: string;
  onClick: (() => void) | null;
  cursor: 'pointer' | 'default';
}

export interface GraphEdge {
  x1: number;
  y1: number;
  x2: number;
  y2: number;
  stroke: string;
  opacity: number;
}

export interface GraphLayout {
  nodes: GraphNode[];
  edges: GraphEdge[];
  focalLabel: string;
  focalDotColor: string;
}

function truncateLabel(s: string, n: number): string {
  return s.length > n ? `${s.slice(0, n - 1)}…` : s;
}

function entityLabel(
  type: GraphEntityType,
  id: string,
  producerById: Map<string, Producer>,
  consumerById: Map<string, Consumer>,
  topicById: Map<string, Topic>,
): string {
  if (type === 'producer') return producerById.get(id)?.name ?? id;
  if (type === 'consumer') return consumerById.get(id)?.name ?? id;
  const topic = topicById.get(id);
  return topic ? (segmentsOf(topic.path).at(-1) ?? id) : id;
}

/**
 * Port of the prototype's computeGraph() — star/ego-graph around a focal
 * producer/consumer/topic (see Design/README.md, Key Behaviors #8). Label
 * positions are computed as percentages with a vertical flip (labels below
 * the node in the top half of the canvas, above it in the bottom half) and
 * rendered as an HTML overlay rather than SVG <text> — preserved from the
 * prototype for the same reasons documented there.
 */
export function useGraphLayout(
  focalType: GraphEntityType | null,
  focalId: string | null,
  topics: Topic[],
  producers: Producer[],
  consumers: Consumer[],
  onFocus: (type: GraphEntityType, id: string) => void,
): GraphLayout | null {
  const producerById = useMemo(() => new Map(producers.map((p) => [p.id, p])), [producers]);
  const consumerById = useMemo(() => new Map(consumers.map((c) => [c.id, c])), [consumers]);
  const topicById = useMemo(() => new Map(topics.map((t) => [t.id, t])), [topics]);

  return useMemo(() => {
    if (!focalType || !focalId) return null;

    const label = (type: GraphEntityType, id: string) => entityLabel(type, id, producerById, consumerById, topicById);

    const nodes: GraphNode[] = [];
    const edges: GraphEdge[] = [];

    function mk(key: string, x: number, y: number, r: number, type: GraphEntityType, text: string, onClick: (() => void) | null, isFocal = false): GraphNode {
      const st = graphNodeStyle(type);
      const labelAbove = y > CY;
      const labelY = labelAbove ? y - r - 15 : y + r + 15;
      return {
        key,
        x,
        y,
        r,
        fill: st.fill,
        stroke: st.stroke,
        strokeWidth: isFocal ? 2.5 : 1.5,
        textColor: st.textColor,
        fontSize: isFocal ? 13 : 11.5,
        fullLabel: text,
        shortLabel: truncateLabel(text, isFocal ? 20 : 14),
        xPct: `${(x / W) * 100}%`,
        labelYPct: `${(labelY / H) * 100}%`,
        labelTransform: labelAbove ? 'translate(-50%, -100%)' : 'translate(-50%, 0)',
        onClick,
        cursor: onClick ? 'pointer' : 'default',
      };
    }

    nodes.push(mk('focal', CX, CY, 28, focalType, label(focalType, focalId), null, true));

    if (focalType === 'topic') {
      const topic = topicById.get(focalId);
      if (!topic) return null;

      if (topic.producerId) {
        const x = CX - R1;
        const y = CY;
        nodes.push(mk(`p-${topic.producerId}`, x, y, 20, 'producer', label('producer', topic.producerId), () => onFocus('producer', topic.producerId!)));
        edges.push({ x1: CX, y1: CY, x2: x, y2: y, stroke: graphNodeStyle('producer').stroke, opacity: 0.7 });
      }

      const n = topic.consumerIds.length;
      topic.consumerIds.forEach((cid, i) => {
        const spread = n === 1 ? 0 : Math.min(2.1, 0.55 * n);
        const ang = n === 1 ? 0 : -spread / 2 + (spread * i) / (n - 1);
        const x = CX + R1 * Math.cos(ang);
        const y = CY + R1 * Math.sin(ang);
        nodes.push(mk(`c-${cid}`, x, y, 18, 'consumer', label('consumer', cid), () => onFocus('consumer', cid)));
        edges.push({ x1: CX, y1: CY, x2: x, y2: y, stroke: graphNodeStyle('consumer').stroke, opacity: 0.7 });
      });
    } else {
      const relTopics = topics.filter((t) => (focalType === 'producer' ? t.producerId === focalId : t.consumerIds.includes(focalId)));
      const n = relTopics.length;
      const farMap = new Map<string, { id: string; type: GraphEntityType; parents: { x: number; y: number }[] }>();

      relTopics.forEach((t, i) => {
        const ang = n === 0 ? 0 : (2 * Math.PI * i) / n - Math.PI / 2;
        const x = CX + R1 * Math.cos(ang);
        const y = CY + R1 * Math.sin(ang);
        const leaf = segmentsOf(t.path).at(-1) ?? t.path;
        nodes.push(mk(`t-${t.id}`, x, y, 15, 'topic', leaf, () => onFocus('topic', t.id)));
        edges.push({ x1: CX, y1: CY, x2: x, y2: y, stroke: graphNodeStyle('topic').stroke, opacity: 0.6 });

        const farType: GraphEntityType = focalType === 'producer' ? 'consumer' : 'producer';
        const farIds = focalType === 'producer' ? t.consumerIds : t.producerId ? [t.producerId] : [];
        farIds.forEach((fid) => {
          const key = `${farType}-${fid}`;
          const existing = farMap.get(key) ?? { id: fid, type: farType, parents: [] };
          existing.parents.push({ x, y });
          farMap.set(key, existing);
        });
      });

      const farNodes = Array.from(farMap.values());
      const m = farNodes.length;
      farNodes.forEach((f, i) => {
        const ang = m === 0 ? 0 : (2 * Math.PI * i) / m - Math.PI / 2;
        const x = CX + R2 * Math.cos(ang);
        const y = CY + R2 * Math.sin(ang);
        nodes.push(mk(`${f.type}2-${f.id}`, x, y, 15, f.type, label(f.type, f.id), () => onFocus(f.type, f.id)));
        f.parents.forEach((p) => edges.push({ x1: p.x, y1: p.y, x2: x, y2: y, stroke: graphNodeStyle(f.type).stroke, opacity: 0.35 }));
      });
    }

    return {
      nodes,
      edges,
      focalLabel: label(focalType, focalId),
      focalDotColor: graphNodeStyle(focalType).stroke,
    };
  }, [focalType, focalId, topics, producerById, consumerById, topicById, onFocus]);
}
