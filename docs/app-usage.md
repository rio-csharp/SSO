# App Usage

This document is for someone who wants to run and use the system without reading the code first.

## What You Will Open

After startup, there are three local applications:

- SSO website: https://localhost:5001
- Protected API: https://localhost:7061
- Sample client: https://localhost:7041

The normal usage path is:

1. Open the sample client.
2. Sign in through the SSO website.
3. Return to the sample client.
4. Use the sample client to call the protected API.

## Before You Start

Make sure all three applications are running:

- MySso.Web
- MySso.Api
- MySso.Sample.ClientWeb

If they are not running yet, use the startup steps in [README.md](../README.md).

Default development sign-in account:

- Email: admin@mysso.local
- Password: ChangeThis123!

## Using The Sample Client

Base URL:

- https://localhost:7041

What this app is for:

- it is the easiest way to test the login flow end to end
- it shows what the signed-in user received from the SSO server
- it can call the protected API with the current access token

### Step By Step

1. Open https://localhost:7041.
2. You should see the title `Sample OIDC Client`.
3. Click `Sign In With SSO`.
4. Your browser should move to the SSO login page at https://localhost:5001/account/login.
5. Sign in with `admin@mysso.local` and `ChangeThis123!`.
6. After a successful login, you should return to the sample client profile page.
7. On the profile page you should see:
	 - user claims
	 - an access token
	 - an ID token
	 - a refresh token
8. Click `Call Protected API`.
9. You should land on a page titled `Protected API Response`.
10. That page should show the JSON returned by the API.
11. Click `Logout` in the top navigation to sign out of the sample client.

### What Success Looks Like

You are using the client correctly if all of these are true:

- the first login starts from the sample client and redirects to the SSO site
- you return to the sample client's `Profile` page after login
- the `Profile` page shows token values
- the `Protected API Response` page shows JSON from the API
- after logout, opening the profile page again sends you back to sign in

### Main Client Pages

- https://localhost:7041/
	- home page
	- entry point for sign-in
- https://localhost:7041/Home/Profile
	- authenticated user profile
	- shows claims and token snapshot
- https://localhost:7041/Home/ApiData
	- shows the response from the protected API

## Using The Protected API

Base URL:

- https://localhost:7061

What this app is for:

- it is the resource server protected by the SSO-issued access token
- it proves that the access token from the sample client is accepted
- it proves that revoked sessions are denied even if the token itself still exists

### Health Endpoints

These do not require login:

- GET https://localhost:7061/health/live
- GET https://localhost:7061/health/ready

Use them to confirm the API process is alive and can reach its required dependencies.

### Protected Endpoint

- GET https://localhost:7061/api/profile/me

This endpoint requires a bearer access token from the SSO server.

### Easiest Way To Use The API

Use the sample client:

1. Sign in at https://localhost:7041.
2. Open the `Profile` page.
3. Click `Call Protected API`.
4. Read the JSON shown on the next page.

### Calling The API Yourself

If you want to call it manually, first sign in through the sample client and copy the access token shown on the profile page.

Then send a request like this:

```http
GET https://localhost:7061/api/profile/me
Authorization: Bearer {access_token}
```

Expected result:

- HTTP 200 when the token is valid and the user session is still active
- a JSON payload containing subject, name, email, and claim information

### Important Behavior

- this API only trusts access tokens issued by the local SSO server
- the API checks both the token and the linked server-side session
- if the session is revoked in the SSO system, the same token is rejected on the next API request

## Using The SSO Website During Testing

You asked for API and client usage, but in practice both depend on the SSO site, so these are the only pages you usually need:

- https://localhost:5001/account/login
	- sign-in page used by the sample client
- https://localhost:5001/sessions
	- shows your active sessions
	- lets you revoke your current session
- https://localhost:5001/admin
	- administrator area

Useful health endpoints:

- GET https://localhost:5001/health/live
- GET https://localhost:5001/health/ready

## Common Problems

### The browser does not open the local HTTPS sites

Cause:

- the local ASP.NET Core development certificate is not trusted

What to do:

- trust the local development certificate and restart the apps

### Login succeeds but the API page fails

Cause:

- MySso.Api is not running
- the API cannot reach PostgreSQL
- the session was revoked

What to do:

- open https://localhost:7061/health/live
- open https://localhost:7061/health/ready
- sign in again if the session was revoked

### The client redirects to login again after you were already signed in

Cause:

- the local client cookie was cleared
- you logged out
- the SSO session was revoked

What to do:

- sign in again from the sample client home page

### The SSO site or API does not start

Cause:

- PostgreSQL is not running
- the connection string is wrong
- required development environment variables were not set

What to do:

- go back to [README.md](../README.md)
- re-run the startup commands exactly as shown