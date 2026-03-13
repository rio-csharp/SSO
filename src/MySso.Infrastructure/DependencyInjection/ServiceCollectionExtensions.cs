using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySso.Application.Common.Interfaces;
using MySso.Infrastructure.Options;
using MySso.Infrastructure.Persistence;
using MySso.Infrastructure.Persistence.Repositories;
using MySso.Infrastructure.Services;

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

        services.AddDbContext<MySsoDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<MySsoDbContext>());
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IRoleRepository, EfRoleRepository>();
        services.AddScoped<IClientRepository, EfClientRepository>();
        services.AddScoped<IUserSessionRepository, EfUserSessionRepository>();
        services.AddScoped<IAuditLogRepository, EfAuditLogRepository>();

        return services.AddInfrastructureCore(configuration);
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