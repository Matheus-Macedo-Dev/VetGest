using System.ComponentModel.DataAnnotations;

namespace VetGest.Contracts.Push;

public sealed class PushUnsubscribeRequest
{
    [Required, StringLength(2048)]
    public string Endpoint { get; set; } = string.Empty;
}