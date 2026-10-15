namespace VetGest.Contracts.Chat;

/// <summary>
/// Chat message between the two participants of a connection. Messages are kept only for a short retention window.
/// </summary>
public sealed class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid ConnectionId { get; set; }
    public string SenderUserId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; }
}