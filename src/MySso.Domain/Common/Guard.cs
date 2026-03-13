namespace MySso.Domain.Common;

public static class Guard
{
    public static Guid AgainstEmpty(Guid value, string paramName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Value cannot be an empty GUID.", paramName);
        }

        return value;
    }

    public static string AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
        }

        return value.Trim();
    }

    public static IReadOnlyCollection<T> AgainstNullOrEmpty<T>(IEnumerable<T>? values, string paramName)
    {
        if (values is null)
        {
            throw new ArgumentNullException(paramName);
        }

        var materialized = values.ToArray();

        if (materialized.Length == 0)
        {
            throw new ArgumentException("Collection cannot be empty.", paramName);
        }

        return materialized;
    }
}