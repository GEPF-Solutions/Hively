/** Ported from Design/MQTT Data Catalog.dc.html's tagColor(). `null` hue = neutral gray tag. */
export interface TagColorTriad {
  color: string;
  bg: string;
  borderColor: string;
}

export function tagColor(hue: number | null): TagColorTriad {
  if (hue === null) {
    return { color: 'oklch(0.7 0.01 254)', bg: 'oklch(0.28 0.01 254)', borderColor: 'oklch(0.4 0.012 254)' };
  }
  return {
    color: `oklch(0.85 0.1 ${hue})`,
    bg: `oklch(0.27 0.06 ${hue})`,
    borderColor: `oklch(0.42 0.09 ${hue})`,
  };
}
