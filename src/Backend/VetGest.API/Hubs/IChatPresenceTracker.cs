namespace VetGest.API.Hubs;

/// <summary>
/// Tracks which users currently hold a live chat hub connection (single API instance).
/// </summary>
public interface IChatPresenceTracker
{
    void Connected(string userId);

    void Disconnected(string userId);

    bool IsOnline(string userId);
}
