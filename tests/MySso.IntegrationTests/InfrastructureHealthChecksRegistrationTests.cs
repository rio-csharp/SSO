using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySso.Infrastructure.DependencyInjection;

namespace MySso.IntegrationTests;

public sealed class InfrastructureHealthChecksRegistrationTests
{
    [Fact]
    public async Task AddInfrastructure_Registers_Live_And_Ready_Health_Checks()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Development",
                ["ConnectionStrings:PostgreSql"] = "Host=localhost;Port=5432;Database=mysso;Username=postgres;Password=postgres",
                ["MySso:Issuer"] = "https://localhost:5001",
                ["MySso:CookieName"] = "MySso.Auth",
                ["MySso:RequireHttps"] = "true",
                ["MySso:AccessTokenLifetimeMinutes"] = "15",
                ["MySso:RefreshTokenLifetimeDays"] = "14"
            })
            .Build();

        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<HealthCheckServiceOptions>>().Value;

        Assert.Contains(options.Registrations, registration => registration.Name == "self" && registration.Tags.Contains("live") && registration.Tags.Contains("ready"));
        Assert.Contains(options.Registrations, registration => registration.Name == "database" && registration.Tags.Contains("ready"));
    }
}