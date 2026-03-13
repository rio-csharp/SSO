using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySso.Application.Common.Interfaces;
using MySso.Infrastructure.Options;
using MySso.Infrastructure.Services;

namespace MySso.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
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