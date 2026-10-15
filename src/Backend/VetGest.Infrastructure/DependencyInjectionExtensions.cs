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
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        ValidatePostgresConnectionString(connectionString);

        // Register DbContext
        services.AddDbContext<VetGestDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(VetGestDbContext).Assembly.GetName().Name);
                npgsqlOptions.EnableRetryOnFailure();
            });
        });

        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<IPregnancyRepository, PregnancyRepository>();
        services.AddScoped<IPregnancyDiaryRepository, PregnancyDiaryRepository>();
        services.AddScoped<IVetConnectionRepository, VetConnectionRepository>();
        services.AddScoped<IUserDirectory, IdentityUserDirectory>();
        services.AddScoped<IAlertRuleRepository, AlertRuleRepository>();

        return services;
    }

    private static void ValidatePostgresConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection is empty. Configure a PostgreSQL/Npgsql connection string.");
        }

        var sqlServerIndicators = new[]
        {
            "Server=",
            "Data Source=",
            "Initial Catalog=",
            "Trusted_Connection=",
            "Integrated Security=",
            "User ID=",
            "UID="
        };

        if (sqlServerIndicators.Any(indicator => connectionString.Contains(indicator, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "DefaultConnection must use a PostgreSQL/Npgsql connection string. SQL Server or Azure SQL connection strings are not supported by the current infrastructure configuration.");
        }
    }
}
