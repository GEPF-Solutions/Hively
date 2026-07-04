// Flat-color honeycomb logo (7-hexagon flower), ported 1:1 from the polygon
// coordinates in Design/MQTT Data Catalog.dc.html's header markup — see
// Design/README.md "Logo/brand" for the color rationale. No gradients.
export default function HiveLogo({ size = 30 }: { size?: number }) {
  return (
    <svg viewBox="0 0 100 100" width={size} height={size} style={{ flexShrink: 0 }}>
      <polygon
        points="65.59,20.00 79.88,28.25 79.88,44.75 65.59,53.00 51.30,44.75 51.30,28.25"
        fill="oklch(0.68 0.14 85)"
        stroke="oklch(0.32 0.05 90)"
        strokeWidth="1.2"
      />
      <polygon
        points="34.41,20.00 48.70,28.25 48.70,44.75 34.41,53.00 20.12,44.75 20.12,28.25"
        fill="oklch(0.68 0.14 85)"
        stroke="oklch(0.32 0.05 90)"
        strokeWidth="1.2"
      />
      <polygon
        points="50.00,60.50 64.29,68.75 64.29,85.25 50.00,93.50 35.71,85.25 35.71,68.75"
        fill="oklch(0.68 0.14 85)"
        stroke="oklch(0.32 0.05 90)"
        strokeWidth="1.2"
      />
      <polygon
        points="50.00,6.50 64.29,14.75 64.29,31.25 50.00,39.50 35.71,31.25 35.71,14.75"
        fill="oklch(0.83 0.17 95)"
        stroke="oklch(0.32 0.05 90)"
        strokeWidth="1.2"
      />
      <polygon
        points="34.41,47.00 48.70,55.25 48.70,71.75 34.41,80.00 20.12,71.75 20.12,55.25"
        fill="oklch(0.83 0.17 95)"
        stroke="oklch(0.32 0.05 90)"
        strokeWidth="1.2"
      />
      <polygon
        points="65.59,47.00 79.88,55.25 79.88,71.75 65.59,80.00 51.30,71.75 51.30,55.25"
        fill="oklch(0.83 0.17 95)"
        stroke="oklch(0.32 0.05 90)"
        strokeWidth="1.2"
      />
      <polygon
        points="50.00,33.50 64.29,41.75 64.29,58.25 50.00,66.50 35.71,58.25 35.71,41.75"
        fill="oklch(0.9 0.15 98)"
        stroke="oklch(0.32 0.05 90)"
        strokeWidth="1.2"
      />
    </svg>
  );
}
