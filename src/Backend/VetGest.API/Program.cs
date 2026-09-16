using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VetGest.API.Authentication;
using VetGest.API.Authorization;
using VetGest.Infrastructure;
using VetGest.Infrastructure.Identity;
using VetGest.Infrastructure.Persistence;
using System.Text;
using VetGest.Application.Alerts;
using VetGest.Application.Pets;
using VetGest.Application.Pregnancies;
using VetGest.Application.Content;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
var configuration = builder.Configuration;

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
builder.Services.AddScoped<RegistrationValidator>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<PregnancyCalculationPolicy>();
builder.Services.AddScoped<IPregnancyService, PregnancyService>();
builder.Services.AddSingleton<IPhaseCareContentCatalog, PhaseCareContentCatalog>();
builder.Services.AddScoped<IPregnancyCareService, PregnancyCareService>();
builder.Services.AddScoped<IExaminationReminderService, ExaminationReminderService>();
builder.Services.AddScoped<IPregnancyDiaryService, PregnancyDiaryService>();
builder.Services.AddScoped<IAlertEvaluationService, AlertEvaluationService>();

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
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "VetGest.API",

        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "VetGest.Client",

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // No clock skew tolerance
    };

    // Log JWT bearer events in development
    if (builder.Environment.IsDevelopment())
    {
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"Token validated for user: {context.Principal?.FindFirst("email")?.Value}");
                return Task.CompletedTask;
            }
        };
    }
});

// Configure CORS for Blazor WebAssembly frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy
            .WithOrigins("https://localhost:7080", "http://localhost:5080")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();

        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5050");
        }
    });
});

// Add SignalR placeholder (for future Vet connection feature)
builder.Services.AddSignalR();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    if (app.Environment.IsDevelopment())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<VetGestDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    await RoleSeeder.EnsureRolesAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowBlazorClient");

// Use authentication and authorization middleware in correct order
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

// Map SignalR hubs (placeholder)
// app.MapHub<VetConnectionHub>("/hubs/vet-connection");

app.Run();
