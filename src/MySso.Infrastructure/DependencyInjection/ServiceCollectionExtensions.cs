using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySso.Application.Common.Interfaces;
using MySso.Infrastructure.Identity;
using MySso.Infrastructure.Options;
using MySso.Infrastructure.Persistence;
using MySso.Infrastructure.Persistence.Repositories;
using MySso.Infrastructure.Services;
using OpenIddict.Abstractions;
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
        services.AddInfrastructureIdentity();
        services.AddInfrastructureOpenIddict(configuration);
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<MySsoDbContext>());
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IRoleRepository, EfRoleRepository>();
        services.AddScoped<IClientRepository, EfClientRepository>();
        services.AddScoped<IUserSessionRepository, EfUserSessionRepository>();
        services.AddScoped<IAuditLogRepository, EfAuditLogRepository>();

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

                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(hostOptions.AccessTokenLifetimeMinutes));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(hostOptions.RefreshTokenLifetimeDays));

                options.RegisterScopes(Scopes.Email, Scopes.OfflineAccess, Scopes.OpenId, Scopes.Profile, "api");

                options.AddEphemeralEncryptionKey()
                    .AddEphemeralSigningKey();

                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough();
            });

        return services;
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