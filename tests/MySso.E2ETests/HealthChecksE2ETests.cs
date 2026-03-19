using System.Net;

namespace MySso.E2ETests;

[Collection(nameof(E2ECollection))]
public sealed class HealthChecksE2ETests
{
    private readonly E2ETestHostFixture _fixture;

    public HealthChecksE2ETests(E2ETestHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData("https://localhost:5001/health/live")]
    [InlineData("https://localhost:5001/health/ready")]
    [InlineData("https://localhost:7061/health/live")]
    [InlineData("https://localhost:7061/health/ready")]
    public async Task Health_Endpoints_Return_Success(string url)
    {
        using var client = _fixture.CreateBrowserClient(new CookieContainer(), allowAutoRedirect: true);

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}