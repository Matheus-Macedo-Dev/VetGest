namespace VetGest.Contracts.Chat;

public sealed class ChatUnreadDto
{
    public Guid ConnectionId { get; set; }
    public int UnreadCount { get; set; }
}