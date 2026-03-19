using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;

namespace MySso.E2ETests;

public sealed class E2ETestHostFixture : IAsyncLifetime
{
    private readonly List<ManagedHostProcess> _processes = new();

    public Uri SsoBaseUri { get; } = new("https://localhost:5001/");

    public Uri ApiBaseUri { get; } = new("https://localhost:7061/");

    public Uri ClientBaseUri { get; } = new("https://localhost:7041/");

    public string AdminEmail => "admin@mysso.local";

    public string AdminPassword => GetRequiredEnvironmentVariable("Bootstrap__AdminPassword");

    public async Task InitializeAsync()
    {
        var rootPath = FindRepositoryRoot();

        var webHost = StartHostProcess(
            Path.Combine(rootPath, "src", "MySso.Web", "bin", "Debug", "net10.0", "MySso.Web.dll"),
            "https://localhost:5001;http://localhost:5000");
        var apiHost = StartHostProcess(
            Path.Combine(rootPath, "src", "MySso.Api", "bin", "Debug", "net10.0", "MySso.Api.dll"),
            "https://localhost:7061;http://localhost:5061");
        var clientHost = StartHostProcess(
            Path.Combine(rootPath, "samples", "MySso.Sample.ClientWeb", "bin", "Debug", "net10.0", "MySso.Sample.ClientWeb.dll"),
            "https://localhost:7041;http://localhost:5041");

        _processes.Add(webHost);
        _processes.Add(apiHost);
        _processes.Add(clientHost);

        await WaitUntilReadyAsync(SsoBaseUri, webHost);
        await WaitUntilReadyAsync(ApiBaseUri, apiHost);
        await WaitUntilReadyAsync(ClientBaseUri, clientHost);
    }

    public Task DisposeAsync()
    {
        foreach (var process in Enumerable.Reverse(_processes))
        {
            process.Dispose();
        }

        return Task.CompletedTask;
    }

    public HttpClient CreateBrowserClient(CookieContainer cookieContainer, bool allowAutoRedirect = false)
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = allowAutoRedirect,
            CookieContainer = cookieContainer,
            ServerCertificateCustomValidationCallback = static (_, _, _, _) => true,
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
        };

        return new HttpClient(handler, disposeHandler: true);
    }

    public static string ExtractHiddenInputValue(string html, string name)
    {
        var pattern = $"name=\"{RegexEscape(name)}\"[^>]*value=\"([^\"]+)\"";
        var match = System.Text.RegularExpressions.Regex.Match(html, pattern);

        if (!match.Success)
        {
            throw new InvalidOperationException($"Hidden input '{name}' was not found.");
        }

        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    public static string ExtractFormAction(string html)
    {
        var match = System.Text.RegularExpressions.Regex.Match(html, "<form[^>]*action=\"([^\"]+)\"");

        if (!match.Success)
        {
            throw new InvalidOperationException("Form action was not found.");
        }

        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static ManagedHostProcess StartHostProcess(string dllPath, string urls)
    {
        if (!File.Exists(dllPath))
        {
            throw new FileNotFoundException($"Expected host binary was not found: {dllPath}");
        }

        var workingDirectory = Path.GetDirectoryName(dllPath)
            ?? throw new InvalidOperationException($"Could not resolve working directory for {dllPath}");

        var startInfo = new ProcessStartInfo("dotnet", $"\"{dllPath}\"")
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.Environment["ASPNETCORE_URLS"] = urls;
        startInfo.Environment["ConnectionStrings__PostgreSql"] = GetRequiredEnvironmentVariable("ConnectionStrings__PostgreSql");
        startInfo.Environment["Bootstrap__AdminPassword"] = GetRequiredEnvironmentVariable("Bootstrap__AdminPassword");
        startInfo.Environment["Bootstrap__ClientSecret"] = GetRequiredEnvironmentVariable("Bootstrap__ClientSecret");

        return new ManagedHostProcess(startInfo);
    }

    private static async Task WaitUntilReadyAsync(Uri baseUri, ManagedHostProcess hostProcess)
    {
        using var httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = static (_, _, _, _) => true
        });

        var timeoutAt = DateTimeOffset.UtcNow.AddSeconds(45);

        while (DateTimeOffset.UtcNow < timeoutAt)
        {
            if (hostProcess.HasExited)
            {
                throw new InvalidOperationException($"Host process for {baseUri} exited before becoming ready.{Environment.NewLine}{hostProcess.GetOutput()}");
            }

            try
            {
                using var response = await httpClient.GetAsync(baseUri);
                if ((int)response.StatusCode < 500)
                {
                    return;
                }
            }
            catch
            {
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"Timed out waiting for host {baseUri} to become ready.{Environment.NewLine}{hostProcess.GetOutput()}");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "SSO.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root from test output directory.");
    }

    private static string GetRequiredEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        throw new InvalidOperationException($"Environment variable '{name}' must be set before running E2E tests.");
    }

    private static string RegexEscape(string value)
        => System.Text.RegularExpressions.Regex.Escape(value);

    private sealed class ManagedHostProcess : IDisposable
    {
        private readonly StringBuilder _output = new();
        private readonly Process _process;

        public ManagedHostProcess(ProcessStartInfo startInfo)
        {
            _process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            _process.OutputDataReceived += (_, args) => AppendLine(args.Data);
            _process.ErrorDataReceived += (_, args) => AppendLine(args.Data);

            if (!_process.Start())
            {
                throw new InvalidOperationException($"Failed to start process: {startInfo.FileName} {startInfo.Arguments}");
            }

            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        public void Dispose()
        {
            try
            {
                if (!_process.HasExited)
                {
                    _process.Kill(entireProcessTree: true);
                    _process.WaitForExit(5000);
                }
            }
            catch
            {
            }
            finally
            {
                _process.Dispose();
            }
        }

        public bool HasExited => _process.HasExited;

        public string GetOutput()
        {
            lock (_output)
            {
                return _output.ToString();
            }
        }

        private void AppendLine(string? line)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                lock (_output)
                {
                    _output.AppendLine(line);
                }
            }
        }
    }
}