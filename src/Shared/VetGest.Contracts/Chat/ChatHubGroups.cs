namespace VetGest.Contracts.Chat;

public static class ChatHubGroups
{
    public static string Connection(Guid connectionId) => $"chat:{connectionId:D}";
}
