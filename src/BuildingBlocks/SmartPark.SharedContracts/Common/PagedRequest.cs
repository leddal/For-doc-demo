namespace SmartPark.SharedContracts.Common;

public sealed record PagedRequest
{
    private const int MaxPageSize = 100;

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public int Skip => (Math.Max(PageNumber, 1) - 1) * Take;

    public int Take => Math.Clamp(PageSize, 1, MaxPageSize);
}
