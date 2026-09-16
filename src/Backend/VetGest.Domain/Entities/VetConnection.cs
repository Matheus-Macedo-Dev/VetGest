using VetGest.Domain.ValueObjects;

namespace VetGest.Domain.Entities;

/// <summary>
/// VetConnection entity. Represents a connection between a Tutor and a Vet
/// for collaboration on a specific pregnancy.
/// Uses invitation codes for secure pairing and includes expiry validation.
/// </summary>
public class VetConnection
{
    /// <summary>
    /// Unique identifier (GUID).
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Foreign key to the Pregnancy this connection relates to.
    /// </summary>
    public Guid PregnancyId { get; private set; }

    /// <summary>
    /// Navigation property to the Pregnancy.
    /// </summary>
    public Pregnancy Pregnancy { get; private set; } = null!;

    /// <summary>
    /// User ID of the veterinarian (the user with Vet role).
    /// </summary>
    public string VetUserId { get; private set; } = string.Empty;

    /// <summary>
    /// User ID of the tutor (the pet owner, user with Tutor role).
    /// </summary>
    public string TutorUserId { get; private set; } = string.Empty;

    /// <summary>
    /// Unique invitation code for the vet to accept the connection.
    /// Single-use, case-insensitive.
    /// </summary>
    public string InvitationCode { get; private set; } = string.Empty;

    /// <summary>
    /// UTC date and time when the invitation expires.
    /// Typically 7-14 days from creation.
    /// </summary>
    public DateTime InvitationExpiresAt { get; private set; }

    /// <summary>
    /// UTC date and time when the vet accepted the invitation (null if pending).
    /// </summary>
    public DateTime? AcceptedAt { get; private set; }

    /// <summary>
    /// Connection status: Pending, Active, Revoked.
    /// </summary>
    public string Status { get; private set; } = VetConnectionStatus.Pending.Value;

    /// <summary>
    /// Shadow property: CreatedAt (audit trail).
    /// </summary>

    /// <summary>
    /// Shadow property: ModifiedAt (audit trail).
    /// </summary>

    /// <summary>
    /// Creates a new VetConnection instance with an auto-generated invitation code.
    /// </summary>
    /// <param name="id">Connection unique identifier.</param>
    /// <param name="pregnancyId">ID of the pregnancy.</param>
    /// <param name="vetUserId">User ID of the vet.</param>
    /// <param name="tutorUserId">User ID of the tutor.</param>
    /// <param name="invitationCode">Unique invitation code.</param>
    /// <param name="invitationExpiresAt">Invitation expiry date/time.</param>
    public VetConnection(
        Guid id,
        Guid pregnancyId,
        string vetUserId,
        string tutorUserId,
        string invitationCode,
        DateTime invitationExpiresAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("VetConnection ID cannot be empty.", nameof(id));

        if (pregnancyId == Guid.Empty)
            throw new ArgumentException("Pregnancy ID cannot be empty.", nameof(pregnancyId));

        if (string.IsNullOrWhiteSpace(vetUserId))
            throw new ArgumentException("Vet user ID is required.", nameof(vetUserId));

        if (string.IsNullOrWhiteSpace(tutorUserId))
            throw new ArgumentException("Tutor user ID is required.", nameof(tutorUserId));

        if (string.IsNullOrWhiteSpace(invitationCode))
            throw new ArgumentException("Invitation code is required.", nameof(invitationCode));

        if (vetUserId == tutorUserId)
            throw new ArgumentException("Vet and Tutor must be different users.", nameof(vetUserId));

        if (invitationExpiresAt.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Invitation expiry must be in UTC.", nameof(invitationExpiresAt));

        if (invitationExpiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Invitation expiry must be in the future.", nameof(invitationExpiresAt));

        Id = id;
        PregnancyId = pregnancyId;
        VetUserId = vetUserId;
        TutorUserId = tutorUserId;
        InvitationCode = invitationCode;
        InvitationExpiresAt = invitationExpiresAt;
    }

    /// <summary>
    /// Checks if the invitation is still valid (not expired and status is Pending).
    /// </summary>
    public bool IsInvitationValid()
    {
        return Status == VetConnectionStatus.Pending.Value
            && DateTime.UtcNow <= InvitationExpiresAt;
    }

    /// <summary>
    /// Accepts the invitation and activates the vet connection.
    /// Can only be called if the invitation is still valid.
    /// </summary>
    public void Accept()
    {
        if (!IsInvitationValid())
            throw new InvalidOperationException(
                "Cannot accept invitation: it has expired or is no longer pending.");

        AcceptedAt = DateTime.UtcNow;
        Status = VetConnectionStatus.Active.Value;
    }

    /// <summary>
    /// Revokes the vet connection (tutor or vet can revoke).
    /// </summary>
    public void Revoke()
    {
        Status = VetConnectionStatus.Revoked.Value;
    }

    /// <summary>
    /// Generates a random invitation code.
    /// </summary>
    /// <returns>A 12-character alphanumeric code.</returns>
    public static string GenerateInvitationCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var code = new string(Enumerable.Range(0, 12)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
        return code;
    }
}
