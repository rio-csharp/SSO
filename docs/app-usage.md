# App Usage

This document is not about starting the three projects in this repository.

This document is about how to use this SSO system as the login center for your own software:

- your own MVC or Razor client
- your own server-side web application
- your own protected API

The goal is that you, or an AI writing code for you, can read only this file and know how to integrate a new application with this SSO system.

## What This SSO System Gives You

This SSO project acts as:

- the login website for your users
- the OpenID Connect authorization server for your client applications
- the OAuth2 token issuer for your protected APIs
- the central session store for logout and session revocation

In practice, your own applications do not need to implement username and password login anymore.

Your application should:

- redirect users to this SSO system for sign-in
- receive tokens from this SSO system
- use the access token to call your protected APIs
- validate bearer tokens in your APIs against this SSO system

## The URLs You Need To Know

Default local URLs in this repository:

- SSO host: https://localhost:5001
- sample protected API: https://localhost:7061

Important SSO endpoints:

- authorize endpoint: https://localhost:5001/connect/authorize
- token endpoint: https://localhost:5001/connect/token
- logout endpoint: https://localhost:5001/connect/logout
- userinfo endpoint: https://localhost:5001/connect/userinfo

## The Integration Model

There are three roles:

1. SSO server
	 - this repository
	 - handles login and token issuance
2. Your client application
	 - the web application your users actually open
	 - redirects to the SSO server for login
3. Your protected API
	 - the backend API your client calls with a bearer token

Typical flow:

1. User opens your client application.
2. Your client application redirects the user to this SSO server.
3. User signs in on the SSO server.
4. SSO server returns an authorization code to your client.
5. Your client exchanges the code for tokens.
6. Your client stores the tokens for the logged-in session.
7. Your client calls your protected API with the access token.
8. Your protected API validates the token against the SSO server.

## Step 1: Register Your Client In The SSO Admin UI

Before writing your own client code, register your application in the SSO admin UI.

Open:

- https://localhost:5001/admin

Create a client with these fields:

- ClientId
	- a unique identifier for your application
	- example: `my-crm-web`
- DisplayName
	- human-readable name
	- example: `My CRM Web`
- RedirectUri
	- the callback URL in your client application
	- example: `https://localhost:7201/signin-oidc`
- AllowedScopes
	- scopes your client will request
	- example: `openid profile email api offline_access`
- ClientSecret
	- required for confidential clients
	- example: `super-secret-value`
- PostLogoutRedirectUri
	- where the user should return after logout
	- example: `https://localhost:7201/signout-callback-oidc`
- ClientType
	- use `Confidential` for a server-side MVC or Razor app
- AllowRefreshTokens
	- enable if your client should receive refresh tokens
- RequireConsent
	- usually keep off for first-party internal apps unless you want a consent screen later

If your client is similar to the sample client in this repository, use:

- `openid`
- `profile`
- `email`
- `api`
- `offline_access`

## Step 2: Write Your Own Client Application

The easiest integration is a server-side ASP.NET Core MVC or Razor application using:

- cookie authentication for your local app session
- OpenID Connect for sign-in against this SSO server

### Minimal Program.cs For Your Client

Use code like this in your own client application:

```csharp
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddAuthentication(options =>
		{
				options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
		})
		.AddCookie()
		.AddOpenIdConnect(options =>
		{
				builder.Configuration.GetSection("Authentication:OpenIdConnect").Bind(options);

				options.ResponseType = OpenIdConnectResponseType.Code;
				options.UsePkce = true;
				options.SaveTokens = true;
				options.GetClaimsFromUserInfoEndpoint = true;
				options.MapInboundClaims = false;

				options.Scope.Clear();
				options.Scope.Add("openid");
				options.Scope.Add("profile");
				options.Scope.Add("email");
				options.Scope.Add("offline_access");
				options.Scope.Add("api");
		});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();
```

### appsettings For Your Client

Use configuration like this in your own client app:

