# MySso

## Start

Requirements:

- .NET SDK 10
- PostgreSQL
- trusted ASP.NET Core development certificate

From the repository root:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__PostgreSql = "Host=localhost;Port=5432;Database=mysso;Username=postgres;Password=postgres"
$env:Bootstrap__AdminPassword = "ChangeThis123!"
$env:Bootstrap__ClientSecret = "sample-client-secret"
dotnet ef database update --project src/MySso.Infrastructure/MySso.Infrastructure.csproj --startup-project src/MySso.Web/MySso.Web.csproj
```

Start the SSO web host:

```powershell
dotnet run --project src/MySso.Web/MySso.Web.csproj --launch-profile https
```

Start the protected API:

```powershell
dotnet run --project src/MySso.Api/MySso.Api.csproj --launch-profile https
```

Start the sample client:

```powershell
dotnet run --project samples/MySso.Sample.ClientWeb/MySso.Sample.ClientWeb.csproj --launch-profile https
```

URLs:

- SSO: https://localhost:5001
- API: https://localhost:7061
- Client: https://localhost:7041

Usage details:

- docs/app-usage.md
