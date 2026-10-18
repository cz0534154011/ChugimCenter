using ChugimCenter.Application.Interfaces.Repositories;
using ChugimCenter.Infrastructure.Persistence;
using ChugimCenter.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChugimCenter.Infrastructure;

public static class DependencyInjection
{
    /// <summary>רישום כל רכיבי Infrastructure במקום אחד. נקרא מ-Program.cs.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // DbContext הוא Scoped: מופע אחד לכל בקשת HTTP
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICourseGroupRepository, CourseGroupRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

        return services;
    }
}
