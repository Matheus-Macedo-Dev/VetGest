using VetGest.Application.Chat;
using VetGest.Application.Pregnancies;
using VetGest.Application.Push;
using VetGest.Contracts.Chat;
using VetGest.Domain.Entities;

namespace VetGest.Application.Tests;

public sealed class ChatServiceTests
{
    [Fact]
    public async Task Participants_of_an_active_connection_can_send_and_read_history()
    {
        var (service, _, connection) = CreateActiveConnection();

        var sent = await service.SendAsync(connection.Id, "vet-1", "  Olá  ", CancellationToken.None);

        Assert.Equal("Olá", sent.Message.Text);
        Assert.Equal("tutor-1", sent.RecipientUserId);
        Assert.Equal("Name vet-1", sent.Message.SenderName);

        var reply = await service.SendAsync(connection.Id, "tutor-1", "Oi doutor", CancellationToken.None);
        Assert.Equal("vet-1", reply.RecipientUserId);

        var history = await service.GetHistoryAsync(connection.Id, "tutor-1", CancellationToken.None);
        Assert.Equal(["Olá", "Oi doutor"], history.Select(message => message.Text));
    }

    [Fact]
    public async Task Outsiders_and_inactive_connections_are_rejected()
    {
        var (service, repository, connection) = CreateActiveConnection();

        await Assert.ThrowsAsync<VetConnectionNotFoundException>(() => service.SendAsync(connection.Id, "intruder", "oi", CancellationToken.None));
        await Assert.ThrowsAsync<VetConnectionNotFoundException>(() => service.GetHistoryAsync(connection.Id, "intruder", CancellationToken.None));
        await Assert.ThrowsAsync<VetConnectionNotFoundException>(() => service.MarkReadAsync(connection.Id, "intruder", CancellationToken.None));
        await Assert.ThrowsAsync<VetConnectionNotFoundException>(() => service.SendAsync(Guid.NewGuid(), "vet-1", "oi", CancellationToken.None));

        connection.Revoke();
        await Assert.ThrowsAsync<VetConnectionNotFoundException>(() => service.SendAsync(connection.Id, "vet-1", "oi", CancellationToken.None));
        await Assert.ThrowsAsync<VetConnectionNotFoundException>(() => service.GetHistoryAsync(connection.Id, "tutor-1", CancellationToken.None));
        Assert.Empty(repository.Messages);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Empty_messages_are_rejected(string text)
    {
        var (service, _, connection) = CreateActiveConnection();

        await Assert.ThrowsAsync<VetConnectionValidationException>(() => service.SendAsync(connection.Id, "vet-1", text, CancellationToken.None));
    }

    [Fact]
    public async Task Too_long_messages_are_rejected()
    {
        var (service, _, connection) = CreateActiveConnection();

        await Assert.ThrowsAsync<VetConnectionValidationException>(() =>
            service.SendAsync(connection.Id, "vet-1", new string('x', ChatMessage.MaxTextLength + 1), CancellationToken.None));
    }

    [Fact]
    public async Task History_hides_messages_outside_the_retention_window()
    {
        var (service, repository, connection) = CreateActiveConnection();
        await repository.AddAsync(
            new ChatMessage(Guid.NewGuid(), connection.Id, "vet-1", "antiga", DateTime.UtcNow.AddHours(-25)), CancellationToken.None);
        await service.SendAsync(connection.Id, "vet-1", "recente", CancellationToken.None);

        var history = await service.GetHistoryAsync(connection.Id, "tutor-1", CancellationToken.None);

        Assert.Equal("recente", Assert.Single(history).Text);
    }

    [Fact]
    public async Task Only_the_recipient_has_unread_messages_and_marking_read_clears_them()
    {
        var (service, _, connection) = CreateActiveConnection();
        await service.SendAsync(connection.Id, "vet-1", "um", CancellationToken.None);
        await service.SendAsync(connection.Id, "vet-1", "dois", CancellationToken.None);

        Assert.Empty(await service.GetUnreadAsync("vet-1", CancellationToken.None));
        Assert.Equal(2, Assert.Single(await service.GetUnreadAsync("tutor-1", CancellationToken.None)).UnreadCount);

        await service.MarkReadAsync(connection.Id, "vet-1", CancellationToken.None);
        Assert.Equal(2, Assert.Single(await service.GetUnreadAsync("tutor-1", CancellationToken.None)).UnreadCount);

        await service.MarkReadAsync(connection.Id, "tutor-1", CancellationToken.None);
        Assert.Empty(await service.GetUnreadAsync("tutor-1", CancellationToken.None));
    }

    [Theory]
    [InlineData("https://fcm.googleapis.com/fcm/send/abc", true)]
    [InlineData("https://updates.push.services.mozilla.com/wpush/v2/abc", true)]
    [InlineData("https://web.push.apple.com/abc", true)]
    [InlineData("https://wns2-par02p.notify.windows.com/w/?token=abc", true)]
    [InlineData("http://fcm.googleapis.com/fcm/send/abc", false)]
    [InlineData("https://evil.example.com/fcm.googleapis.com", false)]
    [InlineData("https://fcm.googleapis.com.evil.example.com/x", false)]
    [InlineData("https://169.254.169.254/latest/meta-data", false)]
    [InlineData("https://localhost/push", false)]
    [InlineData("https://fcm.googleapis.com:8443/x", false)]
    [InlineData("not a url", false)]
    [InlineData("", false)]
    public void Push_endpoint_policy_only_allows_known_browser_push_services(string endpoint, bool expected)
    {
        Assert.Equal(expected, PushEndpointPolicy.IsAllowed(endpoint));
    }

    private static (ChatService Service, FakeChatRepository Chat, VetConnection Connection) CreateActiveConnection()
    {
        var pregnancy = new Pregnancy(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.Date.AddDays(50), DateTime.UtcNow.Date.AddDays(-10));
        var connection = VetConnection.CreateOpenCode(Guid.NewGuid(), "vet-1", "CHATCODE0001", DateTime.UtcNow.AddDays(1));
        connection.AcceptByTutor("tutor-1", pregnancy.Id);

        var connectionRepository = new VetConnectionServiceTests.FakeRepository(pregnancy, "tutor-1") { Connection = connection };
        var chatRepository = new FakeChatRepository(connection);
        return (new ChatService(chatRepository, connectionRepository, new FakeUserDirectory()), chatRepository, connection);
    }

    private sealed class FakeUserDirectory : IUserDirectory
    {
        public Task<IReadOnlyDictionary<string, string>> GetDisplayNamesAsync(IReadOnlyCollection<string> userIds, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyDictionary<string, string>>(userIds.ToDictionary(id => id, id => $"Name {id}"));
    }

    private sealed class FakeChatRepository(VetConnection connection) : IChatRepository
    {
        public List<ChatMessage> Messages { get; } = [];

        public Task AddAsync(ChatMessage message, CancellationToken cancellationToken)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ChatMessage>> ListAsync(Guid connectionId, DateTime sinceUtc, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ChatMessage>>(Messages.Where(m => m.ConnectionId == connectionId && m.SentAtUtc >= sinceUtc).OrderBy(m => m.SentAtUtc).ToList());

        public Task MarkReadAsync(Guid connectionId, string readerUserId, DateTime sinceUtc, DateTime nowUtc, CancellationToken cancellationToken)
        {
            foreach (var message in Messages.Where(m => m.ConnectionId == connectionId && m.SenderUserId != readerUserId && m.ReadAtUtc is null))
                typeof(ChatMessage).GetProperty(nameof(ChatMessage.ReadAtUtc))!.SetValue(message, nowUtc);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ChatUnreadDto>> GetUnreadAsync(string userId, DateTime sinceUtc, CancellationToken cancellationToken)
        {
            var isParticipant = connection.VetUserId == userId || connection.TutorUserId == userId;
            var count = isParticipant
                ? Messages.Count(m => m.SenderUserId != userId && m.ReadAtUtc is null && m.SentAtUtc >= sinceUtc)
                : 0;
            IReadOnlyList<ChatUnreadDto> result = count == 0 ? [] : [new ChatUnreadDto { ConnectionId = connection.Id, UnreadCount = count }];
            return Task.FromResult(result);
        }

        public Task<int> DeleteExpiredAsync(DateTime cutoffUtc, CancellationToken cancellationToken) => Task.FromResult(0);
    }
}