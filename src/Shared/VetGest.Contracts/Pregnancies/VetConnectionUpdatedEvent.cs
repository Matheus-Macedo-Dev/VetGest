namespace VetGest.Contracts.Pregnancies;

public sealed class VetConnectionUpdatedEvent
{
    public Guid ConnectionId { get; set; }
    public Guid PetId { get; set; }
    public Guid PregnancyId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
}
