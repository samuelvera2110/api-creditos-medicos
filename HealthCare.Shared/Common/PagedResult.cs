namespace HealthCare.Shared.Common;

public sealed record PagedResult<T>(
    IEnumerable<T> Items,
    int            Total,
    int            Page,
    int            PageSize)
{
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
}