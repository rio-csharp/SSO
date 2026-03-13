using Microsoft.AspNetCore.Authentication.JwtBearer;
using MySso.Infrastructure.DependencyInjection;
using MySso.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var settings = builder.Configuration.GetSection(MySsoHostOptions.SectionName).Get<MySsoHostOptions>() ?? new MySsoHostOptions();
        options.Authority = settings.Issuer;
        options.Audience = "resource_api";
        options.RequireHttpsMetadata = settings.RequireHttps;
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    service = "MySso.Api",
    stage = "phase-3",
    status = "ready"
}));

app.Run();
