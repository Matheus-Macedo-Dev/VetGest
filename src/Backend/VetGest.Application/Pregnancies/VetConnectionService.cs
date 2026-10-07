using VetGest.Contracts.Pregnancies;
using VetGest.Domain.Entities;
using VetGest.Domain.ValueObjects;

namespace VetGest.Application.Pregnancies;

public sealed class VetConnectionService : IVetConnectionService
{
    private const int MaxInvitationGenerationAttempts = 5;
    private static readonly TimeSpan FallbackInvitationTtl = TimeSpan.FromDays(7);
    private static readonly TimeSpan FallbackMaxInvitationTtl = TimeSpan.FromDays(14);

    private readonly IVetConnectionRepository _repository;
    private readonly VetConnectionServiceOptions _options;

    public VetConnectionService(IVetConnectionRepository repository, VetConnectionServiceOptions? options = null)
    {
        _repository = repository;
        _options = options ?? new VetConnectionServiceOptions();
    }

    public async Task<VetConnectionDto> CreateInvitationAsync(
        CreateVetInvitationRequest request,
        string vetUserId,
        CancellationToken cancellationToken)
    {
        EnsureUserId(vetUserId, nameof(vetUserId));
        var errors = ValidateCreateRequest(request, vetUserId);
        if (errors.Count > 0)
            throw new VetConnectionValidationException(errors);

        var pregnancy = await _repository.GetOwnedPregnancyAsync(
            request.PetId,
            request.PregnancyId,
            request.TutorUserId.Trim(),
            cancellationToken);

        if (pregnancy is null || !pregnancy.IsActive())
            throw new VetConnectionNotFoundException();

        var invitationCode = await GenerateUniqueCodeAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var expiresAt = now.Add(ClampInvitationTtl(_options.InvitationTtl, _options.MaxInvitationTtl));
        var connection = new VetConnection(
            Guid.NewGuid(),
            request.PregnancyId,
            vetUserId.Trim(),
            request.TutorUserId.Trim(),
            invitationCode,
            expiresAt);

        await _repository.AddAsync(connection, request.TutorUserId.Trim(), cancellationToken);
        return Map(connection, pregnancy.PetId);
    }

    public async Task<VetConnectionDto> AcceptInvitationAsync(
        AcceptVetInvitationRequest request,
        string tutorUserId,
        CancellationToken cancellationToken)
    {
        EnsureUserId(tutorUserId, nameof(tutorUserId));

        var code = request.InvitationCode?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new VetConnectionValidationException(
                new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
                {
                    [nameof(request.InvitationCode)] = ["O código de convite é obrigatório."]
                });
        }

        var connection = await _repository.GetByInvitationCodeAsync(code, cancellationToken);
        if (connection is null)
            throw new VetConnectionNotFoundException();

        if (!StringComparer.Ordinal.Equals(connection.TutorUserId, tutorUserId.Trim()))
            throw new VetConnectionNotFoundException();

        if (StringComparer.Ordinal.Equals(connection.Status, VetConnectionStatus.Active.Value))
            throw new VetInvitationConflictException("Este convite já foi aceito.");

        if (StringComparer.Ordinal.Equals(connection.Status, VetConnectionStatus.Revoked.Value))
            throw new VetInvitationConflictException("Este convite foi revogado.");

        if (!connection.IsInvitationValid())
            throw new VetInvitationConflictException("Este convite expirou e não pode mais ser aceito.");

        connection.Accept();
        await _repository.SaveChangesAsync(cancellationToken);

