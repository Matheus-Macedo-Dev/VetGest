namespace VetGest.Contracts.Chat;

public static class ChatHubMethods
{
    public const string MessageReceived = "MessageReceived";
    public const string ChatClosed = "ChatClosed";
    public const string NotificationReceived = "NotificationReceived";
    public const int MaxMessageLength = 1000;
    public const int RetentionHours = 24;
}