```json
{
	"Authentication": {
		"OpenIdConnect": {
			"Authority": "https://localhost:5001",
			"ClientId": "my-crm-web",
			"ClientSecret": "super-secret-value",
			"CallbackPath": "/signin-oidc",
			"SignedOutCallbackPath": "/signout-callback-oidc",
			"SignedOutRedirectUri": "https://localhost:7201/signout-callback-oidc"
		}
	},
	"Api": {
		"BaseUrl": "https://localhost:7301/"
	}
}
```

### Login And Logout Endpoints In Your Client

Your client usually needs a login action and a logout action.

Example controller:

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public sealed class AccountController : Controller
{
		[AllowAnonymous]
		[HttpPost("account/login")]
		[ValidateAntiForgeryToken]
		public IActionResult Login(string? returnUrl = null)
		{
				return Challenge(new AuthenticationProperties
				{
						RedirectUri = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl
				}, OpenIdConnectDefaults.AuthenticationScheme);
		}

		[HttpPost("account/logout")]
		[ValidateAntiForgeryToken]
		public IActionResult Logout()
		{
				return SignOut(new AuthenticationProperties
				{
						RedirectUri = "/"
				}, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
		}
}
```

### Reading Tokens In Your Client

After login, you can read the issued tokens from the current user session.

Example:

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public sealed class ProfileController : Controller
{
		[HttpGet("profile")]
		public async Task<IActionResult> Index()
		{
				var accessToken = await HttpContext.GetTokenAsync("access_token");
				var idToken = await HttpContext.GetTokenAsync("id_token");
				var refreshToken = await HttpContext.GetTokenAsync("refresh_token");

				return Ok(new
				{
						accessToken,
						idToken,
						refreshToken
				});
		}
}
```

### Calling Your API From Your Client

The normal pattern is:

1. read `access_token` from the current authenticated session
2. send it as a bearer token to your own API

Example:

```csharp
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public sealed class OrdersController : Controller
{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;

		public OrdersController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
		{
				_httpClientFactory = httpClientFactory;
				_configuration = configuration;
		}

		[HttpGet("orders")]
		public async Task<IActionResult> Index(CancellationToken cancellationToken)
		{
				var accessToken = await HttpContext.GetTokenAsync("access_token");
				if (string.IsNullOrWhiteSpace(accessToken))
				{
						return Unauthorized("No access token is available.");
				}

				var client = _httpClientFactory.CreateClient();
				client.BaseAddress = new Uri(_configuration["Api:BaseUrl"]!);
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

				var response = await client.GetAsync("api/orders", cancellationToken);
				var body = await response.Content.ReadAsStringAsync(cancellationToken);

				return Content(body, "application/json");
		}
}
```

## Step 3: Write Your Own Protected API

Your API must trust tokens issued by this SSO server.

At minimum, your API should:

- use JWT bearer authentication
- set `Authority` to this SSO host
- set `Audience` to `resource_api`
- require the `api` scope in the client

### Minimal Program.cs For Your API

Use code like this in your own protected API:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
		.AddJwtBearer(options =>
		{
				options.Authority = builder.Configuration["Authentication:Authority"];
				options.Audience = "resource_api";
				options.RequireHttpsMetadata = true;
				options.MapInboundClaims = false;
				options.TokenValidationParameters.NameClaimType = "name";
				options.TokenValidationParameters.RoleClaimType = "role";
		});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### appsettings For Your API

Use configuration like this:

```json
{
	"Authentication": {
		"Authority": "https://localhost:5001"
	}
}
```

### Example Protected Controller

Example:

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
		[Authorize]
		[HttpGet]
		public IActionResult GetMine()
		{
				var subject = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
				var email = User.FindFirstValue("email") ?? User.FindFirstValue(ClaimTypes.Email);

				return Ok(new
				{
						subject,
						email,
						orders = new[]
						{
								new { id = 1001, status = "Paid" },
								new { id = 1002, status = "Pending" }
						}
				});
		}
}
```

## Step 4: If You Want Session Revocation To Matter Immediately

There are two levels of integration for your API.

### Level 1: Standard Token Validation

If you only do JWT bearer validation against the issuer, your API will trust valid tokens until token expiry.

This is the simplest integration and is enough for many apps.

### Level 2: Match This Repository's Revocation Behavior

This repository does something stronger in its sample API:

- it reads the `sid` claim from the access token
- it checks whether that server-side session is still active
- if the session was revoked in the SSO system, the API rejects the token immediately

If you want that exact behavior in your own API, the easiest path is:

1. keep your API in the same solution or deployment boundary as this SSO system
2. reuse the same infrastructure registration and session lifecycle service
3. add the same `OnTokenValidated` check that the sample API uses

Example:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MySso.Application.Common.Interfaces;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
		.AddJwtBearer(options =>
		{
				options.Authority = "https://localhost:5001";
				options.Audience = "resource_api";
				options.MapInboundClaims = false;

				options.Events = new JwtBearerEvents
				{
						OnTokenValidated = async context =>
						{
								var sessionClaim = context.Principal?.FindFirst("sid")?.Value;
								if (!Guid.TryParse(sessionClaim, out var sessionId))
								{
										context.Fail("The access token does not contain a valid session identifier.");
										return;
								}

								var sessionLifecycleService = context.HttpContext.RequestServices.GetRequiredService<ISessionLifecycleService>();
								if (!await sessionLifecycleService.IsSessionActiveAsync(sessionId, context.HttpContext.RequestAborted))
								{
										context.Fail("The user session is no longer active.");
								}
						}
				};
		});
```

