using Microsoft.EntityFrameworkCore;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Repositories;
using LMS.Web.Services;

namespace LMS.Web.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, 
        string connectionString)
    {
        // Infrastructure
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Application Services
        services.AddScoped<CourseService>();
        services.AddScoped<LessonService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<NotificationService>();

        return services;
    }
} 