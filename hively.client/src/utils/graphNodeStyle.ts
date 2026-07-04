export type GraphEntityType = 'producer' | 'consumer' | 'topic';

interface GraphNodeStyle {
  fill: string;
  stroke: string;
  textColor: string;
}

// Deliberately spread across distinct hues per Design/README.md's Graph tab
// note — an earlier iteration used two similar blues and was hard to read.
export function graphNodeStyle(type: GraphEntityType): GraphNodeStyle {
  if (type === 'producer') return { fill: 'oklch(0.28 0.09 200)', stroke: 'oklch(0.78 0.15 200)', textColor: 'oklch(0.88 0.04 200)' };
  if (type === 'consumer') return { fill: 'oklch(0.28 0.1 330)', stroke: 'oklch(0.75 0.17 330)', textColor: 'oklch(0.88 0.05 330)' };
  return { fill: 'oklch(0.32 0.09 80)', stroke: 'oklch(0.78 0.14 80)', textColor: 'oklch(0.9 0.03 80)' };
}
