<p align="center">
  <img src="Design/assets/hively-lockup-4x1-transparent.png" alt="Hively" width="360">
</p>

# Hively

A data catalog for an MQTT Unified Namespace. If you know Confluent's Data Catalog for Kafka, it's that idea, just for MQTT/UNS instead — which doesn't really have a decent affordable equivalent today.

In a physical Unified Namespace, every topic path encodes where a piece of equipment lives in the plant (`enterprise/region/country/site/area/line/cell/equipment/...`). That's elegant until a machine physically moves — relocate a press from a German plant to a US one and every one of its topics changes path and drops off the map simultaneously, even though nothing about the actual sensor data changed. Hively exists mainly to make that survivable: it tracks who produces and consumes each topic, what schema the payload should match, and gives admins a fast way to recognize "oh, this new untracked topic is actually just the old one that moved" instead of re-cataloguing a whole namespace from scratch.

## What it does

- **Catalogues topics** as they show up on the broker — anything Hively has never seen gets flagged untracked, with a live count in the sidebar, rather than silently getting lost.
- **Bulk-assigns producers, schemas, and tags via rules** (MQTT wildcard patterns, ranked by specificity) so you're not hand-configuring fifty sub-topics off the same physical device one at a time. Consumers are the one thing a rule can't set — which device produces a topic follows from its pattern, but who consumes it doesn't, so that stays manual. A rule can also be marked to auto-apply itself the moment it's the only one that matches a new topic — multiple matches still always need a human to pick.
- **Validates schema compliance** against the last message seen on each topic. Schemas are defined once in a shared catalog (not per-topic — that was an earlier design that got scrapped for exactly the reason you'd expect: too much duplication) and mirror the payload's actual JSON shape, nested objects included, with fields markable as optional or as one of a few possible types (`"string|number"`) for values that genuinely vary by message.
- **Recovers from relocations.** When a topic goes untracked, Hively looks for an existing tracked topic whose last few path segments match but whose full path differs, and which has gone quiet for a while — a strong signal it's the same physical measurement point under a new address. An admin confirms the relink; nothing merges silently.
- **Pushes updates live** over SignalR — new untracked topics, compliance changes, broker connectivity — instead of polling.
- **A relationship graph** for tracing producer → topic → consumer connections visually, centered on whatever entity you search for, with animated edges showing which way the data actually flows.

## Screenshots

<!--
  Drop real screenshots of the running app into docs/screenshots/ using these
  filenames and they'll show up here:
    topics-list.png      — Topics list, hierarchy view, untracked callout
    topic-detail.png      — Topic detail page (producer/schema/compliance/activity cards)
    configure-topic.png    — Configure Topic modal
    manage-schemas.png     — Manage Schemas panel
    graph-view.png         — Producer/consumer relationship graph
-->

| | |
|---|---|
| ![Topics list](docs/screenshots/topics-list.png) | ![Topic detail](docs/screenshots/topic-detail.png) |
| ![Configure topic](docs/screenshots/configure-topic.png) | ![Graph view](docs/screenshots/graph-view.png) |

## How it's put together

Backend is ASP.NET Core on .NET 10, strictly layered — controllers only handle auth and HTTP concerns, services hold the actual business logic (rule matching, the relink heuristic, schema validation), repositories are the only thing that touches EF Core. Postgres for storage, chosen mainly for the JSON support since schema definitions and MQTT payloads are both JSON documents. MQTT ingestion runs as a background hosted service on MQTTnet, pinned to the 4.x line because `MQTTnet.Extensions.ManagedClient` — which is what gives us automatic reconnect — doesn't have a build against MQTTnet 5 yet.

There's no EF migrations folder. The schema is hand-written SQL in `Design/schema.sql` (kept importable into drawdb.app for diagramming), applied to Postgres directly, and the C# entities are reverse-engineered from the live database rather than the other way around.

Frontend is React 19 + TypeScript, Tailwind v4 with an OKLCH palette, no icon library (typography, color, and a handful of CSS shapes do all the work — turns out you don't need `lucide-react` for a dashboard-shaped app). State is local component state plus small custom hooks; no Redux, no React Query. Given the size of this app that's genuinely simpler to reason about than it sounds, not a corner cut for lack of time.

Auth is real OIDC — Google and Microsoft Entra ID — not a mocked session cookie. App-level roles (Admin/Viewer) live in Hively's own `Users` table, looked up by the external identity's email, not pulled from the identity provider. First person to ever log in becomes Admin automatically; everyone after that starts as a Viewer and gets promoted by an existing Admin.

## Running it locally

You'll need .NET 10, Node 20+, and either Docker or Podman.

```bash
# Postgres + a local Mosquitto broker
docker compose up -d      # or: podman compose up -d

# Backend (also launches the Vite dev server via SpaProxy)
dotnet run --project Hively.Server

# ...or if you just want the frontend on its own
cd hively.client && npm run dev
```

The backend listens on `https://localhost:7295` by default (see `Hively.Server/Properties/launchSettings.json` if you need to change it).

Google and Entra ID OAuth client credentials go in .NET user-secrets, never in `appsettings.*.json` — see `docs/deployment.md` for the exact config keys, whether you're wiring up real credentials or deploying somewhere.

Changing the database schema means editing `Design/schema.sql` first, applying it to Postgres by hand (`psql` or `podman exec ... psql`), then re-running `dotnet ef dbcontext scaffold` to regenerate the C# entities — never edit the generated `DbModel/` classes directly, they'll just get overwritten.

## Where things stand

Backend: all six core entities (Topics, Producers, Consumers, Schemas, Tags, Rules) have full CRUD, the rule-matching and relocation-relink logic is wired in and working against a real broker, real OIDC auth is enforced across every endpoint, and SignalR pushes topic/rule/broker-status changes live. Frontend covers the full catalog: the topic list and detail views, all the admin management panels, the relationship graph, and the login flow.

Not there yet: this hasn't been run against production traffic at any real volume, and there's no automated test suite — everything so far has been verified by hand against a real broker and a real Postgres instance. Take that into account before pointing it at anything that matters.

## License

Not yet decided — treat this as source-available for now, not open source, until that's settled.
