namespace VetGest.Domain.Entities;

/// <summary>
/// A chat message between the two participants of a vet connection. Stored only for a short retention window.
/// </summary>
public class ChatMessage
{
    public const int MaxTextLength = 1000;

    public Guid Id { get; private set; }

    public Guid ConnectionId { get; private set; }

    public string SenderUserId { get; private set; } = string.Empty;

    public string Text { get; private set; } = string.Empty;

    public DateTime SentAtUtc { get; private set; }

    public DateTime? ReadAtUtc { get; private set; }

    private ChatMessage()
    {
    }

    public ChatMessage(Guid id, Guid connectionId, string senderUserId, string text, DateTime sentAtUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Message ID cannot be empty.", nameof(id));

        if (connectionId == Guid.Empty)
            throw new ArgumentException("Connection ID cannot be empty.", nameof(connectionId));

        if (string.IsNullOrWhiteSpace(senderUserId))
            throw new ArgumentException("Sender is required.", nameof(senderUserId));

        if (string.IsNullOrWhiteSpace(text) || text.Length > MaxTextLength)
            throw new ArgumentException($"Text must have 1 to {MaxTextLength} characters.", nameof(text));

        Id = id;
        ConnectionId = connectionId;
        SenderUserId = senderUserId;
        Text = text;
        SentAtUtc = sentAtUtc;
    }
}