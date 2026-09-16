namespace VetGest.Web.Services.Auth;

public interface IBrowserTokenStore
{
    ValueTask<string?> GetAsync();
    ValueTask SetAsync(string token);
    ValueTask ClearAsync();
}