# Hively — CLAUDE.md

Hively is a data-catalog application for an MQTT Unified Namespace (UNS) — think "Confluent Data Catalog, but for MQTT." It tracks topics, their producers/consumers, assigned schemas + compliance, tags, and bulk-assignment rules, and helps admins recover when a physical relocation makes a whole branch of the namespace go untracked at once.

**Full domain spec, behaviors, and design tokens live in `Design/README.md` — read it before implementing any feature.** This file only covers architecture/conventions; it deliberately does not repeat the domain model, key behaviors (rules, relocation heuristic, search, graph view, etc.), or design tokens documented there. `Design/MQTT Data Catalog.dc.html` is a clickable prototype — open it in a browser to see exact behavior before building a screen.

## Reference project

`../RMCD-App` (sibling directory) is the architectural template for this project — same tech family (ASP.NET Core + React, `.esproj`-hosted SPA), further along in development. When unsure how to structure something, check how RMCD-App does it before inventing a new pattern. This doc encodes the conventions pulled from it; where Hively intentionally diverges (see Decisions below), that's called out explicitly.

## Decisions made for this project

| Area | Choice | Notes |
|---|---|---|
| Database | PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`) | RMCD-App uses MySQL/Pomelo; Hively uses Postgres instead — good JSON support for schema definitions/payloads. |
| MQTT ingestion | Real client from day one, `MQTTnet` | Not mocked — wire up a real background hosted service against a real broker. |
| Live updates | SignalR from day one | Matches `Design/README.md`'s suggestion. Frontend gets a real hub connection, not polling. |
| Auth | OIDC login via **both** Microsoft Entra ID and Google | No manual session-cookie auth like RMCD-App — use ASP.NET Core's real authentication middleware. App-level roles (Admin/Viewer) are looked up from our own `Users` table by external identity, not from the identity provider. |
| Icons | None, except OAuth provider marks | Per design doc: no icon font/SVG iconography, typography + color + CSS shapes only. Don't add `lucide-react` or similar even though RMCD-App does. One deliberate exception: `pages/Login/ProviderIcons.tsx` inlines the official Google/Microsoft logo marks on the login buttons, since both providers' own sign-in branding guidelines call for them — not general app iconography, so it doesn't reopen the door to an icon library. |
| Frontend state | Local component state + small custom hooks | No Redux/Zustand/React Query. RMCD-App proves this scales fine for an app this size, and it's the simplest mental model to hold — important since the user is still learning React. |

---

## Backend: 3-layer architecture

`Controllers → Services → Repositories`, strictly one-directional (controllers never touch `DbModel`/EF directly, services never touch `HttpContext`).

```
Hively.Server/
  Controllers/              one controller per aggregate root (TopicController, ProducerController, ...)
  Services/
    Abstractions/            I<Name>Service interfaces
    <Name>Service.cs
  Repository/
    Abstractions/            I<Name>Repository interfaces
    <Name>Repository.cs
  DbModel/                   EF Core entities + the DbContext (HivelyContext)
  Dto/                       request/response shapes, never expose DbModel entities over the wire
  Exceptions/                EntityNotFoundException, etc.
  Infrastructure/            strongly-typed settings classes bound from appsettings (Options pattern)
  Hubs/                      SignalR hubs
  Services/Ingestion/        MQTT background hosted service (MqttIngestionService, TopicIngestionService)
