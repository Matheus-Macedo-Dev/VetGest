namespace VetGest.Application.Chat;

public sealed class ChatServiceOptions
{
    /// <summary>How long chat messages are kept and visible.</summary>
    public TimeSpan Retention { get; set; } = TimeSpan.FromHours(24);
}