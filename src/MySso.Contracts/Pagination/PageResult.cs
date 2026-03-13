namespace MySso.Contracts.Pagination;

public sealed record PageResult<T>(IReadOnlyCollection<T> Items, int PageNumber, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}