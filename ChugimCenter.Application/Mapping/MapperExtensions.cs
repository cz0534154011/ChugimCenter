using AutoMapper;
using ChugimCenter.Application.DTOs.Common;

namespace ChugimCenter.Application.Mapping;

public static class MapperExtensions
{
    /// <summary>
    /// ממיר עמוד של ישויות לעמוד של DTOs, ומשאיר את נתוני העמוד כמו שהם.
    /// משמש את כל הרשימות עם pagination, כדי לא לכפול את הקוד בכל שירות.
    /// </summary>
    public static PagedResult<TDestination> MapPaged<TSource, TDestination>(
        this IMapper mapper, PagedResult<TSource> page)
        => new()
        {
            Items = mapper.Map<List<TDestination>>(page.Items),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        };
}
