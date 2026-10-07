using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VetGest.Application.Alerts;
using VetGest.Application.Pets;
using VetGest.Application.Pregnancies;
using VetGest.Infrastructure.Persistence;

namespace VetGest.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure services.
/// Handles EF Core configuration. Identity and Authorization registration
/// should be done at the API/Web layer.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Register EF Core database context.
    /// Identity and Authorization must be registered separately at the web layer.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<VetGestDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection not configured");
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(VetGestDbContext).Assembly.GetName().Name);
                sqlOptions.EnableRetryOnFailure();
            });
        });

        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<IPregnancyRepository, PregnancyRepository>();
        services.AddScoped<IPregnancyDiaryRepository, PregnancyDiaryRepository>();
        services.AddScoped<IVetConnectionRepository, VetConnectionRepository>();
        services.AddScoped<IAlertRuleRepository, AlertRuleRepository>();

        return services;
    }
}
