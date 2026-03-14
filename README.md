# MySso

MySso is a .NET 10 multi-project single sign-on solution built around ASP.NET Core Identity, OpenIddict, Entity Framework Core, and PostgreSQL. The repository combines an SSO web host, an OpenID Connect / OAuth 2.0 authorization server, a protected API, a sample MVC client, and supporting domain/application/infrastructure layers in one solution.

The codebase is organized for two goals:

- provide a working local SSO reference implementation
- keep domain rules, use cases, framework code, and host-specific concerns separated enough for continued evolution

## What This Repository Contains

At the time of writing, this repository includes:

- local username/password sign-in with ASP.NET Core Identity
- an OpenIddict-based authorization server with authorization, token, logout, userinfo, revocation, and introspection endpoints
- OpenID Connect authorization code flow with PKCE and refresh tokens
- a protected resource API that accepts bearer tokens from the SSO issuer
- persisted user sessions with revocation checks enforced by the API through the sid claim
- an administrator dashboard for users, roles, client records, sessions, and audit logs
- a user-facing profile page and self-service session listing/revocation page
- a sample MVC client that signs in through MySso and calls the protected API

## Why This Project Exists

This repository is useful if you want to study or extend a layered .NET SSO solution that brings together:

- interactive user authentication
- OpenID Connect token issuance
- protected API access
- back-office administration
- session lifecycle management
- audit logging

It is not currently presented as a production-ready platform package. Several production concerns are intentionally or currently left open and are called out later in this document.

## Core Features

- ASP.NET Core Identity local account authentication for the SSO host
- OpenIddict server integration for OpenID Connect and OAuth 2.0 flows
- authorization code flow with PKCE enabled by default
- refresh token flow enabled by default
- PostgreSQL persistence through Entity Framework Core migrations
- development bootstrap for a default administrator account and a sample confidential OIDC client
- administrator-only management surface for users, roles, client records, sessions, and audit activity
- session revocation that affects subsequent API bearer token validation
- sample MVC application that demonstrates sign-in, token storage, and API calls
- unit and integration tests across domain, application, and infrastructure boundaries

## Technology Stack

- .NET SDK 10.0.200
- ASP.NET Core MVC
- ASP.NET Core Identity
- OpenIddict 7.3.0
- Entity Framework Core 10
- Npgsql / PostgreSQL
- xUnit

## Architecture Overview

The solution follows a layered structure:

1. MySso.Domain: domain entities, value objects, enums, and invariants.
2. MySso.Contracts: shared DTOs, result models, and pagination contracts.
3. MySso.Application: use-case handlers and abstraction interfaces for persistence, identity provisioning, current-user access, queries, and session lifecycle.
4. MySso.Infrastructure: EF Core, PostgreSQL, Identity, OpenIddict, repository implementations, bootstrap seeding, and infrastructure services.
5. MySso.Web: the SSO web application, local login UI, OIDC endpoints, user pages, and administrator dashboard.
6. MySso.Api: a protected API that validates access tokens issued by MySso.
7. MySso.Sample.ClientWeb: a sample confidential MVC client using OpenID Connect code flow with PKCE.

Runtime relationship:

```text
Browser
  -> MySso.Sample.ClientWeb
       -> redirects to MySso.Web for interactive sign-in
       -> receives tokens from MySso.Web
       -> calls MySso.Api with bearer access token

MySso.Web
  -> uses ASP.NET Core Identity for local account authentication
  -> uses OpenIddict for token issuance and userinfo/logout flows
  -> stores data in PostgreSQL through MySso.Infrastructure

MySso.Api
  -> validates JWT bearer tokens against the configured issuer
  -> checks sid against persisted user sessions before allowing access
```

## Solution Structure

```text
src/
  MySso.Domain/          Domain entities, value objects, and rules
  MySso.Contracts/       DTOs, result models, and pagination contracts
  MySso.Application/     Use cases and application interfaces
  MySso.Infrastructure/  EF Core, PostgreSQL, Identity, OpenIddict, services
  MySso.Web/             SSO UI, OIDC endpoints, admin UI, profile, sessions
  MySso.Api/             Protected resource API
samples/
  MySso.Sample.ClientWeb/ Sample MVC client using OpenID Connect
tests/
  MySso.Domain.Tests/       Domain behavior tests
  MySso.Application.Tests/  Application handler tests
  MySso.IntegrationTests/   Infrastructure and registration tests
```

## Module Responsibilities

### src/MySso.Domain

- owns core concepts such as registered clients, domain users, roles, sessions, and audit logs
- enforces rules such as email normalization, absolute redirect URIs, PKCE requirement, and one-time session revocation semantics

### src/MySso.Contracts

- exposes boundary-safe models such as user summaries, role summaries, client summaries, audit entries, and paginated results

### src/MySso.Application

- defines the application boundary through interfaces such as repositories, current-user context, unit of work, query services, and session lifecycle
- contains handlers for creating local users, creating roles, registering clients, and revoking user sessions

### src/MySso.Infrastructure

