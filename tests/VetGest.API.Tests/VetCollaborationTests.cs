using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using VetGest.API.Tests.Infrastructure;
using VetGest.Contracts.Chat;
using VetGest.Contracts.Common;
using VetGest.Contracts.Pregnancies;

namespace VetGest.API.Tests;

public sealed class VetCollaborationTests : IClassFixture<VetGestApiFactory>
{
    private readonly VetGestApiFactory _factory;

    public VetCollaborationTests(VetGestApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Create_code_is_vet_only()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await _factory.CreateClient().PostAsync("/api/vet-connections/codes", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await CreateClient("tutor-1", "Tutor").PostAsync("/api/vet-connections/codes", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await CreateClient("admin-1", "Admin").PostAsync("/api/vet-connections/codes", null)).StatusCode);

        var created = await CreateClient("vet-1", "Vet").PostAsync("/api/vet-connections/codes", null);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var envelope = await created.Content.ReadFromJsonAsync<ApiResponse<VetConnectionDto>>();
        Assert.Equal("Pending", envelope!.Data!.Status);
    }

    [Fact]
    public async Task Vet_patients_are_vet_only_and_hide_other_vets_connections()
    {
        var connectionId = Guid.NewGuid();
        _factory.VetPatientService.Allowed.Add((connectionId, "vet-1"));

        Assert.Equal(HttpStatusCode.Unauthorized, (await _factory.CreateClient().GetAsync("/api/vet/patients")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await CreateClient("tutor-1", "Tutor").GetAsync("/api/vet/patients")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await CreateClient("tutor-1", "Tutor").GetAsync($"/api/vet/patients/{connectionId}")).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await CreateClient("vet-1", "Vet").GetAsync("/api/vet/patients")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await CreateClient("vet-1", "Vet").GetAsync($"/api/vet/patients/{connectionId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await CreateClient("vet-2", "Vet").GetAsync($"/api/vet/patients/{connectionId}")).StatusCode);
    }

    [Fact]
    public async Task Chat_rejects_non_participants_and_relays_between_participants()
    {
        var connectionId = Guid.NewGuid();
        _factory.VetConnectionService.AllowedChat.Add((connectionId, "vet-1"));
        _factory.VetConnectionService.AllowedChat.Add((connectionId, "tutor-1"));

        await using var outsider = BuildChat("tutor-9;Tutor");
        await outsider.StartAsync();
        await Assert.ThrowsAsync<HubException>(() => outsider.InvokeAsync("JoinChat", connectionId));
        await Assert.ThrowsAsync<HubException>(() => outsider.InvokeAsync("SendMessage", connectionId, "oi"));

        await using var vet = BuildChat("vet-1;Vet");
        await using var tutor = BuildChat("tutor-1;Tutor");
        var received = new TaskCompletionSource<ChatMessageDto>(TaskCreationOptions.RunContinuationsAsynchronously);
        tutor.On<ChatMessageDto>(ChatHubMethods.MessageReceived, message => received.TrySetResult(message));

        await vet.StartAsync();
        await tutor.StartAsync();
        await vet.InvokeAsync("JoinChat", connectionId);
        await tutor.InvokeAsync("JoinChat", connectionId);

        await Assert.ThrowsAsync<HubException>(() => vet.InvokeAsync("SendMessage", connectionId, "   "));
        await Assert.ThrowsAsync<HubException>(() => vet.InvokeAsync("SendMessage", connectionId, new string('x', ChatHubMethods.MaxMessageLength + 1)));

        await vet.InvokeAsync("SendMessage", connectionId, "  Olá  ");
        var message = await received.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal("Olá", message.Text);
        Assert.Equal("vet-1", message.SenderUserId);
        Assert.Equal("Name vet-1", message.SenderName);
    }

    [Fact]
    public async Task Online_recipient_gets_in_app_notification_and_offline_recipient_gets_push_plus_history()
    {
        var connectionId = Guid.NewGuid();
        _factory.VetConnectionService.AllowedChat.Add((connectionId, "vet-5"));
        _factory.VetConnectionService.AllowedChat.Add((connectionId, "tutor-5"));
        _factory.PushSender.Sent.Clear();

        await using var vet = BuildChat("vet-5;Vet");
        await vet.StartAsync();
        await vet.InvokeAsync("JoinChat", connectionId);

        // Offline recipient: nothing connected for tutor-5 yet, so push is used and the message is stored.
        await vet.InvokeAsync("SendMessage", connectionId, "Você está aí?");
        var push = Assert.Single(_factory.PushSender.Sent);
        Assert.Equal("tutor-5", push.UserId);
        Assert.DoesNotContain("Você está aí?", push.Message.Body);
        Assert.Contains(connectionId.ToString("D"), push.Message.Url);

        var tutorHttp = CreateClient("tutor-5", "Tutor");
        var unread = await tutorHttp.GetFromJsonAsync<ApiResponse<List<ChatUnreadDto>>>("/api/chat/unread");
        Assert.Equal(1, Assert.Single(unread!.Data!).UnreadCount);

        var history = await tutorHttp.GetFromJsonAsync<ApiResponse<List<ChatMessageDto>>>($"/api/chat/{connectionId}/messages");
        Assert.Equal("Você está aí?", Assert.Single(history!.Data!).Text);

        Assert.Equal(HttpStatusCode.OK, (await tutorHttp.PostAsync($"/api/chat/{connectionId}/read", null)).StatusCode);
        var after = await tutorHttp.GetFromJsonAsync<ApiResponse<List<ChatUnreadDto>>>("/api/chat/unread");
        Assert.Empty(after!.Data!);

        // Online recipient (connected, not in the chat group): gets the in-app signal and no push.
        await using var tutor = BuildChat("tutor-5;Tutor");
        var notified = new TaskCompletionSource<ChatNotificationDto>(TaskCreationOptions.RunContinuationsAsynchronously);
        tutor.On<ChatNotificationDto>(ChatHubMethods.NotificationReceived, notification => notified.TrySetResult(notification));
        await tutor.StartAsync();

        _factory.PushSender.Sent.Clear();
        await Task.Delay(500);
        await vet.InvokeAsync("SendMessage", connectionId, "Agora sim");
        var signal = await notified.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal(connectionId, signal.ConnectionId);
        Assert.Equal("Name vet-5", signal.SenderName);
        Assert.Empty(_factory.PushSender.Sent);
    }

    [Fact]
    public async Task Chat_history_is_limited_to_participants()
    {
        var connectionId = Guid.NewGuid();
        _factory.VetConnectionService.AllowedChat.Add((connectionId, "vet-6"));
        _factory.VetConnectionService.AllowedChat.Add((connectionId, "tutor-6"));

        Assert.Equal(HttpStatusCode.Unauthorized, (await _factory.CreateClient().GetAsync($"/api/chat/{connectionId}/messages")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await CreateClient("tutor-9", "Tutor").GetAsync($"/api/chat/{connectionId}/messages")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await CreateClient("tutor-9", "Tutor").PostAsync($"/api/chat/{connectionId}/read", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await CreateClient("vet-6", "Vet").GetAsync($"/api/chat/{connectionId}/messages")).StatusCode);
    }

    private HubConnection BuildChat(string bearerToken) =>
        new HubConnectionBuilder()
            .WithUrl(new Uri(_factory.Server.BaseAddress, "/hubs/chat"), options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
                options.AccessTokenProvider = () => Task.FromResult<string?>(bearerToken);
            })
            .Build();

    private HttpClient CreateClient(string userId, params string[] roles)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", $"{userId};{string.Join(',', roles)}");
        return client;
    }
}
