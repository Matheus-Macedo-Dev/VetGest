namespace VetGest.Application.Pregnancies;

public sealed class VetConnectionServiceOptions
{
    public TimeSpan InvitationTtl { get; init; } = TimeSpan.FromDays(7);
    public TimeSpan MaxInvitationTtl { get; init; } = TimeSpan.FromDays(14);
}
