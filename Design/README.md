# Handoff: Hively — UNS (MQTT) Data Catalog

## Overview
**Hively** is a data-catalog application for a **Unified Namespace (UNS)** running over MQTT — the MQTT-world equivalent of tools like a Kafka schema/data catalog (e.g. Confluent Data Catalog), which don't have a good affordable analogue for MQTT/UNS today. The name/logo riff on a beehive: an organized honeycomb of cells, standing in for a well-governed namespace of topics.

Core purpose: let plant/OT engineers and admins understand and govern the topics flowing across their broker:
- Who **produces** a given topic, who **consumes** it
- What **schema** the payload should match, and whether the **last message** actually complies
- Auto-detect topics seen on the broker that haven't been catalogued yet (**untracked**), and give admins a fast path to bring them under management — individually, or in bulk via pattern-based **rules**
- Free-form, admin-defined **tags** (colored pills) for organizing/filtering topics
- Survive the fact that in a physical UNS, a machine's topic path encodes its **physical location** in the plant hierarchy — when a machine physically relocates (e.g. moves between plants/countries), all of its topics change path and go "untracked" simultaneously. The catalog needs to help admins recognize and recover from this instead of starting from scratch.

## About the Design Files
The bundled file (`MQTT Data Catalog.dc.html`) is a **high-fidelity, fully-interactive HTML/React prototype** built as a single self-contained file with an in-memory mock data store (no real backend, no real MQTT connection — all data is hardcoded sample data and all "writes" only mutate in-memory React state). It is a **design reference**, not code to port directly.

Your task is to **recreate this design and its behavior in a real C# + React application**:
- **Backend**: C# (ASP.NET Core Web API recommended), with a real MQTT client/listener, persistence (e.g. EF Core + SQL), and REST/GraphQL/SignalR APIs for the React frontend.
- **Frontend**: React (the prototype's component/state shape below is a reasonable starting point, but use the target codebase's existing component library, routing, state management, and styling conventions instead of copying the prototype's inline-style approach).

Open `MQTT Data Catalog.dc.html` directly in a browser to interact with the live prototype — clicking through it is the fastest way to understand exact behavior. It only needs a static file server (or double-click to open); it has no build step and no server dependency.

## Fidelity
**High-fidelity.** Colors, spacing, typography, copy, and all interaction flows below are final intent, not placeholders. Recreate the UI and behavior precisely using the target codebase's component library/design system if it has one; if the target app has no established visual system yet, this prototype's dark, technical/dev-tool aesthetic (think Grafana/Datadog) should be treated as the visual direction.

---

## Domain Model

### Topic
The central entity. A topic corresponds to one full MQTT topic path (a leaf in the namespace tree).

| Field | Type | Notes |
|---|---|---|
| `path` (segments) | `string[]` | Full topic path split on `/`. In the prototype, paths are **12 levels deep** following an ISA-95-flavored hierarchy: `enterprise / region / country / site / area / line / cell / equipment / module / subsystem / sensor / measurement`. Real deployments may vary in depth — do not hardcode 12; derive level labels/behavior generically from position. |
| `tracked` | `bool` | `false` = seen on the broker but never catalogued ("untracked"). |
| `producerId` | `Producer?` (nullable FK) | Nullable — "unknown producer" is valid. |
| `consumerIds` | `Consumer[]` (many-to-many) | Zero or more. |
| `schemaId` | `Schema?` (nullable FK) | Nullable — "no schema assigned" is valid; compliance is then "unknown," not pass/fail. |
| `tagIds` | `Tag[]` (many-to-many) | Zero or more, admin-assigned. |
| `lastPayload` | `json` | Last observed message body, captured by the MQTT listener. |
| `lastSeenAt` | `datetime` | Timestamp of last observed message. |
| `retained` | `bool` | Whether the last message was an MQTT retained message. |
| `violationCount` | `int` | Running count of schema-validation failures since last cleared. |
| `lastClearedAt` | `datetime?` | When an admin last reset the violation counter. `null` = never cleared. |
| `activityHistogram` | `int[24]` | Messages/hour for the last 24h, for the sparkline chart. |

**Derived/computed (not stored, computed on read):**
- `compliant` — `null` if no schema assigned; else `true`/`false` from validating `lastPayload` against the assigned schema's field:type definition.
- `mismatches` — list of human-readable reasons the last payload fails the schema (`"'value' expected number, got string"`, `"'unit' missing"`).

### Producer
A registry entity (not free text) — canonical name + optional description. Prevents typos/duplicates across topics ("press-01-edge-gateway" vs "Press01Gateway").
| Field | Type |
|---|---|
| `id` | `Guid`/`string` |
| `name` | `string` |
| `description` | `string?` |

### Consumer
Same shape as Producer — a registry, not free text.
| Field | Type |
|---|---|
| `id` | `Guid`/`string` |
| `name` | `string` |
| `description` | `string?` |

### Schema
A **globally-defined, reusable** schema — NOT per-topic. Admins define schemas once in a central catalog, then assign an existing schema to any number of topics (this was an explicit pivot during design — earlier iteration had per-topic inline schema editing, which was rejected in favor of a shared catalog + assignment model).
| Field | Type | Notes |
|---|---|---|
| `id` | `Guid`/`string` | |
| `name` | `string` | e.g. "Sensor Reading (value+unit+quality)" |
| `definition` | `json` (field → type map, nestable) | Simple `{ "value": "number", "unit": "string" }` shape in the prototype. **Hively's implementation diverges from the prototype here**: the definition can nest — a field's value is either a leaf type expression or another field→type object, mirroring the payload's own shape so a nested object gets its own sub-fields checked. Leaf type names: `"string"`, `"number"`, `"boolean"`, `"object"`/`"array"`/`"null"` to require a field exist as that JSON kind without checking inside it (for payloads whose nested shape varies between messages), and `"any"` to require the field exist as any JSON kind at all. Leaf types can be `\|`-combined into a union (e.g. `"string\|number"`, for a field that's genuinely one of a few types depending on the message — some KNX datapoints report as a string sometimes and a number other times). A trailing `?` on a leaf type expression (e.g. `"string?"`, `"string\|number?"`) marks the field optional: missing or explicitly `null` is fine, but a present non-null value still has to match one of the declared types — required fields (no `?`) keep the old missing-is-a-violation behavior. No dotted-path keys — nesting is expressed as real nested JSON, not `"a.b.c"` strings, since that reads as a literal mirror of the payload. |
| `version` | `string` | e.g. `v2`, auto-incremented on edit. |
| `history` | `[{version, date}]` | Every edit creates a new version entry; old versions are kept for audit/traceability. |

