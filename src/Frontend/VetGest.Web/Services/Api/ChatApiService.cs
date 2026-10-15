using VetGest.Contracts.Chat;

namespace VetGest.Web.Services.Api;

public sealed class ChatApiService(HttpClient httpClient, IApiWakeUpService wakeUpService)
    : ApiClient(httpClient, wakeUpService)
{
    public Task<ApiCallResult<IReadOnlyList<ChatMessageDto>>> GetHistoryAsync(Guid connectionId, CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<ChatMessageDto>>(new HttpRequestMessage(HttpMethod.Get, $"api/chat/{connectionId}/messages"), cancellationToken);

    public async Task<bool> MarkReadAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/chat/{connectionId}/read");
        var result = await SendAsync<bool>(message, cancellationToken);
        return result.Succeeded;
    }

    public Task<ApiCallResult<IReadOnlyList<ChatUnreadDto>>> GetUnreadAsync(CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<ChatUnreadDto>>(new HttpRequestMessage(HttpMethod.Get, "api/chat/unread"), cancellationToken);
}