namespace VetGest.Domain.Entities;

/// <summary>
/// A browser Web Push subscription owned by one user.
/// </summary>
public class UserPushSubscription
{
    public Guid Id { get; private set; }

    public string UserId { get; private set; } = string.Empty;

    public string Endpoint { get; private set; } = string.Empty;

    public string P256dh { get; private set; } = string.Empty;

    public string Auth { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    private UserPushSubscription()
    {
    }

    public UserPushSubscription(Guid id, string userId, string endpoint, string p256dh, string auth, DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(endpoint)
            || string.IsNullOrWhiteSpace(p256dh) || string.IsNullOrWhiteSpace(auth))
            throw new ArgumentException("User, endpoint and keys are required.");

        Id = id;
        UserId = userId;
        Endpoint = endpoint;
        P256dh = p256dh;
        Auth = auth;
        CreatedAtUtc = createdAtUtc;
    }

    public void Reassign(string userId, string p256dh, string auth)
    {
        UserId = userId;
        P256dh = p256dh;
        Auth = auth;
    }
}