using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace MySso.E2ETests;

[CollectionDefinition(nameof(E2ECollection), DisableParallelization = true)]
public sealed class E2ECollection : ICollectionFixture<E2ETestHostFixture>
{
}

[Collection(nameof(E2ECollection))]
public sealed class OidcFlowE2ETests
{
    private readonly E2ETestHostFixture _fixture;

    public OidcFlowE2ETests(E2ETestHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AuthorizationCodeFlow_Login_Api_And_Logout_Work_EndToEnd()
    {
        var session = await SignInThroughSampleClientAsync();
        var profilePage = await session.RedirectingBrowser.GetAsync(new Uri(_fixture.ClientBaseUri, "Home/Profile"));
        var profileHtml = await profilePage.Content.ReadAsStringAsync();
        Assert.Contains("Authenticated Profile", profileHtml, StringComparison.Ordinal);
        Assert.Contains("Access token:", profileHtml, StringComparison.Ordinal);

        var apiPage = await session.RedirectingBrowser.GetAsync(new Uri(_fixture.ClientBaseUri, "Home/ApiData"));
        var apiHtml = await apiPage.Content.ReadAsStringAsync();
        Assert.Contains("&quot;subject&quot;", apiHtml, StringComparison.Ordinal);
        Assert.Contains("admin@mysso.local", apiHtml, StringComparison.Ordinal);

        var logoutToken = E2ETestHostFixture.ExtractHiddenInputValue(profileHtml, "__RequestVerificationToken");
        var logoutResponse = await session.Browser.PostAsync(new Uri(_fixture.ClientBaseUri, "Home/Logout"),
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = logoutToken
            }));

        Assert.Equal(HttpStatusCode.Found, logoutResponse.StatusCode);

        var postLogoutProfile = await session.Browser.GetAsync(new Uri(_fixture.ClientBaseUri, "Home/Profile"));
        Assert.Equal(HttpStatusCode.Found, postLogoutProfile.StatusCode);
        Assert.Contains("connect/authorize", postLogoutProfile.Headers.Location?.ToString(), StringComparison.Ordinal);

