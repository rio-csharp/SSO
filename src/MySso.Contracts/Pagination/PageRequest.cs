namespace MySso.Contracts.Pagination;

public sealed record PageRequest
{
    public PageRequest(int pageNumber = 1, int pageSize = 20)
    {
        if (pageNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
        }

        if (pageSize is <= 0 or > 500)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 500.");
        }

        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int PageNumber { get; }

    public int PageSize { get; }
}