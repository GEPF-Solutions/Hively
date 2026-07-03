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
| Icons | None | Per design doc: no icon font/SVG iconography, typography + color + CSS shapes only. Don't add `lucide-react` or similar even though RMCD-App does. |
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
  Services/Ingestion/        MQTT background hosted service + pure matching/validation helpers
```

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
- Migrations via `dotnet ef migrations add <Name>` / `dotnet ef database update`, run from `Hively.Server/`.
- Store `Topic.Segments` as a normalized child table or a delimited string (see `Design/README.md`'s EF Core note) — decide when building the Topic entity, not before; don't over-design this ahead of time.

### MQTT ingestion

- A single `IHostedService` (e.g. `MqttIngestionService`) using `MQTTnet`, subscribed to `#`, injected with a scoped-service-factory (hosted services are singletons; create a scope per message to resolve `ITopicService`/`HivelyContext`).
- On every message: upsert last payload/timestamp/retained on the matching topic (create an untracked stub if never seen), run schema validation if assigned (increment `violationCount` on failure), roll up the `activityHistogram` bucket.
- Keep `matchTopic`, `ruleSpecificity`, `findMatchingRules`, `findRelinkCandidate`, and schema `validateSchema` as pure, unit-testable functions — port them from the prototype's `<script>` block in `Design/MQTT Data Catalog.dc.html`, don't reinvent the logic.

### Real-time (SignalR)

- One hub (e.g. `TopicHub`) pushes: new untracked topic detected, topic last-message/compliance updated, violation count changed.
- The MQTT ingestion service and any service-layer mutation (e.g. clearing a violation counter, applying a rule) push through the hub after committing to the DB — don't let the frontend find out via a stale poll.

### Auth (Entra ID + Google)

- Cookie authentication as the sign-in scheme; both `AddOpenIdConnect("Entra", ...)` (Microsoft Entra ID) and `AddGoogle` (`Microsoft.AspNetCore.Authentication.Google`) as external challenge schemes signing into that cookie. Entra deliberately uses plain `AddOpenIdConnect` pointed at `login.microsoftonline.com/{tenantId}/v2.0` rather than `Microsoft.Identity.Web`'s `AddMicrosoftIdentityWebApp` — that package's token-cache/MSAL machinery is for apps calling Graph or other downstream APIs, which this app never does; we only need the ID token's email/subject claims.
- Provider secrets (`Authentication:Google:ClientId/ClientSecret`, `Authentication:Entra:ClientId/ClientSecret/TenantId`) live in .NET user-secrets, never in `appsettings.*.json`.
- On first successful external login, upsert a row in our own `Users` table keyed by email/external subject id, defaulting to `Viewer`; an existing Admin promotes others via the `UserController` (no self-service admin signup).
- Authorize admin-only endpoints/actions with `[Authorize(Roles = "Admin")]`; read-only endpoints just need `[Authorize]`.

---

## Frontend: React, heavily modularized

Small, single-purpose files over big ones — optimize for "easy to find and hold in your head," not for fewest files. Mirror RMCD-App's decomposition exactly; don't flatten it.

```
hively.client/src/
  components/
    ui/            generic, app-agnostic building blocks (Button, Modal, Input, Select, Badge, ...) — one file each, index.ts barrel
    layout/        Header, Sidebar/NamespaceBrowser shell, Footer
    navigation/    NavLink, ProtectedRoute, ScrollToTop
    shared/        small app-aware but reusable pieces (StatCard, ComplianceBadge, TagPill, ...)
  pages/
    Topics/
      Topics.tsx              page shell only — composes the pieces below
      components/             TopicRow, HierarchyView, ListView, ...
      cards/                  (on TopicDetail) ProducerCard, ConsumerCard, SchemaCard, ComplianceCard, ActivityCard
      modals/                 ConfigureTopicModal, SchemaAssignModal, ProducerAssignModal
      hooks/                  useTopicFilters, useNamespaceDrilldown, ...
      contexts/               only if a subtree genuinely needs shared local state
      index.ts                `export { default } from './Topics'`
    Schemas/  Rules/  Producers/  Consumers/  Tags/  Graph/   (same shape as Topics/)
  services/
    apiService.ts              generic fetch wrapper (copy RMCD-App's: timeout, credentials:'include', JSON/error parsing) — every other service goes through this, never call fetch directly elsewhere
    topicService.ts  producerService.ts  consumerService.ts  schemaService.ts  tagService.ts  ruleService.ts  userService.ts
    signalr.ts                  connection factory for the SignalR hub client
  hooks/
    data/                       one hook per entity (useTopics, useTopic, useProducers, ...) — useState + useEffect + loading/error, calling the matching service, matching RMCD-App's useMachines.ts pattern exactly
    ui/                         cross-cutting UI hooks (useModalState, useFormState, ...)
  contexts/                     AuthContext (current user + role), ToastContext, SignalRContext (shared hub connection)
  types/
    api.ts                      shapes mirroring backend DTOs
    ui.ts                       frontend-only view types
    index.ts                    `export * from './api'; export * from './ui';`
  constants/                    apiEndpoints.ts, validation.ts
  utils/
  App.tsx  main.tsx
```

