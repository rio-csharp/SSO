using System.ComponentModel.DataAnnotations;

namespace MySso.Infrastructure.Options;

public sealed class MySsoRateLimitOptions
{
    public const string SectionName = "RateLimiting";

    [Range(1, 1000)]
    public int LoginPermitLimit { get; set; } = 10;

    [Range(1, 1000)]
    public int ProtocolPermitLimit { get; set; } = 30;

    [Range(1, 5000)]
    public int ApiPermitLimit { get; set; } = 120;

    [Range(1, 3600)]
    public int WindowSeconds { get; set; } = 60;
}