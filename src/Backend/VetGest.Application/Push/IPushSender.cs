namespace VetGest.Application.Push;

public interface IPushSender
{
    bool Enabled { get; }

    Task SendAsync(string userId, PushMessage message, CancellationToken cancellationToken);
}