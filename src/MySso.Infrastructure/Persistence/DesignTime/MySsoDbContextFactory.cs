using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MySso.Infrastructure.Persistence.DesignTime;

public sealed class MySsoDbContextFactory : IDesignTimeDbContextFactory<MySsoDbContext>
{
    public MySsoDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<MySsoDbContext>();
        builder.UseNpgsql("Host=localhost;Port=5432;Database=mysso;Username=postgres;Password=postgres");

        return new MySsoDbContext(builder.Options);
    }
}