using System.Net;

namespace MySso.E2ETests;

[CollectionDefinition(nameof(RateLimitE2ECollection))]
public sealed class RateLimitE2ECollection : ICollectionFixture<E2ETestHostFixture>
{
}

[Collection(nameof(RateLimitE2ECollection))]
public sealed class RateLimitingE2ETests
{
    private readonly E2ETestHostFixture _fixture;

    public RateLimitingE2ETests(E2ETestHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Login_Page_Is_Rate_Limited_After_Allowed_Burst()
    {
        using var client = _fixture.CreateBrowserClient(new CookieContainer(), allowAutoRedirect: false);

        HttpResponseMessage? lastResponse = null;
        for (var index = 0; index < 11; index++)
        {
            lastResponse = await client.GetAsync(new Uri(_fixture.SsoBaseUri, "account/login"));
        }

        Assert.NotNull(lastResponse);
        Assert.Equal((HttpStatusCode)429, lastResponse!.StatusCode);
        Assert.True(lastResponse.Headers.RetryAfter is not null || lastResponse.Headers.TryGetValues("Retry-After", out _));
    }
}