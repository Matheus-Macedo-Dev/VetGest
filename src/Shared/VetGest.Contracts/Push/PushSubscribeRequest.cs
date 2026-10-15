using System.ComponentModel.DataAnnotations;

namespace VetGest.Contracts.Push;

/// <summary>
/// Browser Web Push subscription (PushSubscription.toJSON()).
/// </summary>
public sealed class PushSubscribeRequest
{
    [Required, StringLength(2048)]
    public string Endpoint { get; set; } = string.Empty;

    [Required, StringLength(256)]
    public string P256dh { get; set; } = string.Empty;

    [Required, StringLength(128)]
    public string Auth { get; set; } = string.Empty;
}