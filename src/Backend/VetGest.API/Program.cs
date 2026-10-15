using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using VetGest.API.Authentication;
using System.Threading.RateLimiting;
using VetGest.API.Admin;
using VetGest.API.Authorization;
using VetGest.Infrastructure;
using VetGest.Infrastructure.Identity;
using VetGest.Infrastructure.Persistence;
using System.Text;
using VetGest.Application.Alerts;
using VetGest.Application.Pets;
using VetGest.Application.Pregnancies;
using VetGest.Application.Content;
using VetGest.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container
var configuration = builder.Configuration;
var allowedCorsOrigins = BuildAllowedCorsOrigins(configuration, builder.Environment.IsDevelopment());

// Add Infrastructure services (EF Core, Database context)
builder.Services.AddInfrastructure(configuration);

// Add Identity services (User management and roles)
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<string>>(options =>
    {
        // Password requirements
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;

        // User requirements
        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    })
    .AddEntityFrameworkStores<VetGestDbContext>()
    .AddErrorDescriber<PortugueseIdentityErrorDescriber>()
    .AddDefaultTokenProviders();

// Add VetGest Authorization services (policies and handlers)
builder.Services.AddVetGestAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IVetConnectionRealtimePublisher, VetConnectionRealtimePublisher>();
builder.Services.AddScoped<RegistrationValidator>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IAdminVetService, AdminVetService>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth-sensitive", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
});
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<PregnancyCalculationPolicy>();
builder.Services.AddScoped<IPregnancyService, PregnancyService>();
builder.Services.AddSingleton<IPhaseCareContentCatalog, PhaseCareContentCatalog>();
builder.Services.AddScoped<IPregnancyCareService, PregnancyCareService>();
builder.Services.AddScoped<IExaminationReminderService, ExaminationReminderService>();
builder.Services.AddScoped<IPregnancyDiaryService, PregnancyDiaryService>();
builder.Services.AddScoped<IAlertEvaluationService, AlertEvaluationService>();
var invitationTtlDays = configuration.GetValue<int?>("VetConnections:InvitationTtlDays") ?? 7;
var maxInvitationTtlDays = configuration.GetValue<int?>("VetConnections:MaxInvitationTtlDays") ?? 14;
builder.Services.AddSingleton(new VetConnectionServiceOptions
{
    InvitationTtl = TimeSpan.FromDays(invitationTtlDays),
    MaxInvitationTtl = TimeSpan.FromDays(maxInvitationTtlDays)
});
builder.Services.AddScoped<IVetPatientService, VetPatientService>();
builder.Services.AddScoped<IVetConnectionService>(serviceProvider => new VetConnectionService(
    serviceProvider.GetRequiredService<IVetConnectionRepository>(),
    serviceProvider.GetRequiredService<VetConnectionServiceOptions>(),
    serviceProvider.GetRequiredService<IUserDirectory>()));

// Register Token Service
var jwtSettings = configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JWT:SecretKey is not configured in appsettings.json");

builder.Services.AddSingleton<ITokenService>(new JwtTokenService(
    secretKey,
    issuer: jwtSettings["Issuer"] ?? "VetGest.API",
    audience: jwtSettings["Audience"] ?? "VetGest.Client"
));

// Add API services
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });
builder.Services.AddOpenApi();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role,
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "VetGest.API",

        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "VetGest.Client",

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // No clock skew tolerance
    };

    // Browser WebSocket/SSE SignalR transports cannot send an Authorization header, so hubs accept the token from the query string.
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                context.Token = accessToken;

            return Task.CompletedTask;
        }
    };

    // Log JWT bearer events in development
    if (builder.Environment.IsDevelopment())
    {
        options.Events.OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            };
        options.Events.OnTokenValidated = context =>
            {
                Console.WriteLine($"Token validated for user: {context.Principal?.FindFirst("email")?.Value}");
                return Task.CompletedTask;
            };
    }
});

// Configure CORS for Blazor WebAssembly frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy
            .WithOrigins(allowedCorsOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add SignalR placeholder (for future Vet connection feature)
builder.Services.AddSignalR();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    // Migrations are opt-in outside Development so a cold start never blocks the listener on the database.
    if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<VetGestDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    // Seed roles after Kestrel is listening so port 8080 opens immediately; retry while the database wakes up.
    app.Lifetime.ApplicationStarted.Register(() => _ = Task.Run(async () =>
    {
        for (var attempt = 1; attempt <= 5; attempt++)
        {
            try
            {
                using var scope = app.Services.CreateScope();
                await RoleSeeder.EnsureRolesAsync(scope.ServiceProvider);
                await AdminSeeder.EnsureAdminAsync(scope.ServiceProvider);
                return;
            }
            catch (Exception ex) when (attempt < 5)
            {
                app.Logger.LogWarning(ex, "Role seeding failed (attempt {Attempt}); retrying.", attempt);
                await Task.Delay(TimeSpan.FromSeconds(5 * attempt));
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "Role seeding failed after {Attempt} attempts.", attempt);
            }
        }
    }));
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
    app.UseHttpsRedirection();
}

app.UseCors("AllowBlazorClient");

// Use authentication and authorization middleware in correct order
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

// Map SignalR hubs (placeholder)
app.MapHub<VetConnectionsHub>("/hubs/vet-connections");
app.MapHub<ChatHub>("/hubs/chat");

app.Run();

static string[] BuildAllowedCorsOrigins(IConfiguration configuration, bool isDevelopment)
{
    var origins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    AddOrigins(origins, configuration.GetSection("Cors:AllowedOrigins").Get<string[]>());
    AddOrigins(origins, SplitOrigins(configuration["Cors:AllowedOrigins"]));
    AddOrigins(origins, SplitOrigins(configuration["Cors:AllowedOriginsCsv"]));

    if (isDevelopment)
    {
        AddOrigins(origins, [
            "https://localhost:7080",
            "http://localhost:5080",
            "http://localhost:3000",
            "http://localhost:5050"
        ]);
    }

    if (origins.Count == 0)
    {
        AddOrigins(origins, ["https://localhost:7080", "http://localhost:5080"]);
    }

    return origins.ToArray();
}

static void AddOrigins(HashSet<string> target, IEnumerable<string>? values)
{
    if (values is null)
    {
        return;
    }

    foreach (var value in values)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            continue;
        }

        var normalized = value.Trim().TrimEnd('/');
        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
        {
            continue;
        }

        if (!uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            && !uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        target.Add($"{uri.Scheme}://{uri.Authority}");
    }
}

static IEnumerable<string> SplitOrigins(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return [];
    }

    return value
        .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}

public partial class Program;
