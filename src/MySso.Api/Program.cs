using Microsoft.AspNetCore.Authentication.JwtBearer;
using MySso.Application.Common.Interfaces;
using MySso.Infrastructure.DependencyInjection;
using MySso.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMySsoApiRateLimiting(builder.Configuration);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var settings = builder.Configuration.GetSection(MySsoHostOptions.SectionName).Get<MySsoHostOptions>() ?? new MySsoHostOptions();
        options.Authority = settings.Issuer;
        options.Audience = "resource_api";
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = settings.RequireHttps;
        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "role";
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
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseMySsoApiRateLimiting();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new()
{
    Predicate = check => check.Tags.Contains("live")
});
app.MapHealthChecks("/health/ready", new()
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    service = "MySso.Api",
    stage = "phase-3",
    status = "ready"
}));

app.Run();
