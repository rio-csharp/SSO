using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySso.Infrastructure.DependencyInjection;
using MySso.Infrastructure.Identity;
using MySso.Infrastructure.Persistence;

namespace MySso.IntegrationTests;

public sealed class IdentityRegistrationTests
{
    [Fact]
    public void AddInfrastructureIdentity_Registers_User_And_Role_Managers()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<MySsoDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString("N")));
        services.AddInfrastructureIdentity();

        using var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetService<UserManager<SsoIdentityUser>>());
        Assert.NotNull(serviceProvider.GetService<RoleManager<SsoIdentityRole>>());
    }
}