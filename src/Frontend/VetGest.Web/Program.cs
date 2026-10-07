using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using VetGest.Web.Services.Today;
using VetGest.Web.Services.Api;
using VetGest.Web.Services.Auth;
using VetGest.Web.Services.Realtime;
using VetGest.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<IBrowserTokenStore, SessionTokenStore>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddScoped(sp =>
{
	var baseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
	var handler = new AuthenticatedHttpMessageHandler(sp.GetRequiredService<IBrowserTokenStore>())
	{
		InnerHandler = new HttpClientHandler()
	};

	return new HttpClient(handler)
	{
		BaseAddress = new Uri(baseUrl, UriKind.Absolute)
	};
});
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PetService>();
builder.Services.AddScoped<PregnancyService>();
builder.Services.AddScoped<CareContentService>();
builder.Services.AddScoped<AlertService>();
builder.Services.AddScoped<ExaminationService>();
builder.Services.AddScoped<DiaryService>();
builder.Services.AddScoped<VetConnectionService>();
builder.Services.AddScoped<VetConnectionRealtimeService>();
builder.Services.AddScoped<LiveTodayService>();
builder.Services.AddScoped<ITodayService, LiveTodayService>();

await builder.Build().RunAsync();
