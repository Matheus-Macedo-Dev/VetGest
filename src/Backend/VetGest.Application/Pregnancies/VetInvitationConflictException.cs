namespace VetGest.Application.Pregnancies;

public sealed class VetInvitationConflictException : Exception
{
    public VetInvitationConflictException(string message)
        : base(message)
    {
    }
}
