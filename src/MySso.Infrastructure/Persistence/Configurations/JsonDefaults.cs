using System.Text.Json;

namespace MySso.Infrastructure.Persistence.Configurations;

internal static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}