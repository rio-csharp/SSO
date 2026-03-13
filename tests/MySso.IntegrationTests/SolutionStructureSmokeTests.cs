using MySso.Application;
using MySso.Contracts;
using MySso.Domain;
using MySso.Infrastructure;

namespace MySso.IntegrationTests;

public sealed class SolutionStructureSmokeTests
{
    [Fact]
    public void Core_Assemblies_Are_Loadable()
    {
        Assert.NotNull(typeof(MySso.Domain.AssemblyReference).Assembly);
        Assert.NotNull(typeof(MySso.Contracts.AssemblyReference).Assembly);
        Assert.NotNull(typeof(MySso.Application.AssemblyReference).Assembly);
        Assert.NotNull(typeof(MySso.Infrastructure.AssemblyReference).Assembly);
    }
}