If your API is a completely separate product and does not share this repository's infrastructure, write the basic JWT integration first, then add a dedicated internal session-validation mechanism later.

## Step 5: What Your AI Should Know When Writing New Apps

If you are using AI to generate a new client or API that integrates with this SSO project, the AI should assume the following:

- the SSO authority is `https://localhost:5001`
- interactive clients should use OpenID Connect authorization code flow with PKCE
- client applications should request scopes like `openid profile email api offline_access`
- protected APIs should validate bearer tokens with audience `resource_api`
- client apps should store tokens in the authenticated server-side session
- if immediate revoke behavior is required, APIs must validate the `sid` claim against the server-side session store

## Minimal Copy-Paste Checklists

### For A New Client App

1. Register the client in `https://localhost:5001/admin`.
2. Set `Authority` to `https://localhost:5001`.
3. Set `ClientId`, `ClientSecret`, `CallbackPath`, and `SignedOutCallbackPath`.
4. Use cookie auth plus OpenID Connect.
5. Request `openid profile email api offline_access`.
6. Save tokens.
7. Use the access token when calling your API.

### For A New API

1. Add JWT bearer authentication.
2. Set `Authority` to `https://localhost:5001`.
3. Set `Audience` to `resource_api`.
4. Set `MapInboundClaims = false`.
5. Protect controllers with `[Authorize]`.
6. If you need immediate revocation, also validate the `sid` claim against the session store.

## Common Problems

### The client redirects to the SSO site but never comes back

Usually one of these is wrong:

- the registered `RedirectUri`
- the client's `CallbackPath`
- the `ClientId`
- the `ClientSecret`

Check that the values in your client code exactly match the values registered in the SSO admin UI.

### The API returns 401 even though login worked

Usually one of these is wrong:

- your API `Authority`
- your API `Audience`
- the client did not request the `api` scope
- the access token was missing or not sent as `Bearer`
- the SSO session was revoked

### You see HTTP 429 Too Many Requests

This SSO host and API now include rate limiting.

If your code is retrying too aggressively:

- slow down repeated login attempts
- avoid rapid refresh loops on `/account/login`
- avoid tight polling loops against protected API endpoints

### You want to use this in another product later

That is exactly what this document is for.

The shortest path is:

1. register a new client in this SSO system
2. copy the client `Program.cs` pattern from this document
3. copy the API `Program.cs` pattern from this document
4. replace the URLs, client id, client secret, callback paths, and your own domain endpoints