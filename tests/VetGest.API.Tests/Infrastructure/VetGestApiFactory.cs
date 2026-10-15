using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VetGest.API.Admin;
using VetGest.API.Hubs;
using VetGest.Application.Chat;
using VetGest.Application.Pregnancies;
using VetGest.Application.Push;

namespace VetGest.API.Tests.Infrastructure;

public sealed class VetGestApiFactory : WebApplicationFactory<Program>
{
    public TestVetConnectionService VetConnectionService { get; } = new();
    public TestVetPatientService VetPatientService { get; } = new();
    public TestChatService ChatService { get; }
    public TestPushSender PushSender { get; } = new();

    public VetGestApiFactory() => ChatService = new TestChatService(VetConnectionService);

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

            services.RemoveAll<IVetPatientService>();
            services.AddSingleton<IVetPatientService>(VetPatientService);

            services.RemoveAll<IUserDirectory>();
            services.AddSingleton<IUserDirectory, TestUserDirectory>();

            services.RemoveAll<IChatService>();
            services.AddSingleton<IChatService>(ChatService);

            services.RemoveAll<IPushSender>();
            services.AddSingleton<IPushSender>(PushSender);

            services.RemoveAll<IAdminTutorService>();
            services.AddSingleton<IAdminTutorService, TestAdminTutorService>();

            services.RemoveAll<IAdminVetService>();
            services.AddSingleton<IAdminVetService, TestAdminVetService>();
        });
    }
}
