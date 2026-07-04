// Same 7-hexagon geometry as HiveLogo, flattened to one faint monochrome
// outline and blown up huge — a background watermark for the login screen,
// the way OpenShift's login page sits a giant grayed-out Red Hat logo behind
// the card. Fill stays almost nonexistent; the stroke is what needs to carry
// this, since a honeycomb only reads as a honeycomb (a grid of cells) with
// its cell borders visible — a solid low-opacity fill just blobs together.
export default function HiveWatermark() {
  return (
    <svg
      viewBox="0 0 100 100"
      aria-hidden="true"
      className="pointer-events-none absolute left-1/2 top-1/2 h-[640px] w-[640px] -translate-x-1/2 -translate-y-1/2 opacity-[0.07] sm:h-[820px] sm:w-[820px]"
    >
      <g fill="oklch(0.85 0.01 254 / 0.15)" stroke="oklch(0.85 0.01 254)" strokeWidth="0.6">
        <polygon points="65.59,20.00 79.88,28.25 79.88,44.75 65.59,53.00 51.30,44.75 51.30,28.25" />
        <polygon points="34.41,20.00 48.70,28.25 48.70,44.75 34.41,53.00 20.12,44.75 20.12,28.25" />
        <polygon points="50.00,60.50 64.29,68.75 64.29,85.25 50.00,93.50 35.71,85.25 35.71,68.75" />
        <polygon points="50.00,6.50 64.29,14.75 64.29,31.25 50.00,39.50 35.71,31.25 35.71,14.75" />
        <polygon points="34.41,47.00 48.70,55.25 48.70,71.75 34.41,80.00 20.12,71.75 20.12,55.25" />
        <polygon points="65.59,47.00 79.88,55.25 79.88,71.75 65.59,80.00 51.30,71.75 51.30,55.25" />
        <polygon points="50.00,33.50 64.29,41.75 64.29,58.25 50.00,66.50 35.71,58.25 35.71,41.75" />
      </g>
    </svg>
  );
}
