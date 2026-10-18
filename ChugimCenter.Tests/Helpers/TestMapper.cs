using AutoMapper;
using ChugimCenter.Application;
using Microsoft.Extensions.DependencyInjection;

namespace ChugimCenter.Tests.Helpers;

/// <summary>
/// ה-AutoMapper האמיתי עם ה-Profiles של הפרויקט, ולא מדומה, כדי שהבדיקות יבדקו גם את ההמרה ל-DTO.
/// נבנה פעם אחת, באותה דרך שבה הוא נבנה בשרת: AddAutoMapper על פרויקט Application.
/// </summary>
public static class TestMapper
{
    public static IMapper Instance { get; } = Create();

    private static IMapper Create()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }
}
