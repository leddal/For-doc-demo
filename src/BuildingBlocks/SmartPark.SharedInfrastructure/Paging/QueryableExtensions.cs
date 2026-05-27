using Microsoft.EntityFrameworkCore;
using SmartPark.SharedContracts.Common;

namespace SmartPark.SharedInfrastructure.Paging;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, request.PageNumber, request.Take);
    }
}
