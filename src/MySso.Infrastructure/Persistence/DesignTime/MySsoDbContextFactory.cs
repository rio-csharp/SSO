using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MySso.Infrastructure.Persistence.DesignTime;

public sealed class MySsoDbContextFactory : IDesignTimeDbContextFactory<MySsoDbContext>
{
    public MySsoDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var connectionString = configuration.GetConnectionString("PostgreSql");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'PostgreSql' is required for design-time operations.");
        }

        var builder = new DbContextOptionsBuilder<MySsoDbContext>();
        builder.UseNpgsql(connectionString);

        return new MySsoDbContext(builder.Options);
    }

    private static IConfiguration BuildConfiguration()
    {
        var candidatePaths = new[]
        {
            Directory.GetCurrentDirectory(),
            Path.Combine(Directory.GetCurrentDirectory(), "src", "MySso.Web"),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "MySso.Web"))
        };

        var configurationBuilder = new ConfigurationBuilder();

        foreach (var candidatePath in candidatePaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!Directory.Exists(candidatePath))
            {
                continue;
            }

            configurationBuilder.SetBasePath(candidatePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true);
        }

        configurationBuilder.AddEnvironmentVariables();

        return configurationBuilder.Build();
    }
}