- implements repositories, EF Core DbContext, Identity integration, OpenIddict integration, bootstrap seeding, and query services
- wires PostgreSQL, OpenIddict, Identity, current-user access, and infrastructure services through dependency injection

### src/MySso.Web

- hosts MVC pages and controllers for:
  - account login and logout
  - OIDC authorization, logout, and userinfo endpoints
  - administrator dashboard
  - current-user profile
  - current-user session management

### src/MySso.Api

- exposes GET /api/profile/me
- validates bearer tokens against the configured issuer
- rejects tokens whose sid claim does not map to an active persisted session

### samples/MySso.Sample.ClientWeb

- demonstrates OpenID Connect code flow with PKCE
- stores tokens in the signed-in session
- calls the protected API with the issued access token

## Environment Requirements

- .NET SDK 10.0.200, as pinned in global.json
- a reachable PostgreSQL instance
- HTTPS development certificates trusted locally

Assumption: the local ASP.NET Core development certificate has already been installed and trusted on the machine running the sample.

## Quick Start

### 1. Configure the database connection

Both MySso.Web and MySso.Api read the PostgreSQL connection string from ConnectionStrings:PostgreSql.

Default values in the repository:

```json
"ConnectionStrings": {
  "PostgreSql": "Host=localhost;Port=5432;Database=mysso;Username=postgres;Password=postgres"
}
```

PowerShell example override:

```powershell
$env:ConnectionStrings__PostgreSql = "Host=localhost;Port=5432;Database=mysso;Username=postgres;Password=postgres"
```

### 2. Apply database migrations

Run from the repository root:

```powershell
$env:ConnectionStrings__PostgreSql = "Host=localhost;Port=5432;Database=mysso;Username=postgres;Password=postgres"
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update --project src/MySso.Infrastructure/MySso.Infrastructure.csproj --startup-project src/MySso.Web/MySso.Web.csproj
```

Notes:

- migrations live in src/MySso.Infrastructure/Persistence/Migrations
- the design-time DbContext factory reads appsettings files and environment variables
- in Development, MySso.Web also checks for pending migrations during startup and applies them if needed

### 3. Start the SSO web host

```powershell
$env:ConnectionStrings__PostgreSql = "Host=localhost;Port=5432;Database=mysso;Username=postgres;Password=postgres"
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src/MySso.Web/MySso.Web.csproj --launch-profile https
```

Default URLs:

- https://localhost:5001
- http://localhost:5000

In Development, startup bootstrap ensures:

- an Administrator role exists
- a default administrator account exists
- a sample OpenIddict client exists

Default development bootstrap values from src/MySso.Web/appsettings.Development.json:

- Admin email: admin@mysso.local
- Admin password: ChangeThis123!
- Sample client id: sample-client-web
- Sample client secret: sample-client-secret
- Sample client redirect URI: https://localhost:7041/signin-oidc
- Sample client post-logout redirect URI: https://localhost:7041/signout-callback-oidc

### 4. Start the protected API

```powershell
$env:ConnectionStrings__PostgreSql = "Host=localhost;Port=5432;Database=mysso;Username=postgres;Password=postgres"
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src/MySso.Api/MySso.Api.csproj --launch-profile https
```

Default URLs:

- https://localhost:7061
- http://localhost:5061