        return Map(connection, await ResolvePetIdAsync(connection, cancellationToken));
    }

    public async Task<VetConnectionDto> RevokeAsync(
        Guid connectionId,
        string userId,
        CancellationToken cancellationToken)
    {
        EnsureUserId(userId, nameof(userId));

        var connection = await _repository.GetByIdAsync(connectionId, cancellationToken);
        if (connection is null)
            throw new VetConnectionNotFoundException();

        var normalizedUserId = userId.Trim();
        var isParticipant = StringComparer.Ordinal.Equals(connection.TutorUserId, normalizedUserId)
            || StringComparer.Ordinal.Equals(connection.VetUserId, normalizedUserId);
        if (!isParticipant)
            throw new VetConnectionForbiddenException();

        if (!StringComparer.Ordinal.Equals(connection.Status, VetConnectionStatus.Revoked.Value))
        {
            connection.Revoke();
            await _repository.SaveChangesAsync(cancellationToken);
        }

        return Map(connection, await ResolvePetIdAsync(connection, cancellationToken));
    }

    public async Task<IReadOnlyList<VetConnectionDto>> ListForUserAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        EnsureUserId(userId, nameof(userId));
        var connections = await _repository.ListByUserAsync(userId.Trim(), cancellationToken);
        return connections.Select(connection => Map(connection, connection.Pregnancy.PetId)).ToArray();
    }

    public async Task<bool> CanAccessPregnancyAsync(
        Guid pregnancyId,
        string userId,
        CancellationToken cancellationToken)
    {
        EnsureUserId(userId, nameof(userId));
        if (pregnancyId == Guid.Empty)
            return false;

        return await _repository.CanUserAccessPregnancyAsync(pregnancyId, userId.Trim(), cancellationToken);
    }

    private static Dictionary<string, string[]> ValidateCreateRequest(
        CreateVetInvitationRequest request,
        string vetUserId)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (request.PetId == Guid.Empty)
            errors[nameof(request.PetId)] = ["PetId é obrigatório."];

        if (request.PregnancyId == Guid.Empty)
            errors[nameof(request.PregnancyId)] = ["PregnancyId é obrigatório."];

        if (string.IsNullOrWhiteSpace(request.TutorUserId))
        {
            errors[nameof(request.TutorUserId)] = ["TutorUserId é obrigatório."];
        }
        else if (StringComparer.Ordinal.Equals(vetUserId.Trim(), request.TutorUserId.Trim()))
        {
            errors[nameof(request.TutorUserId)] = ["O veterinário e o tutor não podem ser o mesmo usuário."];
        }

        return errors;
    }

    private async Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxInvitationGenerationAttempts; attempt++)
        {
            var code = VetConnection.GenerateInvitationCode();
            if (!await _repository.InvitationCodeExistsAsync(code, cancellationToken))
                return code;
        }

        throw new VetInvitationConflictException("Não foi possível gerar um código de convite único. Tente novamente.");
    }

    private static TimeSpan ClampInvitationTtl(TimeSpan ttl, TimeSpan maxTtl)
    {
        if (ttl <= TimeSpan.Zero)
            return FallbackInvitationTtl;

        if (maxTtl <= TimeSpan.Zero)
            maxTtl = FallbackMaxInvitationTtl;

        return ttl > maxTtl ? maxTtl : ttl;
    }

    private static VetConnectionDto Map(VetConnection connection, Guid petId) => new()
    {
        Id = connection.Id,
        PetId = petId,
        PregnancyId = connection.PregnancyId,
        VetUserId = connection.VetUserId,
        TutorUserId = connection.TutorUserId,
        InvitationCode = connection.InvitationCode,
        InvitationExpiresAt = connection.InvitationExpiresAt,
        AcceptedAt = connection.AcceptedAt,
        Status = connection.Status
    };

    private static void EnsureUserId(string userId, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("É necessário ter um usuário autenticado.", parameterName);
    }

    private async Task<Guid> ResolvePetIdAsync(VetConnection connection, CancellationToken cancellationToken)
    {
        if (connection.Pregnancy is not null)
            return connection.Pregnancy.PetId;

        var petId = await _repository.GetPetIdByPregnancyIdAsync(connection.PregnancyId, cancellationToken);
        return petId ?? Guid.Empty;
    }
}
