namespace VetGest.Contracts.Chat;

/// <summary>
/// Lightweight "new message" signal sent to the recipient. It deliberately carries no message text.
/// </summary>
public sealed class ChatNotificationDto
{
    public Guid ConnectionId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; }
}