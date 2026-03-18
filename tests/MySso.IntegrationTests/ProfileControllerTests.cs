using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySso.Api.Controllers;

namespace MySso.IntegrationTests;

public sealed class ProfileControllerTests
{
    [Fact]
    public void Me_Returns_Email_From_Unmapped_Email_Claim()
    {
        var controller = CreateController(new Claim("sub", "user-1"), new Claim("email", "alice@example.com"));

        var result = Assert.IsType<OkObjectResult>(controller.Me());
        var payload = JsonSerializer.Serialize(result.Value);

        Assert.Contains("\"email\":\"alice@example.com\"", payload, StringComparison.Ordinal);
    }

    [Fact]
    public void Me_Falls_Back_To_Mapped_Email_Claim_Type()
    {
        var controller = CreateController(new Claim("sub", "user-1"), new Claim(ClaimTypes.Email, "alice@example.com"));

        var result = Assert.IsType<OkObjectResult>(controller.Me());
        var payload = JsonSerializer.Serialize(result.Value);

        Assert.Contains("\"email\":\"alice@example.com\"", payload, StringComparison.Ordinal);
    }

    private static ProfileController CreateController(params Claim[] claims)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"))
        };

        return new ProfileController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }
}