        session.Dispose();
    }

    [Fact]
    public async Task Revoked_Session_Invalidates_Existing_Access_Token_EndToEnd()
    {
        var session = await SignInThroughSampleClientAsync();
        var profilePage = await session.RedirectingBrowser.GetAsync(new Uri(_fixture.ClientBaseUri, "Home/Profile"));
        var profileHtml = await profilePage.Content.ReadAsStringAsync();
        var accessToken = ExtractPrecedingPreValue(profileHtml, "Access token:");

        using var apiClient = CreateApiClient();
        apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var activeResponse = await apiClient.GetAsync(new Uri(_fixture.ApiBaseUri, "api/profile/me"));
        Assert.Equal(HttpStatusCode.OK, activeResponse.StatusCode);

        var sessionsPage = await session.RedirectingBrowser.GetAsync(new Uri(_fixture.SsoBaseUri, "sessions"));
        var sessionsHtml = await sessionsPage.Content.ReadAsStringAsync();
        Assert.Contains("My Sessions", sessionsHtml, StringComparison.Ordinal);

        var revokeToken = E2ETestHostFixture.ExtractHiddenInputValue(sessionsHtml, "__RequestVerificationToken");
        var revokeAction = ExtractFirstSessionRevokeAction(sessionsHtml);
        var revokeResponse = await session.Browser.PostAsync(new Uri(_fixture.SsoBaseUri, revokeAction),
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = revokeToken
            }));

        Assert.Equal(HttpStatusCode.Found, revokeResponse.StatusCode);

        var updatedSessionsPage = await session.RedirectingBrowser.GetAsync(new Uri(_fixture.SsoBaseUri, "sessions"));
        var updatedSessionsHtml = await updatedSessionsPage.Content.ReadAsStringAsync();
        Assert.Contains("Session revoked.", updatedSessionsHtml, StringComparison.Ordinal);
        Assert.Contains("Revoked", updatedSessionsHtml, StringComparison.Ordinal);

        var revokedResponse = await apiClient.GetAsync(new Uri(_fixture.ApiBaseUri, "api/profile/me"));
        Assert.Equal(HttpStatusCode.Unauthorized, revokedResponse.StatusCode);
        Assert.Contains("invalid_token", string.Join(' ', revokedResponse.Headers.WwwAuthenticate.Select(header => header.Parameter)), StringComparison.OrdinalIgnoreCase);

        session.Dispose();
    }

    private static Uri ResolveUri(Uri baseUri, Uri? location, string errorMessage)
    {
        var redirectUri = location ?? throw new InvalidOperationException(errorMessage);
        return redirectUri.IsAbsoluteUri ? redirectUri : new Uri(baseUri, redirectUri);
    }

    private async Task<AuthenticatedSession> SignInThroughSampleClientAsync()
    {
        var cookies = new CookieContainer();
        var browser = _fixture.CreateBrowserClient(cookies);
        var redirectingBrowser = _fixture.CreateBrowserClient(cookies, allowAutoRedirect: true);

        var clientHome = await browser.GetAsync(_fixture.ClientBaseUri);
        var clientHomeHtml = await clientHome.Content.ReadAsStringAsync();
        var clientToken = E2ETestHostFixture.ExtractHiddenInputValue(clientHomeHtml, "__RequestVerificationToken");

        var challengeResponse = await browser.PostAsync(new Uri(_fixture.ClientBaseUri, "Home/Login"),
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = clientToken
            }));

        Assert.Equal(HttpStatusCode.Found, challengeResponse.StatusCode);
        var authorizeUrl = ResolveUri(_fixture.ClientBaseUri, challengeResponse.Headers.Location, "Authorize redirect was missing.");

        var authorizeResponse = await browser.GetAsync(authorizeUrl);
        Assert.Equal(HttpStatusCode.Found, authorizeResponse.StatusCode);

        var loginUrl = ResolveUri(_fixture.SsoBaseUri, authorizeResponse.Headers.Location, "Login redirect was missing.");
        var loginPage = await browser.GetAsync(loginUrl);
        var loginHtml = await loginPage.Content.ReadAsStringAsync();
        var loginToken = E2ETestHostFixture.ExtractHiddenInputValue(loginHtml, "__RequestVerificationToken");
        var returnUrl = E2ETestHostFixture.ExtractHiddenInputValue(loginHtml, "ReturnUrl");

        var loginPost = await browser.PostAsync(new Uri(_fixture.SsoBaseUri, "account/login"),
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = loginToken,
                ["ReturnUrl"] = returnUrl,
                ["Email"] = _fixture.AdminEmail,
                ["Password"] = _fixture.AdminPassword,
                ["RememberMe"] = bool.FalseString.ToLowerInvariant()
            }));

        Assert.Equal(HttpStatusCode.Found, loginPost.StatusCode);
        var postLoginAuthorizeUrl = ResolveUri(_fixture.SsoBaseUri, loginPost.Headers.Location, "Post-login authorize redirect was missing.");

        var authorizeFormResponse = await browser.GetAsync(postLoginAuthorizeUrl);
        var authorizeFormHtml = await authorizeFormResponse.Content.ReadAsStringAsync();
        var callbackAction = E2ETestHostFixture.ExtractFormAction(authorizeFormHtml);
        var authorizationCode = E2ETestHostFixture.ExtractHiddenInputValue(authorizeFormHtml, "code");
        var oidcState = E2ETestHostFixture.ExtractHiddenInputValue(authorizeFormHtml, "state");
        var issuer = E2ETestHostFixture.ExtractHiddenInputValue(authorizeFormHtml, "iss");
        var callbackUri = Uri.TryCreate(callbackAction, UriKind.Absolute, out var absoluteCallbackUri)
            ? absoluteCallbackUri
            : new Uri(_fixture.ClientBaseUri, callbackAction);

        var callbackResponse = await browser.PostAsync(callbackUri,
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = authorizationCode,
                ["state"] = oidcState,
                ["iss"] = issuer
            }));

        Assert.Equal(HttpStatusCode.Found, callbackResponse.StatusCode);

        return new AuthenticatedSession(browser, redirectingBrowser);
    }

    private HttpClient CreateApiClient()
        => new(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = static (_, _, _, _) => true
        }, disposeHandler: true);

    private static string ExtractPrecedingPreValue(string html, string label)
    {
        var pattern = Regex.Escape(label) + @"</strong></p>\s*<pre[^>]*>(.*?)</pre>";
        var match = Regex.Match(html, pattern, RegexOptions.Singleline);

        if (!match.Success)
        {
            throw new InvalidOperationException($"Could not extract value for '{label}'.");
        }

        return WebUtility.HtmlDecode(match.Groups[1].Value).Trim();
    }

    private static string ExtractFirstSessionRevokeAction(string html)
    {
        var match = Regex.Match(html, "<form[^>]*action=\"([^\"]*/sessions/[0-9a-fA-F-]+/revoke)\"");

        if (!match.Success)
        {
            throw new InvalidOperationException("Could not find a session revoke action on the sessions page.");
        }

        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private sealed class AuthenticatedSession : IDisposable
    {
        public AuthenticatedSession(HttpClient browser, HttpClient redirectingBrowser)
        {
            Browser = browser;
            RedirectingBrowser = redirectingBrowser;
        }

        public HttpClient Browser { get; }

        public HttpClient RedirectingBrowser { get; }

        public void Dispose()
        {
            Browser.Dispose();
            RedirectingBrowser.Dispose();
        }
    }
}