Validation logic (`SchemaComplianceValidator`, `Hively.Server/Services/SchemaComplianceValidator.cs`): walks the definition tree alongside the payload. For each key: if the definition value there is a nested object, recurse (the payload value must also be an object, and nesting is always required — `?` only applies to leaf types); if it's a string, strip a trailing `?` (if present, marking the field optional), split the rest on `|` into candidate types, then check `typeof payload[key]` matches *any* of them — unless the field is optional and absent/null, which is never a violation. Collect all mismatches (don't short-circuit on first failure) so the compliance card can list every violated field, using dotted paths (e.g. `Metadata.Name`) only in the mismatch *messages*, not in how the definition itself is authored.

### Tag
Freely admin-defined colored pill.
| Field | Type |
|---|---|
| `id` | `string` (slug) |
| `label` | `string` |
| `hue` | `number \| null` | OKLCH hue angle driving a consistent color+background+border triad (`null` = neutral gray tag). A real app can store a hex/HSL color directly instead. |

Multiple tags per topic. No additional "status" concept beyond tags + the tracked/untracked + compliant/non-compliant states.

### Rule
Bulk-assignment tool for admins managing many topics from the same producer, or recovering from a namespace reshuffle (see "Relocation & Rules" below). Deliberately doesn't assign consumers — which physical device produces a topic (and what schema its payload should match) follows from the topic's pattern, but who *consumes* it doesn't follow the same way, so that stays a manual per-topic decision.
| Field | Type |
|---|---|
| `id` | `string` |
| `pattern` | `string` | MQTT-style topic filter using `+` (single-level wildcard) and `#` (multi-level wildcard), e.g. `acme/+/+/+/+/+/press-01/#`. |
| `producerId` | `Producer?` | Producer to assign when applied. |
| `schemaId` | `Schema?` | Schema to assign when applied. |
| `tagIds` | `Tag[]` | Tags to assign (unioned with the topic's existing tags) when applied. |

---

## Key Behaviors

### 1. Untracked topic detection & management
- Any topic path observed on the broker that has no matching `Topic` record (or has `tracked=false`) is flagged **UNTRACKED** in the UI (amber/orange accent, left border stripe) and excluded from compliance stats.
- Admins can click **Configure** on any untracked row to open a modal where they set producer, consumers, schema, and tags, then **"Mark as Managed"** flips `tracked=true`.
- A sidebar callout shows a live count of untracked topics with a one-click filter to see only those.

### 2. Rules — bulk producer/schema/tag assignment
Real UNS deployments have many sub-topics per physical device (e.g. `press-01/temperature`, `press-01/pressure`, `press-01/vibration`) that all share one producer and, usually, one schema per measurement. Configuring each one-by-one doesn't scale. A **Rule** is a saved MQTT-pattern (`+`/`#` wildcards) + producer + schema + tag set (consumers deliberately excluded — see the Rule entity note above):
- When an untracked topic matches exactly one rule, the row shows a one-click **"⚡ Apply rule"** action.
- When an untracked topic matches **multiple** rules, don't guess — surface a conflict badge ("⚠ N rules match") that routes the admin to the Configure modal, where all matching rules are listed ranked by **specificity** (more literal path segments = more specific; `#` counts as least specific, `+` counts as half), the most specific pre-labeled "recommended," and the admin picks one explicitly.
- The Manage Rules panel also supports **"Apply to N now"** — retroactively bulk-apply a rule to every currently-untracked topic it matches.

### 3. Relocation resilience (the hard problem)
When a physical machine's location in the org changes (e.g. it moves between countries/sites), every segment of its topic path upstream of the device-level segments changes, so **all of its topics simultaneously go untracked** — even though nothing about the machine's actual measurements changed.

Mitigation implemented: a **relink suggestion**. When opening the Configure modal for an untracked topic, the app looks for an existing *tracked* topic whose **last N path segments are identical** (in the prototype, the last 6: cell/equipment/module/subsystem/sensor/measurement) but whose full path differs, AND which has gone silent for a long time (>12h, configurable `STALE_THRESHOLD_MIN`) — a strong signal it's the "old address" of the same physical measurement point. If found, the admin sees:
> "Looks like a relocation of `<old path>` (silent 3d ago) — inherit its producer, schema, tags & history?"

Accepting **inherits** producer, schema assignment, tags, and — critically — the violation counter and `lastClearedAt`, then **retires** the old topic record (rather than leaving a stale duplicate). This is a heuristic, not a certainty — the UI always shows it as a suggestion the admin confirms, never an automatic silent merge.

Rules (see above) are the complementary mechanism for the *proactive* case — if the rule's pattern uses wildcards for site/country, it keeps matching after a relocation without needing the relink heuristic at all.

### 4. Schema catalog & compliance
- Schemas live in a **global, searchable catalog** (Manage → Schemas), each with its own version history. Assigning a schema to a topic is a **selection**, not authoring a new schema inline.
- A topic's compliance badge (`COMPLIANT` / `N VIOLATIONS` / `NO SCHEMA — UNKNOWN`) is computed live by validating the last-seen payload against the assigned schema.
- `violationCount` is a **running counter incremented by the ingestion pipeline** every time an incoming message fails validation (not just reflecting the current message's pass/fail) — the detail view shows both the counter and the specific field-level mismatches for the *current* last message. Admins can **Clear** the counter once the root cause is fixed; the UI always shows "last cleared: `<relative time>` / never cleared" so nobody mistakes a stale counter for a live one.

### 5. Search
The topic search box accepts either:
- **Plain substring** search (default) — matched case-insensitively against the full topic path, resolved producer name, resolved consumer names, and tag ids.
- **Wildcard/pattern** search, auto-detected when the query contains `*`, `?`, `+`, or `#`:
  - `+`/`#` → interpreted as MQTT topic-filter wildcards (single-level / multi-level) matched against the path only, using the same matcher as Rules.
  - `*`/`?` → interpreted as glob wildcards (`*` = any run of characters, `?` = any single character), matched against the path only.

### 6. Roles
Two views, toggled in the prototype via a header switch (in the real app this should come from real auth/authorization, not a manual toggle):
- **Admin**: full CRUD on tags, schemas, rules, producers, consumers; can configure/manage topics, edit schema assignments, clear violation counters, reassign producer/consumers.
- **Viewer**: read-only browsing, search, and filtering. Untracked rows show "view only" instead of action buttons.

### 7. Namespace browser (deep hierarchy)
Because UNS paths are deep (10–15 segments is common; the prototype models 12), the sidebar is a **drill-down breadcrumb browser**, not a fully-expanded nested tree:
- Shows a breadcrumb of the currently-selected path prefix (starting from the enterprise root).
- Below it, lists the distinct **next-level segment names** under the current prefix, each with a topic count and an untracked-indicator dot; clicking drills in one level.
- This pattern scales to arbitrary depth without needing infinite nested indentation — generalize it rather than hardcoding "site"/"line" style fixed levels.
- The main topic list also offers **Hierarchy** (grouped into sections by physical cell) vs **List** (flat, alphabetically sorted) view modes, independent of the sidebar drill-down.

### 8. Producer/Consumer relationship graph (Graph tab)
A star/ego-graph view, separate from the topic catalog, for visualizing producer↔topic↔consumer relationships:
- User searches/selects a **focal entity** (a producer, a consumer, or a topic) via a type-to-search combobox.
- The focal entity renders at the center of an SVG canvas; its directly-related entities render on a ring around it (for a topic: its producer on one side, its consumers fanned on the other; for a producer/consumer: all its topics on ring 1, and *their* other-side entities aggregated on ring 2).
- Clicking any node re-centers the graph on that node (drill navigation), with a "← back" breadcrumb history and a "clear" reset.
- Nodes are color-coded and clearly distinguished: producer = cyan, topic = amber/gold, consumer = magenta — deliberately spread across distinct hues (an earlier iteration used two similar blues and was hard to read).
- Zoom controls (−/reset/+, 50–300%) scale the graph via CSS transform with the container scrollable for panning when zoomed in.
- **Known scaling limitation, discussed and deliberately deferred**: a literal "every topic as a node" full-UNS graph was considered and explicitly rejected for now due to potential performance/legibility problems at real-world scale (a UNS can have thousands of topics — a full hairball graph doesn't clarify anything). If a full-namespace overview is wanted later, the recommended approach (not yet built) is: show only producers + consumers as nodes (bounded, finite count), with edge thickness/label representing aggregate topic count between them rather than one edge per topic, and clicking a node drills into the existing per-entity star-graph where individual topics do appear. Untracked topics have no producer/consumer, so they'd need to render as a separate, unconnected "orphaned" cluster (e.g. dashed amber, possibly grouped/counted rather than one node per topic) rather than being silently omitted.

**Implementation note for the port**: node label positions are computed in JS (not CSS) as percentages, with a **vertical flip** — labels below the node in the top half of the canvas, above the node in the bottom half — specifically to keep labels from clipping the container edges at the ring's extremes. Preserve this if re-implementing the layout math. Also note labels are rendered as an HTML overlay `<div>` positioned over the SVG rather than as SVG `<text>` — this was a deliberate fix for a real bug (the prototype's templating layer wraps interpolated text in a tracking `<span>`, which is invalid inside SVG `<text>` and silently collapses to zero size in real browsers).

---

## Suggested C# Backend Architecture
This is a **suggestion**, not a spec — adapt to the target codebase's existing conventions if one exists.

- **ASP.NET Core Web API** exposing REST (or GraphQL) endpoints for: Topics (list/filter/get/update), Producers, Consumers, Schemas (+ version history), Tags, Rules.
- **MQTT ingestion service** (background hosted service, e.g. using `MQTTnet`) subscribed to `#` on the broker:
  - On every message: upsert last payload/timestamp/retained flag on the matching `Topic` (create an untracked stub row if the path has never been seen).
  - Run schema validation if a schema is assigned; increment `violationCount` on failure.
  - Roll up `activityHistogram` (e.g. via a time-bucketed counter, Redis, or a simple in-memory ring buffer flushed periodically).
- **SignalR hub** to push live updates to the React frontend (new untracked topics, updated last-message/compliance, violation count changes) rather than polling.
- **EF Core** entities mirroring the Domain Model section above; `Topic.Segments` can be stored as a delimited string or a normalized child table if you need to query by depth/segment efficiently.
- **Rule matching** and **relink heuristic** are pure functions over in-memory topic state — port `matchTopic`, `ruleSpecificity`, `findMatchingRules`, and `findRelinkCandidate` from the prototype's `<script>` block (readable JS, straightforward to transliterate to C#).

## Suggested React Frontend Structure
The prototype is one big component with inline styles and a flat state object — fine for a prototype, not for production. Recommended decomposition:
- `TopicListPage` (namespace sidebar + list/hierarchy toggle + row table)
- `TopicDetailPage` (producer/consumer/schema/compliance/activity cards)
- `ConfigureTopicModal`, `SchemaAssignModal`, `ProducerAssignModal`
- `ManageTagsPanel`, `ManageSchemasPanel`, `ManageRulesPanel`, `ManageProducersPanel`, `ManageConsumersPanel`
- Shared `SearchableCombobox` component (text filter + option list) — used ~7 times in the prototype for producer/schema/consumer pickers; worth extracting as one real reusable component.
- Global state via whatever the codebase already uses (Redux/Zustand/React Query, etc.) backed by the real API + SignalR live updates instead of the prototype's local `useState`.

---

## Design Tokens

**Logo/brand**: a small honeycomb (7-hexagon flower: 1 center + 6 ring cells) rendered as flat-color SVG polygons — no gradients. Center cell brightest (`oklch(0.9 0.15 98)`), 3 ring cells mid-gold (`oklch(0.83 0.17 95)`), 3 ring cells deeper amber (`oklch(0.68 0.14 85)`), thin dark-amber cell borders (`oklch(0.32 0.05 90)`). Sits next to the wordmark "HIVELY" (monospace, letter-spaced) in the header. See the inline `<svg>` in the header markup for exact hexagon coordinates (a 7-cell honeycomb inscribed in a 100×100 viewBox).

**Palette** (OKLCH, dark theme):
- Background (page): `oklch(0.17 0.014 254)`
- Background (panels/header): `oklch(0.20 0.015 254)` / header bar `oklch(0.155 0.012 254)`
- Borders: `oklch(0.28–0.32 0.02 254)`
- Primary text: `oklch(0.93 0.006 254)`
- Muted text: `oklch(0.5–0.6 0.012 254)`
- Accent (brand/primary actions, "compliant"): cyan, `oklch(0.75 0.13 200)`
- Warning/untracked/rules: amber, `oklch(0.78 0.15 80)`
- Danger/violations: red, `oklch(0.7 0.18 25)`
- Tag hues are arbitrary OKLCH hue angles (200 cyan, 25 red, 300 magenta, 240 blue, 150 green, 80 amber, null = neutral gray) each rendered as a `{color, background, border}` triad at fixed lightness/chroma — see `tagColor()` in the prototype's script.

**Typography**:
- UI text: system font stack (`-apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif`)
- Monospace (topic paths, code, patterns, IDs): `IBM Plex Mono` (weights 400/500/600), loaded from Google Fonts.
- Header wordmark: 13px, 600 weight, 1.5px letter-spacing, monospace, uppercase with a middle dot separator (`UNS·CATALOG`).

**Shape**: 6–12px border radii throughout; pill shapes (`border-radius: 99px`) for tags and compliance badges.

## Assets
No external image/icon assets — the UI uses only typography, color, and simple CSS shapes (dots, bars for the activity sparkline). No icon font or SVG iconography is used; keep it that way unless the target design system mandates otherwise.

## Logo Assets
`assets/` contains the Hively honeycomb logo as flat PNGs in a few ratios/uses:
- `hively-icon-1x1-transparent.png` / `hively-icon-1x1-dark.png` — square icon only (512×512), for app icons/favicons.
- `hively-lockup-4x1-transparent.png` / `hively-lockup-4x1-dark.png` — icon + "HIVELY" wordmark, wide lockup (1200×300), for headers/nav bars.
- `hively-banner-1.91x1-dark.png` — icon + wordmark + tagline, social/og-image ratio (1200×628).
All use flat colors (no gradients): center hex brightest gold, alternating ring hexes in two gold tones, dark amber-brown cell borders. Recreate at any size from the hex polygon coordinates in the header `<svg>` inside `MQTT Data Catalog.dc.html` if you need other sizes/formats (e.g. true vector SVG/ICO).

## Screenshots
See `screenshots/` for static reference images (the live prototype is more useful, but these help at a glance):
- `01-topic-list.png` — main topic list, Hierarchy grouping, untracked-topic callout, tag filter chips, namespace drill-down sidebar.
- `02-topic-detail.png` — topic detail page: producer/consumer/schema/last-message/compliance/activity cards.
- `03-configure-modal.png` — the "Configure Topic" flow for bringing an untracked topic under management (producer/consumer search-comboboxes, schema/tag assignment).
- `04-manage-schemas.png` — the global schema catalog (filter box, version badges, usage counts, new/edit schema form).
- `05-graph-view.png` — the producer/topic/consumer star-graph, centered on a topic (showing the Hively logo/wordmark in the header).

## Files
- `MQTT Data Catalog.dc.html` — the full interactive prototype. Self-contained (one file, loads its own fonts from Google Fonts, everything else inline). Open directly in any browser.
- `screenshots/` — static reference images, see above.
