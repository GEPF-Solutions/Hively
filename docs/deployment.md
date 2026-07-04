# Running Hively.Server in a container

The published image (built from the root `Dockerfile`) takes all configuration via
environment variables — there is no `appsettings.Production.json` checked in, and
user-secrets (used for local dev) don't exist inside the container.

.NET config binds `Section:Key` from `appsettings.json` to env vars by replacing `:`
with a double underscore, e.g. `Authentication:Google:ClientId` becomes
`Authentication__Google__ClientId`. All variables below follow that convention.

## Image

Published to GitHub Container Registry: `ghcr.io/gepf-solutions/hively`. Pull `:latest`
for the newest build, or a specific tag if one's been cut.

```bash
docker run -d \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Host=<host>;Port=5432;Database=hively;Username=<user>;Password=<password>" \
  -e Authentication__Google__Enabled=true \
  -e Authentication__Google__ClientId=<id> \
  -e Authentication__Google__ClientSecret=<secret> \
  -e Authentication__Entra__Enabled=true \
  -e Authentication__Entra__ClientId=<id> \
  -e Authentication__Entra__ClientSecret=<secret> \
  -e Authentication__Entra__TenantId=<tenant> \
  -e MqttBroker__Host=<host> \
  -e MqttBroker__Port=1883 \
  -e MqttBroker__ClientId=hively-ingestion \
  -e MqttBroker__UseTls=false \
  ghcr.io/gepf-solutions/hively:latest
```

Needs a reachable Postgres (schema applied from `Design/schema.sql` — the image
doesn't run migrations, since the project doesn't use EF migrations at all; see the
root `CLAUDE.md`) and a reachable MQTT broker. See below for the full variable list
and what's optional.

## Required

| Variable | Purpose |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | Set to `Production` (or anything other than `Development`) in real deployments — dev-only behavior (EF sensitive data logging, HTTP logging, SpaProxy) is gated on `IsDevelopment()` and must stay off outside local dev. |
| `ConnectionStrings__DefaultConnection` | Postgres connection string, e.g. `Host=<host>;Port=5432;Database=hively;Username=<user>;Password=<password>`. Points at whatever Postgres the container/cluster provides — not the same instance as local `docker-compose.yml`. |
| `MqttBroker__Host` | Hostname/IP of the MQTT broker the ingestion service connects to. |
| `MqttBroker__Port` | Broker port (typically `1883` plain, `8883` TLS). |
| `MqttBroker__ClientId` | MQTT client ID the ingestion service connects as. |
| `MqttBroker__UseTls` | `true`/`false` — whether to connect over TLS. |

At least one of the two providers below needs `Enabled=true`, or there's no way to log in at all — `AuthController.Providers` (`GET /api/auth/providers`) is what the frontend's login page queries to decide which buttons to show.

| Variable | Purpose |
|---|---|
| `Authentication__Google__Enabled` | `true`/`false` (default `false`) — registers Google's OAuth handler at all. Neither of the two rows below matter unless this is `true`. |
| `Authentication__Google__ClientId` | Required if Google is enabled. Google OAuth client ID (Google Cloud Console credentials). |
| `Authentication__Google__ClientSecret` | Required if Google is enabled. Google OAuth client secret. Treat as a secret. |
| `Authentication__Entra__Enabled` | `true`/`false` (default `false`) — registers Entra's OIDC handler at all. None of the three rows below matter unless this is `true`. |
| `Authentication__Entra__ClientId` | Required if Entra is enabled. Entra ID (Azure AD) app registration client ID. |
| `Authentication__Entra__ClientSecret` | Required if Entra is enabled. Entra ID app registration client secret. Treat as a secret. |
| `Authentication__Entra__TenantId` | Required if Entra is enabled. Entra tenant ID — pins `AddOpenIdConnect`'s authority to a single org tenant, restricting Microsoft login to that org (see `Program.cs`). |

## Optional

| Variable | Purpose |
|---|---|
| `MqttBroker__Username` / `MqttBroker__Password` | Only needed if the broker requires auth (anonymous access is fine for a local/dev broker). Treat the password as a secret. |
| `Authentication__Google__AllowedHostedDomain` | Only relevant if Google is enabled. Restricts Google login to a single Google Workspace domain (e.g. `yourcompany.com`) — checks the `hd` claim server-side in `OnCreatingTicket`, same idea as `Authentication__Entra__TenantId`. Leave unset to allow any Google account, including personal `@gmail.com` ones. |

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

- `.env.example` / `docker-compose.yml` at the repo root only configure the local
  Postgres container for dev — per the comment in `.env.example`, none of that is
  used in OpenShift, where config comes from cluster Secrets/ConfigMaps instead.
  The table above is what those Secrets/ConfigMaps need to populate.
- No CORS configuration exists or is needed — the SPA is published into the same
  host/origin as the API (`hively.client` via the `.esproj`), so there's no
  cross-origin call to allow.
