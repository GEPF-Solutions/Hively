/** Ported from Design/MQTT Data Catalog.dc.html's relTime(). */
export function relativeTimeFromMinutes(minutes: number): string {
  if (minutes < 1) return 'just now';
  if (minutes < 60) return `${Math.floor(minutes)}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  return `${Math.floor(hours / 24)}d ago`;
}

export function relativeTimeFromDate(isoDate: string | null): string | null {
  if (!isoDate) return null;
  const minutes = (Date.now() - new Date(isoDate).getTime()) / 60000;
  return relativeTimeFromMinutes(minutes);
}