### Conventions

- **All backend calls go through `services/apiService.ts`.** Per-entity service files (`topicService.ts`, etc.) are the only callers of `apiRequest<T>()`; components/hooks never call it directly. This is the one required layer — everything else above is decomposition for readability.
- Data-fetching hooks (`hooks/data/*`) are the only things that call entity services from UI code. A page component calls a hook, not a service, directly.
- Routing via `react-router-dom`, mirroring RMCD-App's `ProtectedRoute` pattern for anything requiring auth, with an additional role check for Admin-only routes (Manage Schemas/Rules/Tags/Producers/Consumers panels).
- Styling: Tailwind CSS v4 (`@tailwindcss/vite`), matching RMCD-App's setup. Dark theme, OKLCH palette, IBM Plex Mono for topic paths/code/patterns/ids — see `Design/README.md` → Design Tokens for exact values; treat them as final, not placeholders.
- No icon library — see Decisions table above.
- A page's `index.ts` re-exports its default component (`export { default } from './Topics'`) so imports elsewhere stay short (`import Topics from '@/pages/Topics'`).
- `SearchableCombobox` (producer/consumer/schema pickers) is the one component explicitly called out in the design doc as worth extracting once and reusing — build it in `components/shared/` early since ~5+ screens need it (Configure modal, Rule form, Graph focal-entity search, etc.).

---

## Status

**Backend**: all six core entities have a full 3-layer CRUD slice, each built, run, and verified end-to-end against a real Postgres instance (not just compiled) — `Producer`, `Consumer`, `Tag`, `Schema` (with version history), `Rule` (with tag assignment), `Topic` (with schema compliance validation via `SchemaComplianceValidator`). Check `git log --oneline` for the exact commit per entity.

The rule-matching engine and relocation-relink heuristic are also built and verified end-to-end: pure functions `TopicPatternMatcher.MatchTopic`, `RuleMatcher.RuleSpecificity`/`FindMatchingRules`, and `RelinkHeuristic.FindRelinkCandidate` (`Hively.Server/Services/`), ported from the prototype's `matchTopic`/`ruleSpecificity`/`findMatchingRules`/`findRelinkCandidate`. Wired into `ITopicService`/`TopicController`: `GET /api/topic/{id}/matching-rules`, `POST /api/topic/{id}/apply-rule/{ruleId}`, `GET /api/topic/{id}/relink-candidate`, `POST /api/topic/{id}/accept-relink/{oldTopicId}`, plus `POST /api/rule/{id}/apply-to-all` for the bulk "Apply to N now" case.

Real auth (see Auth section above) is also built and verified end-to-end against real Google and Entra ID accounts, resolving through `IUserService.UpsertFromExternalLoginAsync` (`Users`/`UserIdentities` tables, matches the algorithm documented in `Design/schema.sql`). The very first login ever is auto-promoted to Admin; everyone after defaults to Viewer. `AuthController` (`login/google`, `login/entra`, `logout`, `me`) and `UserController` (Admin-only user list + role promotion) are the new endpoints. All six CRUD controllers now carry `[Authorize]` on reads and `[Authorize(Roles = "Admin")]` on mutations — verified that anonymous requests 401 across the board.

**Deliberately not built yet** — these depend on subsystems that don't exist yet, scoped out of the CRUD pass on purpose, not forgotten:
- MQTT ingestion (real broker connection, `MQTTnet` background hosted service, untracked-stub creation, violation counting, activity histogram rollup) — the rule-matching/relink logic above is ready for it to call into once it exists
- SignalR hub for live push updates (`TopicHub`) — reads/writes work, nothing pushes yet
- **Frontend — nothing built yet.** `hively.client/` is still the untouched default Vite/React template (`App.tsx`, `main.tsx`, default assets only). None of the structure described above (`components/`, `pages/`, `services/`, `hooks/`) exists on disk yet.

## Dev commands

- Backend: from repo root, `dotnet run --project Hively.Server` (needs `--project` if not already inside that directory) or via the `.slnx`/IDE run config — SpaProxy launches the Vite dev server automatically. Local dev API listens on `http://localhost:5012` (`Properties/launchSettings.json`).
- Frontend only: `npm run dev` from `hively.client/`.
- Local Postgres: `docker-compose.yml` at repo root (works with `podman compose up -d` or `docker compose up -d`); connection string already set in `appsettings.Development.json` to match its defaults (`localhost:5432`, db/user `hively`).
- Migrations: `dotnet ef migrations add <Name>` / `dotnet ef database update`, run from `Hively.Server/`.
- Lint: `npm run lint` (`oxlint`) from `hively.client/`.
