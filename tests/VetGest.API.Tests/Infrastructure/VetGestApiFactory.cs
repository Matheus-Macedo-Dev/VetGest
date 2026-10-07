using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VetGest.API.Hubs;
using VetGest.Application.Pregnancies;

namespace VetGest.API.Tests.Infrastructure;

public sealed class VetGestApiFactory : WebApplicationFactory<Program>
{
    public TestVetConnectionService VetConnectionService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.PostConfigureAll<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                options.DefaultForbidScheme = TestAuthHandler.SchemeName;
            });
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            services.RemoveAll<IVetConnectionService>();
            services.AddSingleton<IVetConnectionService>(VetConnectionService);

            services.RemoveAll<IVetConnectionRealtimePublisher>();
            services.AddSingleton<IVetConnectionRealtimePublisher, NoopVetConnectionRealtimePublisher>();
        });
    }
}
