namespace VetGest.Contracts.Chat;

/// <summary>
/// Live chat message. Messages are relayed only and never stored.
/// </summary>
public sealed class ChatMessageDto
{
    public Guid ConnectionId { get; set; }
    public string SenderUserId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; }
}
