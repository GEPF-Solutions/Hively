# Running Hively.Server in a container

The published image (built from the root `Dockerfile`) takes all configuration via
environment variables — there is no `appsettings.Production.json` checked in, and
user-secrets (used for local dev) don't exist inside the container.

.NET config binds `Section:Key` from `appsettings.json` to env vars by replacing `:`
with a double underscore, e.g. `Authentication:Google:ClientId` becomes
`Authentication__Google__ClientId`. All variables below follow that convention.

## Required

| Variable | Purpose |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | Set to `Production` (or anything other than `Development`) in real deployments — dev-only behavior (EF sensitive data logging, HTTP logging, SpaProxy) is gated on `IsDevelopment()` and must stay off outside local dev. |
| `ConnectionStrings__DefaultConnection` | Postgres connection string, e.g. `Host=<host>;Port=5432;Database=hively;Username=<user>;Password=<password>`. Points at whatever Postgres the container/cluster provides — not the same instance as local `docker-compose.yml`. |
| `Authentication__Google__ClientId` | Google OAuth client ID (Google Cloud Console credentials). |
| `Authentication__Google__ClientSecret` | Google OAuth client secret. Treat as a secret. |
| `Authentication__Entra__ClientId` | Entra ID (Azure AD) app registration client ID. |
| `Authentication__Entra__ClientSecret` | Entra ID app registration client secret. Treat as a secret. |
| `Authentication__Entra__TenantId` | Entra tenant ID — pins `AddOpenIdConnect`'s authority to a single org tenant, restricting Microsoft login to that org (see `Program.cs`). |
| `MqttBroker__Host` | Hostname/IP of the MQTT broker the ingestion service connects to. |
| `MqttBroker__Port` | Broker port (typically `1883` plain, `8883` TLS). |
| `MqttBroker__ClientId` | MQTT client ID the ingestion service connects as. |
| `MqttBroker__UseTls` | `true`/`false` — whether to connect over TLS. |

## Optional

| Variable | Purpose |
|---|---|
| `MqttBroker__Username` / `MqttBroker__Password` | Only needed if the broker requires auth (anonymous access is fine for a local/dev broker). Treat the password as a secret. |

## Ports

The Dockerfile's base image exposes `8080` (HTTP) and `8081` (HTTPS). The
`Container (Dockerfile)` launch profile sets:

- `ASPNETCORE_HTTP_PORTS=8080`
- `ASPNETCORE_HTTPS_PORTS=8081`

In most real deployments (OpenShift routes, a reverse proxy, an ingress controller)
TLS is terminated in front of the container, so only `ASPNETCORE_HTTP_PORTS=8080`
is typically needed — drop the HTTPS port/cert setup unless the container itself
must terminate TLS.

## Not required / notes

- **Google login has no org restriction today** — any Google account can sign in
  and gets auto-created as `Viewer`. Only Entra is tenant-restricted. If that
  needs to change, it's a code change (`hd` claim check), not an env var.
- `.env.example` / `docker-compose.yml` at the repo root only configure the local
  Postgres container for dev — per the comment in `.env.example`, none of that is
  used in OpenShift, where config comes from cluster Secrets/ConfigMaps instead.
  The table above is what those Secrets/ConfigMaps need to populate.
- No CORS configuration exists or is needed — the SPA is published into the same
  host/origin as the API (`hively.client` via the `.esproj`), so there's no
  cross-origin call to allow.
