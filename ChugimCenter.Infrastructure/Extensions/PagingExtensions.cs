using ChugimCenter.Application.DTOs.Common;
using Microsoft.EntityFrameworkCore;

namespace ChugimCenter.Infrastructure.Extensions;

public static class PagingExtensions
{
    /// <summary>
    /// Pagination אמיתי מול בסיס הנתונים: Skip ו-Take בתוך השאילתה.
    /// משמש את כל הרשימות במערכת, כדי לא לכפול את הקוד.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, int page, int pageSize, CancellationToken ct)
    {
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
