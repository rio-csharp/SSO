using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySso.Infrastructure.DependencyInjection;
using MySso.Infrastructure.Persistence;
using OpenIddict.Abstractions;

namespace MySso.IntegrationTests;

public sealed class OpenIddictRegistrationTests
{
    [Fact]
    public void AddInfrastructureOpenIddict_Registers_Core_Managers()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<MySsoDbContext>(options =>
        {
            options.UseInMemoryDatabase(Guid.NewGuid().ToString("N"));
            options.UseOpenIddict<Guid>();
        });

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Development",
                ["MySso:Issuer"] = "https://localhost:5001",
                ["MySso:CookieName"] = "MySso.Auth",
                ["MySso:RequireHttps"] = "true",
                ["MySso:AccessTokenLifetimeMinutes"] = "15",
                ["MySso:RefreshTokenLifetimeDays"] = "14"
            })
            .Build();

        services.AddInfrastructureIdentity();
        services.AddInfrastructureOpenIddict(configuration);

        using var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetService<IOpenIddictApplicationManager>());
        Assert.NotNull(serviceProvider.GetService<IOpenIddictTokenManager>());
    }

    [Fact]
    public void AddInfrastructureOpenIddict_Throws_In_NonDevelopment_When_Certificates_Are_Not_Configured()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<MySsoDbContext>(options =>
        {
            options.UseInMemoryDatabase(Guid.NewGuid().ToString("N"));
            options.UseOpenIddict<Guid>();
        });

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Production",
                ["MySso:Issuer"] = "https://localhost:5001",
                ["MySso:CookieName"] = "MySso.Auth",
                ["MySso:RequireHttps"] = "true",
                ["MySso:AccessTokenLifetimeMinutes"] = "15",
                ["MySso:RefreshTokenLifetimeDays"] = "14"
            })
            .Build();

        services.AddInfrastructureIdentity();

        var exception = Assert.Throws<InvalidOperationException>(() => services.AddInfrastructureOpenIddict(configuration));
        Assert.Contains("non-development environments require", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}