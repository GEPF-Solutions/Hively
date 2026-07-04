// ISA-95-flavored level names for the prototype's reference 12-level hierarchy
// (see Design/README.md, Topic.path). Real deployments may be shallower or
// deeper — always fall back to a generic "level" label past this list rather
// than assuming 12.
export const LEVEL_NAMES = [
  'enterprise',
  'region',
  'country',
  'site',
  'area',
  'line',
  'cell',
  'equipment',
  'module',
  'subsystem',
  'sensor',
  'measurement',
];

export function levelName(depth: number): string {
  return LEVEL_NAMES[depth] ?? 'level';
}

export function segmentsOf(path: string): string[] {
  return path.split('/');
}
