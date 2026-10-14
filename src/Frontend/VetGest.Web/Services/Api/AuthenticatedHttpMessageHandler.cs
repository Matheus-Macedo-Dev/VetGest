using System.Net.Http.Headers;
using VetGest.Web.Services.Auth;

namespace VetGest.Web.Services.Api;

public sealed class AuthenticatedHttpMessageHandler(IBrowserTokenStore tokenStore) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await tokenStore.GetAsync();
        var requestPath = request.RequestUri is { IsAbsoluteUri: true } absoluteUri
            ? absoluteUri.AbsolutePath
            : request.RequestUri?.OriginalString;

        var isApiRequest = requestPath?.StartsWith("api/", StringComparison.OrdinalIgnoreCase) == true
            || requestPath?.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) == true;

        if (!string.IsNullOrWhiteSpace(token) && isApiRequest)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}