```

Pure matching/validation helpers (`SchemaComplianceValidator`, `TopicPatternMatcher`, `RuleMatcher`, `RelinkHeuristic`) live directly in `Services/` as static classes, not in `Services/Ingestion/` — that subfolder holds only the hosted service itself and its ingestion-specific logic.

### Layer responsibilities

- **Controller**: auth/authorization only (`[Authorize]` / `[Authorize(Roles = "Admin")]`), model binding, calling exactly one service method, mapping exceptions to status codes. No business logic.
- **Service**: business logic, orchestration, validation rules from the domain (schema validation, rule specificity, relink heuristic). Maps between `Dto` and `DbModel` via DTO constructors (`new TopicDto(topicEntity)`). Throws domain exceptions (`EntityNotFoundException`, etc.) rather than returning null/bool for error cases.
- **Repository**: EF Core queries only. Returns/accepts `DbModel` entities, not DTOs. Throws `EntityNotFoundException` when a lookup by id fails so services don't need null-checks.

### Conventions (mirrored from RMCD-App)

- Every interface method takes a `CancellationToken` as the last parameter.
- DTOs have a constructor that maps from the DbModel entity (`public TopicDto(Topic topic) { ... }`) plus a parameterless constructor for deserialization.
- XML `/// <summary>` doc comments on every public controller action, service method, and repository interface method — RMCD-App does this consistently; keep matching it for cross-project consistency.
- Register every `I<Name>Service`/`I<Name>Repository` pair as `Scoped` in `Program.cs`, in one place, grouped by feature.
- Controller actions catch specific exception types in order (most specific first) and map to status codes: `EntityNotFoundException` → 404, validation/`InvalidOperationException` → 400, generic `Exception` → 500 (log first). Auth failures are now handled by `[Authorize]` middleware, not a manual cookie check per action (this is where Hively diverges from RMCD-App's manual `session_id` cookie check — we have real middleware now, use it).
- Route convention: `[Route("api/[controller]")]`, one controller per aggregate (`TopicController`, `ProducerController`, `ConsumerController`, `SchemaController`, `TagController`, `RuleController`, `UserController`).
- **`[FromBody]` request DTO gotcha**: any non-nullable reference-typed property (e.g. `string`) on a DTO used as `[FromBody]` input is treated as implicitly required by `[ApiController]`'s automatic model validation — the request 400s if the client omits it, even if your action never reads that field. Hit this twice already (`SchemaDto.Version`, `TopicDto.Path` during Topic *update*). Two valid fixes: make the field nullable if it's genuinely optional input (`SchemaDto.Version`, system-managed, ignored on insert), or split off a narrower request DTO scoped to just that action's real inputs when the same conceptual entity has very different-shaped operations (`TopicConfigureDto` vs `TopicDto` — configuring a topic never touches `Path`). Prefer the narrower-DTO fix when a field is required for *some* operations on the entity but not others; don't just make everything nullable.

### Logging

Console only for now (the default `ILogger` + Console provider ASP.NET Core wires up automatically) — no Serilog/Seq/structured sinks until there's an actual need for one.

- **Logging happens at the controller layer only.** Services/repositories throw; the controller's `catch` blocks are the single place a given failure gets logged, so nothing gets logged twice for the same request. Repositories/services don't take an `ILogger`.
- Every action logs on **both** the success and failure path, using structured placeholders (`_logger.LogInformation("Created producer {ProducerId} ({ProducerName}).", id, name)`) rather than string interpolation — keeps entries greppable/queryable even though the sink is plain console today.
- `EntityNotFoundException` (404 case) logs at `LogWarning`, not `LogError` — it's an expected, client-facing outcome, not a real error. Everything else in the generic `catch (Exception)` block logs at `LogError`.
- **Dev-only, never in Production** (configured in `Program.cs`, gated on `builder.Environment.IsDevelopment()`):
  - `options.EnableSensitiveDataLogging()` + `EnableDetailedErrors()` on the `HivelyContext` registration — logs full SQL text with parameter values for every query via the `Microsoft.EntityFrameworkCore.Database.Command` category.
  - `AddHttpLogging()` / `UseHttpLogging()` — logs every request/response (method, path, status, headers).
  - Both require **explicit category overrides** in `appsettings.Development.json`'s `Logging:LogLevel` — the existing `"Microsoft.AspNetCore": "Warning"` catch-all silently swallows `Microsoft.AspNetCore.HttpLogging` (and would swallow EF Core's category too if it didn't fall under `Default`) unless each is called out by its own explicit `"Information"` entry. Forgetting this is why a new log category can silently produce zero output — check `appsettings.Development.json` first if that happens.

### Database

- EF Core + `Npgsql.EntityFrameworkCore.PostgreSQL`. `HivelyContext : DbContext` in `DbModel/`.
- **No EF migrations** — there's no `Migrations/` folder and none is used. The schema is hand-authored as raw DDL in `Design/schema.sql` (kept drawDB-importable for schema design/diagramming), applied directly to Postgres; `DbModel/` entities are reverse-engineered from the live DB via `dotnet ef dbcontext scaffold` whenever the schema changes, not generated ahead of it. When adding/changing a table: edit `Design/schema.sql` first, apply it to Postgres by hand (`psql`/`podman exec ... psql`), then re-scaffold.
- Store `Topic.Segments` as a normalized child table or a delimited string (see `Design/README.md`'s EF Core note) — decide when building the Topic entity, not before; don't over-design this ahead of time.

### MQTT ingestion

- A single `IHostedService` (e.g. `MqttIngestionService`) using `MQTTnet`, subscribed to `#`, injected with a scoped-service-factory (hosted services are singletons; create a scope per message to resolve `ITopicService`/`HivelyContext`).
- On every message: upsert last payload/timestamp/retained on the matching topic (create an untracked stub if never seen), run schema validation if assigned (increment `violationCount` on failure), roll up the `activityHistogram` bucket.
- Keep `matchTopic`, `ruleSpecificity`, `findMatchingRules`, `findRelinkCandidate`, and schema `validateSchema` as pure, unit-testable functions — port them from the prototype's `<script>` block in `Design/MQTT Data Catalog.dc.html`, don't reinvent the logic.

### Real-time (SignalR)

- One hub (e.g. `TopicHub`) pushes: new untracked topic detected, topic last-message/compliance updated, violation count changed.
- The MQTT ingestion service and any service-layer mutation (e.g. clearing a violation counter, applying a rule) push through the hub after committing to the DB — don't let the frontend find out via a stale poll.

### Auth (Entra ID + Google + Basic)

- Cookie authentication as the sign-in scheme; both `AddOpenIdConnect("Entra", ...)` (Microsoft Entra ID) and `AddGoogle` (`Microsoft.AspNetCore.Authentication.Google`) as external challenge schemes signing into that cookie. Entra deliberately uses plain `AddOpenIdConnect` pointed at `login.microsoftonline.com/{tenantId}/v2.0` rather than `Microsoft.Identity.Web`'s `AddMicrosoftIdentityWebApp` — that package's token-cache/MSAL machinery is for apps calling Graph or other downstream APIs, which this app never does; we only need the ID token's email/subject claims.
- **A third provider, "Basic," is a single hardcoded username/password pair from config** (`Authentication:Basic:Username`/`Password`) — for self-hosted setups with no Entra tenant or Google Workspace domain available. No handler middleware, no redirect: `AuthController.LoginBasic` (`POST /api/auth/login/basic`) compares the submitted credentials directly against config **in cleartext, not hashed, for now** and, on a match, signs into the same cookie scheme via `HttpContext.SignInAsync`, going through `IUserService.UpsertFromExternalLoginAsync("basic", username, username, ct)` just like Google/Entra do so it gets the same Admin/Viewer role logic (there's no real email for a Basic login, so username doubles as both the external subject and the "email" field). `Login.tsx` renders it as a username/password form, not a button, since there's no redirect to leave the SPA for.
- **Each provider is opt-in via `Authentication:{Provider}:Enabled` (default `false` if unset)** — `Program.cs` reads the Google/Entra flags before the auth chain even starts, and only calls `.AddGoogle(...)`/`.AddOpenIdConnect("Entra", ...)` for whichever are `true`, so a deployment that only wants Entra never registers Google's handler at all (no need for real Google credentials to even be present). Basic doesn't need a `Program.cs` branch since it has no handler to conditionally register — `LoginBasic` itself checks `Authentication:Basic:Enabled` and 404s if it's off. `appsettings.Development.json` sets Google/Entra to `true` (Basic stays unconfigured/off, since this dev environment already has real Google/Entra credentials). `AuthController.Providers` (`GET /api/auth/providers`, anonymous) exposes which are enabled so `Login.tsx` knows which buttons/form to render; `LoginGoogle`/`LoginEntra` also 404 directly if hit while their provider is disabled, rather than throwing the framework's own "no handler registered for scheme" exception.
- Provider secrets (`Authentication:Google:ClientId/ClientSecret`, `Authentication:Entra:ClientId/ClientSecret/TenantId`) live in .NET user-secrets, never in `appsettings.*.json` — the `Enabled` flags aren't secrets and do live in `appsettings.Development.json`/deployment env vars.
- **Google login can be restricted to a single Workspace domain via `Authentication:Google:AllowedHostedDomain`** (optional — unset allows any Google account, including personal `@gmail.com` ones), mirroring what `TenantId` already does for Entra. Two parts, both in the `AddGoogle` config: `OnRedirectToAuthorizationEndpoint` adds `hd=<domain>` to the outgoing authorization request (a UX nicety only — pre-filters Google's account chooser — not a real restriction, since a client could strip that query param), and `OnCreatingTicket` re-checks the `hd` claim on the *response* server-side, calling `ctx.Fail(...)` on a mismatch or a missing claim (personal accounts never have `hd` at all) — that's the actual enforcement. `OnRemoteFailure` redirects a failed/denied login back to `/login?error=access_denied` instead of showing a raw exception page; the frontend's `Login.tsx` reads that query param to show a toast.
- On first successful external login, upsert a row in our own `Users` table keyed by email/external subject id, defaulting to `Viewer`; an existing Admin promotes others via the `UserController` (no self-service admin signup).
- Authorize admin-only endpoints/actions with `[Authorize(Roles = "Admin")]`; read-only endpoints just need `[Authorize]`.
- **Entra's `options.ResponseMode` is forced to `"query"`, explicitly overriding the handler's `form_post` default.** Hit `AuthenticationFailureException: message.State is null or empty` against the local dev proxy chain (Vite → SpaProxy → Kestrel) — Google's OAuth handler never showed it since it already calls back via a plain GET (`?code=...&state=...`); OIDC's default `form_post` callback POSTs code/state as a form body, which is far more fragile through that chain (either the body doesn't arrive intact, or the correlation cookie's `SameSite=None`+`Secure` requirements for surviving a cross-site POST get tripped up by scheme detection behind the proxy). Forcing `query` sidesteps both failure modes at once — a GET has no body to lose, and its correlation cookie only ever needs `SameSite=Lax`.

---

## Frontend: React, heavily modularized

Small, single-purpose files over big ones — optimize for "easy to find and hold in your head," not for fewest files. This is the actual built structure (it diverges from the original plan in a few places — noted inline — where building the real thing turned out simpler than the upfront guess):

```
hively.client/src/
  components/
    ui/            generic, app-agnostic building blocks — Button, Modal, Input, Select, Badge, ToastContainer — one file each, index.ts barrel
    layout/        Header, HiveLogo, BrokerStatusPill (no Footer — the design is a fixed-height dashboard with no footer in the prototype; adding one would've been fabricating UI, not fidelity)
    navigation/    NavLink, ProtectedRoute, ScrollToTop
    shared/        ComplianceBadge, ConfirmModal, ManageList, TagPill, SearchableCombobox (single-select list, producer/schema), MultiSelectCombobox (multi-select list, consumers)
  pages/
    Topics/
      Topics.tsx              page shell — owns the /topics/* sub-routes (index = list, :topicId = TopicDetail)
      components/             NamespaceSidebar, TopicListView, TopicRow
      modals/                 ConfigureTopicModal, AddTopicModal
      hooks/                  useTopicFilters, useDisplayGroups
      index.ts                `export { default } from './Topics'`
    TopicDetail/               its own top-level page (not nested under Topics/) since it's routed independently
      TopicDetail.tsx
      cards/                   ProducerCard, ConsumerCard, SchemaCard, LastMessageCard, ComplianceCard, ActivityCard, RelatedTopicsCard
      modals/                  ProducerAssignModal, SchemaAssignModal (ConfigureTopicModal is reused from Topics/ for the untracked case)
    Manage/                    Tags/Schemas/Rules/Producers/Consumers/Users are modals off the header's Manage dropdown, not separate routed pages — matches the prototype's overlay-on-catalog UX (Design/README.md), which the original plan below got wrong by listing them as page directories
      ManageMenu.tsx           the dropdown trigger + which-panel-is-open state
      panels/                  ManageTagsPanel, ManageSchemasPanel, ManageRulesPanel, ManageProducersPanel, ManageConsumersPanel, ManageUsersPanel
    Graph/
      Graph.tsx  hooks/useGraphLayout.ts  index.ts
    Login/
      Login.tsx  index.ts                OIDC redirect buttons, no password form
  services/
    apiService.ts              generic fetch wrapper — every other service goes through this, never call fetch directly elsewhere
    topicService.ts  producerService.ts  consumerService.ts  schemaService.ts  tagService.ts  ruleService.ts  userService.ts  authService.ts  mqttStatusService.ts
    signalr.ts                  connection factory for the shared TopicHub client
  hooks/
    data/                       one hook per entity (useTopics, useTopic, useProducers, useConsumers, useSchemas, useTags, useRules, useUsers, useMqttStatus) — useState + useEffect + loading/error/refetch, calling the matching service
    ui/                         useModalState (useFormState from the original plan was never built — no concrete form needed it beyond plain useState)
  contexts/                     AuthContext (current user + role, OIDC redirect login/logout), ToastContext, SignalRContext (one shared TopicHub connection, started once authenticated)
  types/
    api.ts                      shapes mirroring backend DTOs
    ui.ts                       frontend-only view types (TopicViewMode, Toast)
    index.ts                    `export * from './api'; export * from './ui';`
  constants/
    apiEndpoints.ts             (validation.ts from the original plan was never built — no shared per-field validation rules ended up needed)
  utils/                        tagColor, searchMatch (substring/wildcard/glob), relativeTime, topicPath (LEVEL_NAMES etc.), ruleMatch (client-side rule specificity for list-row hints), graphNodeStyle
  App.tsx  main.tsx
```

### Conventions

- **All backend calls go through `services/apiService.ts`.** Per-entity service files (`topicService.ts`, etc.) are the only callers of `apiRequest<T>()`; components/hooks never call it directly. This is the one required layer — everything else above is decomposition for readability.
- Data-fetching hooks (`hooks/data/*`) are the only things that call entity services from UI code. A page component calls a hook, not a service, directly.
- Routing via `react-router-dom`: `ProtectedRoute` wraps everything except `/login`, with an `adminOnly` prop for Admin-gated routes. The Manage panels aren't routes at all — `ManageMenu` only renders for `isAdmin`, so there's nothing to route-guard there.
- Styling: Tailwind CSS v4 (`@tailwindcss/vite`). Dark theme, OKLCH palette, IBM Plex Mono for topic paths/code/patterns/ids, Space Grotesk for the HIVELY wordmark specifically (a deliberate deviation from Design/README.md's monospace wordmark spec, per user preference) — see `Design/README.md` → Design Tokens for the rest.
- No icon library — see Decisions table above.
- A page's `index.ts` re-exports its default component (`export { default } from './Topics'`) so imports elsewhere stay short.
- `SearchableCombobox`/`MultiSelectCombobox` (`components/shared/`) both support an optional `onCreate` — typing a search term with no exact match shows a "+ Create" option, so producer/consumer pickers across Configure/Assign modals and the Rules form can create one inline instead of forcing a trip to the Manage panel first.

---

## Status

**Backend**: all six core entities have a full 3-layer CRUD slice, each built, run, and verified end-to-end against a real Postgres instance (not just compiled) — `Producer`, `Consumer`, `Tag`, `Schema` (with version history), `Rule` (with producer/schema/tag assignment), `Topic` (with schema compliance validation via `SchemaComplianceValidator`). Check `git log --oneline` for the exact commit per entity.

The rule-matching engine and relocation-relink heuristic are also built and verified end-to-end: pure functions `TopicPatternMatcher.MatchTopic`, `RuleMatcher.RuleSpecificity`/`FindMatchingRules`, and `RelinkHeuristic.FindRelinkCandidate` (`Hively.Server/Services/`), ported from the prototype's `matchTopic`/`ruleSpecificity`/`findMatchingRules`/`findRelinkCandidate`. Wired into `ITopicService`/`TopicController`: `GET /api/topic/{id}/matching-rules`, `POST /api/topic/{id}/apply-rule/{ruleId}`, `GET /api/topic/{id}/relink-candidate`, `POST /api/topic/{id}/accept-relink/{oldTopicId}`.

Real auth (see Auth section above) is also built and verified end-to-end against real Google and Entra ID accounts, resolving through `IUserService.UpsertFromExternalLoginAsync` (`Users`/`UserIdentities` tables, matches the algorithm documented in `Design/schema.sql`). The very first login ever is auto-promoted to Admin; everyone after defaults to Viewer. `AuthController` (`login/google`, `login/entra`, `logout`, `me`) and `UserController` (Admin-only user list + role promotion) are the new endpoints. All six CRUD controllers now carry `[Authorize]` on reads and `[Authorize(Roles = "Admin")]` on mutations — verified that anonymous requests 401 across the board.

MQTT ingestion is also built and verified end-to-end against a real broker and Postgres: `MqttIngestionService` (`Hively.Server/Services/Ingestion/`, a `BackgroundService` using `MQTTnet`'s `IManagedMqttClient` for automatic reconnect, pinned to the 4.x line since `MQTTnet.Extensions.ManagedClient` has no build against MQTTnet 5) subscribes to `#` and, per message, opens a DI scope and calls `ITopicIngestionService.IngestMessageAsync` (`Services/Ingestion/TopicIngestionService.cs`), which upserts the matching topic via `ITopicRepository.FindTopicByPathAsync`/`RecordIngestedMessageAsync` — creating an untracked stub on first sight of a path, running `SchemaComplianceValidator` to increment `violationCount` on failure, and rolling `activityHistogram` forward via the pure `ActivityHistogramRoller.Roll` (`Services/`). Non-JSON payloads are wrapped as a JSON string scalar rather than rejected. Broker connection is configured via the `MqttBrokerSettings` options class (`Infrastructure/`), bound from the `MqttBroker` config section — see `docs/deployment.md` for the container env var names.

The MQTT broker's own connection state (not just ingestion's success/failure) is tracked separately: `IMqttStatusService`/`MqttStatusService` (`Services/`, Singleton — it's written from the managed MQTT client's connect/disconnect callbacks and read from request threads) holds a snapshot (connected, host, port, connected-since, last disconnect reason) behind a lock, pushes `BrokerStatusChanged` through `TopicHub` on every change, and backs `GET /api/mqtt-status` (`MqttStatusController`) for the initial fetch. This is what the frontend header's connection pill shows — it used to just reflect the browser's own SignalR websocket, which was true but not what anyone actually wants to know.

Rule auto-application on ingestion **is** built (this reverses what used to be explicitly out of scope): `rules.auto_apply` lets an admin opt a specific rule in to applying itself the moment it's the *only* rule matching a newly-untracked topic — `IAutoRuleApplier`/`AutoRuleApplier` (`Services/`) is the shared check, called from both `TopicIngestionService` (first sighting) and `TopicService.InsertTopicAsync` (the manual stub-insert endpoint). Multiple matching rules still always require a manual pick — this only ever fires when there's exactly one match.

Separately, saving a rule (`RuleService.InsertRuleAsync`/`UpdateRuleAsync`) immediately calls `ITopicService.ApplyRuleToAllMatchingAsync` for that rule, re-applying it to every topic matching its pattern — tracked or not, and regardless of `AutoApply` — since rule application otherwise only ever fires once, at first tracking, and an edited rule (a schema added after the fact, say) would never reach a topic it had already configured without this. There's no separate manual "apply to all" endpoint anymore (the old `POST /api/rule/{id}/apply-to-all` was removed) — re-saving the rule, even with nothing changed, achieves the identical bulk re-apply through this same automatic path.

The SignalR hub is also built and verified end-to-end: `TopicHub` (`Hively.Server/Hubs/`, `[Authorize]`, mapped at `/hubs/topic`) pushes through the strongly-typed `ITopicHubClient` interface — `TopicUntracked`/`TopicUpdated`/`TopicRemoved`, plus `BrokerStatusChanged` and `RulesChanged` (both cross-cutting events reusing this same hub rather than standing up a second connection for one small signal each). `ITopicNotifier`/`TopicNotifier` and `IRuleNotifier`/`RuleNotifier` (`Services/`) wrap `IHubContext<TopicHub, ITopicHubClient>` and are injected wherever a mutation needs to announce itself after committing — first sighting of a topic path (ingestion, the manual insert endpoint, or a topic that got auto-applied straight to tracked) fires `TopicUntracked`/`TopicUpdated` as appropriate, every other topic mutation fires `TopicUpdated`/`TopicRemoved`, and any rule create/edit/delete fires `RulesChanged` (no payload — clients just refetch, since what changed is cheaper to recompute client-side than to model server-side). Verified against a real broker and a standalone SignalR client, including that anonymous hub connections 401.

Schema compliance validation (`SchemaComplianceValidator`, `Services/`) is considerably more capable than the prototype's flat `{field: type}` map it started from: the definition now mirrors the payload's own shape (a nested object in the definition recurses into the payload's matching object instead of needing dotted-path keys), leaf types can be `|`-combined into a union (`"string|number"`, for values like some KNX datapoints that report as a string sometimes and a number other times) or declared `"any"` to allow any JSON kind, and a trailing `?` marks a field optional (missing/null is fine; a present value still has to match). `"object"`/`"array"`/`"null"` are also valid leaf types, for asserting a field's JSON kind without checking what's inside it.

Duplicate-entity conflicts (inserting/renaming to a `Topic.Path`, `Producer.Name`, `Consumer.Name`, or `Tag.Id` that already exists) return 409 with a clean message instead of a bare 500 — `DbUpdateExceptionExtensions.IsUniqueViolation()` (`Repository/`) lets a repository tell a Postgres unique-violation apart from any other `DbUpdateException` via an exception filter and throw the new `DuplicateEntityException`, which controllers catch alongside `EntityNotFoundException`/`InvalidOperationException`.

**Frontend**: fully built out, covering everything in the frontend structure described above. `App.tsx` routes `/login` (OIDC redirect buttons, no password form), `/topics` (+ `/topics/:id` for detail), and `/graph` behind `ProtectedRoute`; `AuthContext`/`SignalRContext`/`ToastContext` and one hook per entity under `hooks/data/` are all wired to the real API and the real `TopicHub` connection, not mocked. The Topics list has the namespace drill-down sidebar, untracked-topic callout, tag filter chips, substring/wildcard search, and Hierarchy/List grouping; Topic Detail has all the producer/consumer/schema/compliance/activity/related-topic cards plus `ConfigureTopicModal` (relink suggestion and rule-match/conflict banners included) and a delete flow gated behind a confirmation dialog rather than the Manage panels' instant `×`. All five original Manage panels (Tags/Schemas/Rules/Producers/Consumers) are built as modals off the header's Manage dropdown, matching the prototype's overlay-on-catalog UX rather than separate routed pages, plus a sixth (Users) for Admin promotion/demotion that the prototype never had since it only ever mocked a role toggle. The Graph view has the full ring layout, HTML label overlay (not SVG `<text>`, matching the prototype's documented workaround), zoom, drill/back navigation, and an animated dash overlay on each edge showing the true producer→topic→consumer flow direction regardless of which entity the graph is centered on — also not in the original prototype. Producer/consumer pickers across the app support creating a new one inline (a "+ Create" option appears when the search text has no match) instead of forcing a trip to the Manage panel first.

**Deliberately not built yet**:
- **No automated test suite.** Everything above has been verified by hand against a real broker and a real Postgres instance, not via unit/integration tests.
- **Full-namespace graph overview** (all producers/consumers as nodes network-wide, edge thickness representing aggregate topic count) — the per-entity star/ego-graph is built; the namespace-wide view was explicitly deferred in Design/README.md's Graph section due to legibility/performance concerns at real scale, and hasn't been revisited since.
- **Not load-tested.** Nothing here has been run against production-scale traffic — verification so far is "does it work correctly," not "does it hold up under load."

## Dev commands

- Backend: from repo root, `dotnet run --project Hively.Server` (needs `--project` if not already inside that directory) or via the `.slnx`/IDE run config — SpaProxy launches the Vite dev server automatically. **Run the "https" profile** (`https://localhost:8443;http://localhost:8080`), not "http" (`Properties/launchSettings.json`) — the http-only profile has no HTTPS endpoint, which silently breaks Google/Entra login (the OIDC/OAuth redirect_uri gets computed as `http://` instead of `https://`, since Vite's proxy target falls back to whatever `ASPNETCORE_URLS` the running profile actually set).
- Frontend only: `npm run dev` from `hively.client/`.
- Local Postgres + a local Mosquitto broker: `docker-compose.yml` at repo root (works with `podman compose up -d` or `docker compose up -d`); Postgres connection string already set in `appsettings.Development.json` to match its defaults (`localhost:5432`, db/user `hively`). Point `MqttBroker` in `appsettings.Development.json` at a real broker instead if you have one (e.g. one running in a cluster) rather than the local container.
- Schema changes: no EF migrations — edit `Design/schema.sql`, apply by hand to Postgres, then re-scaffold `DbModel/` with `dotnet ef dbcontext scaffold` (see Database section above).
- Lint: `npm run lint` (`oxlint`) from `hively.client/`.
- Container image: built from the root `Dockerfile` (multi-stage, builds the SPA and publishes the API into one image), published to `ghcr.io/gepf-solutions/hively`. Takes all config via env vars, no user-secrets inside the container — see `docs/deployment.md` for the full variable list and a `docker run` example.