### 5. Start the sample MVC client

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project samples/MySso.Sample.ClientWeb/MySso.Sample.ClientWeb.csproj --launch-profile https
```

Default URLs:

- https://localhost:7041
- http://localhost:5041

### 6. Verify the local flow

1. Open https://localhost:7041.
2. Trigger sign-in from the sample client.
3. Authenticate using the development administrator credentials.
4. Return to the sample client profile page.
5. Open the sample API page and confirm the protected API response is displayed.
6. Open https://localhost:5001/admin and confirm the administrator dashboard loads.

## Runtime Endpoints and Hosts

### MySso.Web

- /account/login
- /account/logout
- /connect/authorize
- /connect/token
- /connect/logout
- /connect/userinfo
- /connect/revoke
- /connect/introspect
- /admin
- /profile
- /sessions

### MySso.Api

- /
- /api/profile/me

### MySso.Sample.ClientWeb

- /
- /Home/Profile
- /Home/ApiData

## Configuration

### MySso host settings

Configured in src/MySso.Web/appsettings.json and src/MySso.Api/appsettings.json:

- Issuer: authority URL used by token issuance and API bearer validation
- CookieName: SSO application cookie name
- RequireHttps: toggles HTTPS metadata enforcement for bearer validation
- AccessTokenLifetimeMinutes: access token lifetime
- RefreshTokenLifetimeDays: refresh token lifetime

### Bootstrap settings

Configured in src/MySso.Web/appsettings.Development.json and validated on startup in Development:

- administrator email and password
- administrator given/family name
- sample client id and secret
- sample client redirect URI
- sample client post-logout redirect URI

### Sample client settings

Configured in samples/MySso.Sample.ClientWeb/appsettings.Development.json:

- Authentication:OpenIdConnect:Authority
- Authentication:OpenIdConnect:ClientId
- Authentication:OpenIdConnect:ClientSecret
- Authentication:OpenIdConnect:CallbackPath
- Authentication:OpenIdConnect:SignedOutCallbackPath
- Api:BaseUrl

## Database and Migrations

The repository currently uses one EF Core DbContext, MySsoDbContext, to store:

- domain entities
- ASP.NET Core Identity entities
- OpenIddict entities

This means MySso currently runs as a single logical data store rather than splitting identity, authorization server, and business data into separate databases.

Migration command target:

- project: src/MySso.Infrastructure/MySso.Infrastructure.csproj
- startup project: src/MySso.Web/MySso.Web.csproj

Assumption: for production deployments, maintainers may prefer a dedicated migration pipeline instead of relying on Development bootstrap behavior.

## Authentication and Authorization Flow

### Interactive login and token issuance

1. The sample MVC client challenges the user with OpenID Connect.
2. The browser is redirected to MySso.Web.
3. MySso.Web authenticates the user through ASP.NET Core Identity cookies.
4. The authorization endpoint issues tokens for the requested scopes.
5. During issuance, MySso creates a persisted interactive session and adds its identifier as the sid claim.
6. The sample client stores the issued tokens for subsequent requests.

### Protected API access

1. The sample client sends the access token to MySso.Api.
2. MySso.Api validates the JWT against the configured issuer.
3. The API extracts the sid claim.
4. The API asks the session lifecycle service whether that session is still active.
5. If the session was revoked, the request is rejected even if the token is otherwise valid.

### Session revocation

- administrators can revoke sessions from the administrator dashboard
- authenticated users can revoke their own sessions from the sessions page
- revocation writes an audit log entry and affects future sid-based API validation

## Administration and User Center

### Administrator dashboard

The administrator-only MVC surface currently supports:

- viewing users
- viewing roles
- viewing registered client records
- viewing user sessions
- viewing audit logs
- creating local users
- creating roles
- registering client records
- revoking user sessions

### User-facing pages

Authenticated end users currently have:

- a profile page
- a session list page
- self-service session revocation

## Sample Client

The sample MVC application is intended to validate the basic SSO path end to end:

- redirect to the authorization server
- local sign-in at MySso.Web
- return to the client with tokens
- inspect stored tokens on the profile page
- call the protected API with the access token

## Testing

Run all tests from the repository root:

```powershell
dotnet test SSO.slnx
```

The repository contains three test projects:

- MySso.Domain.Tests
- MySso.Application.Tests
- MySso.IntegrationTests

Current test focus includes:

- email normalization and validation
- registered client invariants such as PKCE and absolute redirect URIs
- session revocation behavior
- application handlers for users and sessions
- infrastructure registration and query/session services

Assumption: if local host processes are still running from manual testing, stopping them before dotnet test may avoid file-lock issues during development.

## Development and Extension Notes

- package versions are centrally managed in Directory.Packages.props
- nullable reference types and implicit usings are enabled across the solution
- MySso.Application depends on MySso.Domain and MySso.Contracts
- MySso.Infrastructure is the layer that depends on EF Core, Npgsql, Identity, and OpenIddict
- MySso.Web and MySso.Api compose the application by calling AddInfrastructure(configuration)

Suggested extension approach:

1. put new domain rules in MySso.Domain
2. add or update use cases and ports in MySso.Application
3. implement adapters in MySso.Infrastructure
4. surface the capability from MySso.Web, MySso.Api, or another host project
5. add tests in the corresponding test project

## Known Limitations and TODO

- OpenIddict signing and encryption keys are ephemeral in the current codebase.
  - This is suitable for local development but not a complete production key-management strategy.
- The administrator Create Client flow writes to the domain-side registered client catalog through RegisterClientHandler.
  - It does not, by itself, create an OpenIddict application record for live protocol use.
  - The sample OpenIddict client is seeded separately during Development bootstrap.
- The repository does not currently include top-level deployment guidance, containerization, or CI/CD configuration.
  - TODO: maintainers should document the intended deployment and delivery model.
- No repository-level CONTRIBUTING.md is present.
  - Assumption: the guidance in this README is sufficient until contribution volume grows.
- No top-level license file is present.
  - TODO: maintainers should add the intended open source license.

## Roadmap

TODO: no roadmap document or published milestone plan is currently present in the repository.

If maintainers want to expose roadmap information publicly, they should add one or more of the following:

- planned features
- production hardening goals
- deployment targets
- release milestones

## Contributing

Contributions should preserve the current architectural boundaries:

- keep domain rules in MySso.Domain
- keep use-case orchestration and interfaces in MySso.Application
- keep framework-specific and persistence-specific code in MySso.Infrastructure
- keep UI or transport concerns in MySso.Web, MySso.Api, or sample applications

Recommended local validation flow:

1. make the smallest coherent change that solves the target problem
2. add or update tests with the change
3. run dotnet build SSO.slnx
4. run dotnet test SSO.slnx
5. validate the affected runtime flow locally when the change touches authentication, sessions, or OIDC behavior

TODO: maintainers may want to add a dedicated CONTRIBUTING.md if the project is opened to broader community participation.

## License

TODO: license information is not currently present at the repository root. Maintainers should add the intended license before publishing broadly.
