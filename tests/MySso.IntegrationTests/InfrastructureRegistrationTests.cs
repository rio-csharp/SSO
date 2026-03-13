using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySso.Application.Common.Interfaces;
using MySso.Infrastructure.DependencyInjection;

namespace MySso.IntegrationTests;

public sealed class InfrastructureRegistrationTests
{
    [Fact]
    public void AddInfrastructureCore_Registers_Core_Abstractions()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MySso:Issuer"] = "https://localhost:5001",
                ["MySso:CookieName"] = "MySso.Auth",
                ["MySso:RequireHttps"] = "true",
                ["MySso:AccessTokenLifetimeMinutes"] = "15",
                ["MySso:RefreshTokenLifetimeDays"] = "14"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructureCore(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<IDateTimeProvider>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICurrentUserContext>());
    }
}