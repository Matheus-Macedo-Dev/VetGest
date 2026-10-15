namespace VetGest.Application.Push;

/// <summary>
/// VAPID configuration. Push is disabled when the keys are not provided (they must come from secrets/environment, never source control).
/// </summary>
public sealed class PushOptions
{
    public string? PublicKey { get; set; }
    public string? PrivateKey { get; set; }
    public string? Subject { get; set; }

    public bool Enabled =>
        !string.IsNullOrWhiteSpace(PublicKey) && !string.IsNullOrWhiteSpace(PrivateKey) && !string.IsNullOrWhiteSpace(Subject);
}