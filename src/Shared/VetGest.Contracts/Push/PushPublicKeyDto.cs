namespace VetGest.Contracts.Push;

/// <summary>
/// VAPID public key. Empty when push is not configured on the server.
/// </summary>
public sealed class PushPublicKeyDto
{
    public string PublicKey { get; set; } = string.Empty;
    public bool Enabled { get; set; }
}