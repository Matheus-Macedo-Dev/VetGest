using System.Net.Http.Headers;
using VetGest.Web.Services.Auth;

namespace VetGest.Web.Services.Api;

public sealed class AuthenticatedHttpMessageHandler(IBrowserTokenStore tokenStore) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await tokenStore.GetAsync();
        if (!string.IsNullOrWhiteSpace(token) && request.RequestUri?.IsAbsoluteUri == true &&
            request.RequestUri.AbsolutePath.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}