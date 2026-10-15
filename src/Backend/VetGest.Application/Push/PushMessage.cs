namespace VetGest.Application.Push;

/// <summary>
/// Push payload. It must never contain private content such as chat text.
/// </summary>
public sealed record PushMessage(string Title, string Body, string Url);