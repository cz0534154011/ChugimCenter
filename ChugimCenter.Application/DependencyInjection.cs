using ChugimCenter.Application.Interfaces.Services;
using ChugimCenter.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChugimCenter.Application;

public static class DependencyInjection
{
    /// <summary>רישום כל רכיבי Application במקום אחד. נקרא מ-Program.cs (בשלב 6).</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AutoMapper סורק את הפרויקט הזה ומוצא את כל ה-Profiles. IMapper נרשם כ-Singleton
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        // השעון של המערכת. מוזרק (ולא DateTime.UtcNow ישירות) כדי שבבדיקות אפשר יהיה לקבוע "היום"
        services.AddSingleton(TimeProvider.System);

        // Scoped: מופע אחד לכל בקשה, כמו ה-Repositories וה-DbContext שהם משתמשים בהם
        services.AddScoped<AccountCreator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICourseGroupService, CourseGroupService>();

        return services;
    }
}
