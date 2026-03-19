using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MySso.Infrastructure.Options;

namespace MySso.Infrastructure.DependencyInjection;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddMySsoWebRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        return services.AddRateLimitOptions(configuration);
    }

    public static IServiceCollection AddMySsoApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        return services.AddRateLimitOptions(configuration);
    }

    public static IApplicationBuilder UseMySsoWebRateLimiting(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = app.ApplicationServices.GetRequiredService<IOptions<MySsoRateLimitOptions>>().Value;
        var limiters = new Dictionary<string, FixedWindowRateLimiter>(StringComparer.Ordinal);
        var syncLock = new object();

        return app.Use(async (context, next) =>
        {
            var descriptor = GetWebDescriptor(context, options);
            if (descriptor is null)
            {
                await next();
                return;
            }

            using var lease = await AcquireLeaseAsync(limiters, syncLock, descriptor, context.RequestAborted);
            if (!lease.IsAcquired)
            {
                await RejectAsync(context, lease);
                return;
            }

            await next();
        });
    }

    public static IApplicationBuilder UseMySsoApiRateLimiting(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = app.ApplicationServices.GetRequiredService<IOptions<MySsoRateLimitOptions>>().Value;
        var limiters = new Dictionary<string, FixedWindowRateLimiter>(StringComparer.Ordinal);
        var syncLock = new object();

        return app.Use(async (context, next) =>
        {
            var descriptor = GetApiDescriptor(context, options);
            if (descriptor is null)
            {
                await next();
                return;
            }

            using var lease = await AcquireLeaseAsync(limiters, syncLock, descriptor, context.RequestAborted);
            if (!lease.IsAcquired)
            {
                await RejectAsync(context, lease);
                return;
            }

            await next();
        });
    }

    private static IServiceCollection AddRateLimitOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MySsoRateLimitOptions>()
            .Bind(configuration.GetSection(MySsoRateLimitOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    private static async Task RejectAsync(HttpContext context, RateLimitLease lease)
    {
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        await context.Response.WriteAsync("Too many requests.", context.RequestAborted);
    }

    private static ValueTask<RateLimitLease> AcquireLeaseAsync(Dictionary<string, FixedWindowRateLimiter> limiters, object syncLock, RateLimitDescriptor descriptor, CancellationToken cancellationToken)
    {
        FixedWindowRateLimiter limiter;
        lock (syncLock)
        {
            if (!limiters.TryGetValue(descriptor.PartitionKey, out limiter!))
            {
                limiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
                {
                    PermitLimit = descriptor.PermitLimit,
                    Window = TimeSpan.FromSeconds(descriptor.WindowSeconds),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });

                limiters[descriptor.PartitionKey] = limiter;
            }
        }

        return limiter.AcquireAsync(1, cancellationToken);
    }

    private static RateLimitDescriptor? GetWebDescriptor(HttpContext context, MySsoRateLimitOptions options)
    {
        var remoteIpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.Request.Path;

        if (path.Equals("/account/login", StringComparison.OrdinalIgnoreCase))
        {
            return new RateLimitDescriptor($"web-login:{remoteIpAddress}", options.LoginPermitLimit, options.WindowSeconds);
        }

        if (path.StartsWithSegments("/connect", StringComparison.OrdinalIgnoreCase))
        {
            return new RateLimitDescriptor($"web-protocol:{remoteIpAddress}", options.ProtocolPermitLimit, options.WindowSeconds);
        }

        return null;
    }

    private static RateLimitDescriptor? GetApiDescriptor(HttpContext context, MySsoRateLimitOptions options)
    {
        var remoteIpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.Request.Path;

        if (path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            return new RateLimitDescriptor($"api:{remoteIpAddress}", options.ApiPermitLimit, options.WindowSeconds);
        }

        return null;
    }

    private sealed record RateLimitDescriptor(string PartitionKey, int PermitLimit, int WindowSeconds);
}