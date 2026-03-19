using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySso.Application.Features.Clients;
using MySso.Application.Common.Interfaces;
using MySso.Application.Features.IdentityUsers;
using MySso.Application.Features.Roles;
using MySso.Application.Features.UserSessions;
using MySso.Infrastructure.HealthChecks;
using MySso.Infrastructure.Identity;
using MySso.Infrastructure.Options;
using MySso.Infrastructure.Persistence;
using MySso.Infrastructure.Persistence.Repositories;
using MySso.Infrastructure.Services;
using OpenIddict.Abstractions;
using System.Security.Cryptography.X509Certificates;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace MySso.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("PostgreSql");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'PostgreSql' is required.");
        }

        services.AddDbContext<MySsoDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
            options.UseOpenIddict<Guid>();
        });
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), new[] { "live", "ready" })
            .AddCheck<MySsoDatabaseHealthCheck>("database", tags: new[] { "ready" });
        services.AddInfrastructureIdentity();
        services.AddInfrastructureOpenIddict(configuration);
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<MySsoDbContext>());
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IRoleRepository, EfRoleRepository>();
        services.AddScoped<IClientRepository, EfClientRepository>();
        services.AddScoped<IClientProvisioningService, OpenIddictClientProvisioningService>();
        services.AddScoped<IUserSessionRepository, EfUserSessionRepository>();
        services.AddScoped<IAuditLogRepository, EfAuditLogRepository>();
        services.AddScoped<IAdministrationQueryService, AdministrationQueryService>();
        services.AddScoped<IIdentityAccountProvisioningService, IdentityAccountProvisioningService>();
        services.AddScoped<ISessionLifecycleService, SessionLifecycleService>();
        services.AddScoped<CreateLocalUserHandler>();
        services.AddScoped<CreateRoleHandler>();
        services.AddScoped<RegisterClientHandler>();
        services.AddScoped<RevokeUserSessionHandler>();

        return services.AddInfrastructureCore(configuration);
    }

    public static IServiceCollection AddInfrastructureIdentity(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddDataProtection();

        services.AddIdentityCore<SsoIdentityUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 12;
            })
            .AddRoles<SsoIdentityRole>()
            .AddEntityFrameworkStores<MySsoDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        return services;
    }

    public static IServiceCollection AddInfrastructureOpenIddict(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var hostOptions = configuration.GetSection(MySsoHostOptions.SectionName).Get<MySsoHostOptions>() ?? new MySsoHostOptions();
        var environmentName = configuration["ASPNETCORE_ENVIRONMENT"] ?? configuration["DOTNET_ENVIRONMENT"] ?? Environments.Production;
        var isDevelopment = string.Equals(environmentName, Environments.Development, StringComparison.OrdinalIgnoreCase);

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<MySsoDbContext>()
                    .ReplaceDefaultEntities<Guid>();
            })
            .AddServer(options =>
            {
                options.SetIssuer(new Uri(hostOptions.Issuer));
                options.SetAuthorizationEndpointUris("connect/authorize")
                    .SetEndSessionEndpointUris("connect/logout")
                    .SetIntrospectionEndpointUris("connect/introspect")
                    .SetRevocationEndpointUris("connect/revoke")
                    .SetTokenEndpointUris("connect/token")
                    .SetUserInfoEndpointUris("connect/userinfo");

                options.AllowAuthorizationCodeFlow();
                options.AllowRefreshTokenFlow();
                options.RequireProofKeyForCodeExchange();
                options.DisableAccessTokenEncryption();

                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(hostOptions.AccessTokenLifetimeMinutes));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(hostOptions.RefreshTokenLifetimeDays));

                options.RegisterScopes(Scopes.Email, Scopes.OfflineAccess, Scopes.OpenId, Scopes.Profile, "api");

                if (isDevelopment)
                {
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                }
                else
                {
                    options.AddEncryptionCertificate(CreateCertificate(hostOptions.EncryptionCertificatePath, hostOptions.EncryptionCertificatePassword, nameof(hostOptions.EncryptionCertificatePath)))
                        .AddSigningCertificate(CreateCertificate(hostOptions.SigningCertificatePath, hostOptions.SigningCertificatePassword, nameof(hostOptions.SigningCertificatePath)));
                }

                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough();
            });

        return services;
    }

    private static X509Certificate2 CreateCertificate(string? certificatePath, string? certificatePassword, string optionName)
    {
        if (string.IsNullOrWhiteSpace(certificatePath))
        {
            throw new InvalidOperationException($"OpenIddict non-development environments require '{optionName}' to be configured.");
        }

        return X509CertificateLoader.LoadPkcs12FromFile(certificatePath, certificatePassword, X509KeyStorageFlags.MachineKeySet);
    }

    public static IServiceCollection AddInfrastructureCore(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddHttpContextAccessor();
        services.AddOptions<MySsoHostOptions>()
            .Bind(configuration.GetSection(MySsoHostOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }
}