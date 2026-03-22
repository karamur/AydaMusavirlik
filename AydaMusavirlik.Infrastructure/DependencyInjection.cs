using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using AydaMusavirlik.Application.Common.Interfaces;
using AydaMusavirlik.Infrastructure.Persistence;
using AydaMusavirlik.Infrastructure.Persistence.Repositories;
using AydaMusavirlik.Infrastructure.Services;

namespace AydaMusavirlik.Infrastructure;

/// <summary>
/// Infrastructure katmani Dependency Injection yapilandirmasi
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Context
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=AydaMusavirlik.db";
        var provider = configuration["DatabaseProvider"] ?? "SQLite";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            switch (provider.ToLower())
            {
                case "sqlserver":
                    options.UseSqlServer(connectionString);
                    break;
                case "postgresql":
                    options.UseNpgsql(connectionString);
                    break;
                default:
                    options.UseSqlite(connectionString);
                    break;
            }
        });

        // Unit of Work & Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IFinancialAnalysisService, FinancialAnalysisService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureWithSettings(this IServiceCollection services, 
        string provider, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            switch (provider.ToLower())
            {
                case "sqlserver":
                    options.UseSqlServer(connectionString);
                    break;
                case "postgresql":
                    options.UseNpgsql(connectionString);
                    break;
                default:
                    options.UseSqlite(connectionString);
                    break;
            }
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IFinancialAnalysisService, FinancialAnalysisService>();

        return services;
    }
}