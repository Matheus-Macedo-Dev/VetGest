using VetGest.Contracts.Pregnancies;
using VetGest.Domain.Entities;
using VetGest.Domain.ValueObjects;

namespace VetGest.Application.Pregnancies;

public sealed class VetConnectionService : IVetConnectionService
{
    private const int MaxInvitationGenerationAttempts = 5;
    private const int MaxOpenCodesPerVet = 5;
    private static readonly TimeSpan FallbackInvitationTtl = TimeSpan.FromDays(7);
    private static readonly TimeSpan FallbackMaxInvitationTtl = TimeSpan.FromDays(14);

    private readonly IVetConnectionRepository _repository;
    private readonly VetConnectionServiceOptions _options;
    private readonly IUserDirectory? _userDirectory;

    public VetConnectionService(
        IVetConnectionRepository repository,
        VetConnectionServiceOptions? options = null,
        IUserDirectory? userDirectory = null)
    {
        _repository = repository;
        _options = options ?? new VetConnectionServiceOptions();
        _userDirectory = userDirectory;
    }

    public async Task<VetConnectionDto> CreateCodeAsync(string vetUserId, CancellationToken cancellationToken)
    {
        EnsureUserId(vetUserId, nameof(vetUserId));

        var now = DateTime.UtcNow;
        if (await _repository.CountOpenCodesAsync(vetUserId.Trim(), now, cancellationToken) >= MaxOpenCodesPerVet)
            throw new VetInvitationConflictException("Você já possui muitos códigos em aberto. Aguarde que expirem ou revogue algum antes de gerar outro.");

        var code = await GenerateUniqueCodeAsync(cancellationToken);
        var expiresAt = now.Add(ClampInvitationTtl(_options.InvitationTtl, _options.MaxInvitationTtl));
        var connection = VetConnection.CreateOpenCode(Guid.NewGuid(), vetUserId.Trim(), code, expiresAt);

        await _repository.AddAsync(connection, vetUserId.Trim(), cancellationToken);
        return await WithNamesAsync(Map(connection, Guid.Empty), null, cancellationToken);
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
        return await WithNamesAsync(Map(connection, pregnancy.PetId), pregnancy.Pet?.Name, cancellationToken);
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

        if (connection.TutorUserId is null)
            return await AcceptOpenCodeAsync(connection, request, tutorUserId.Trim(), cancellationToken);

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

        return await WithNamesAsync(
            Map(connection, await ResolvePetIdAsync(connection, cancellationToken)),
            connection.Pregnancy?.Pet?.Name,
            cancellationToken);
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

        return await WithNamesAsync(
            Map(connection, await ResolvePetIdAsync(connection, cancellationToken)),
            connection.Pregnancy?.Pet?.Name,
            cancellationToken);
    }

    public async Task<IReadOnlyList<VetConnectionDto>> ListForUserAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        EnsureUserId(userId, nameof(userId));
        var connections = await _repository.ListByUserAsync(userId.Trim(), cancellationToken);
        var dtos = new List<VetConnectionDto>(connections.Count);
        foreach (var connection in connections)
            dtos.Add(await WithNamesAsync(Map(connection, connection.Pregnancy?.PetId ?? Guid.Empty), connection.Pregnancy?.Pet?.Name, cancellationToken));

        return dtos;
    }

    public async Task<bool> CanChatAsync(Guid connectionId, string userId, CancellationToken cancellationToken)
    {
        EnsureUserId(userId, nameof(userId));
        if (connectionId == Guid.Empty)
            return false;

        var connection = await _repository.GetByIdAsync(connectionId, cancellationToken);
        var normalizedUserId = userId.Trim();
        return connection is not null
            && StringComparer.Ordinal.Equals(connection.Status, VetConnectionStatus.Active.Value)
            && (StringComparer.Ordinal.Equals(connection.VetUserId, normalizedUserId)
                || StringComparer.Ordinal.Equals(connection.TutorUserId, normalizedUserId));
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

    private async Task<VetConnectionDto> AcceptOpenCodeAsync(
        VetConnection connection,
        AcceptVetInvitationRequest request,
        string tutorUserId,
        CancellationToken cancellationToken)
    {
        if (StringComparer.Ordinal.Equals(connection.Status, VetConnectionStatus.Revoked.Value))
            throw new VetInvitationConflictException("Este convite foi revogado.");

        if (!connection.IsInvitationValid())
            throw new VetInvitationConflictException("Este convite expirou e não pode mais ser aceito.");

        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        if (request.PetId is null || request.PetId == Guid.Empty)
            errors[nameof(request.PetId)] = ["Escolha o pet cuja gestação será compartilhada."];

        if (request.PregnancyId is null || request.PregnancyId == Guid.Empty)
            errors[nameof(request.PregnancyId)] = ["Escolha a gestação que será compartilhada."];

        if (StringComparer.Ordinal.Equals(connection.VetUserId, tutorUserId))
            errors[nameof(request.InvitationCode)] = ["O veterinário e o tutor não podem ser o mesmo usuário."];

        if (errors.Count > 0)
            throw new VetConnectionValidationException(errors);

        var pregnancy = await _repository.GetOwnedPregnancyAsync(request.PetId!.Value, request.PregnancyId!.Value, tutorUserId, cancellationToken);
        if (pregnancy is null || !pregnancy.IsActive())
            throw new VetConnectionNotFoundException();

        if (await _repository.HasActiveConnectionAsync(connection.VetUserId, pregnancy.Id, cancellationToken))
            throw new VetInvitationConflictException("Este veterinário já está conectado a esta gestação.");

        connection.AcceptByTutor(tutorUserId, pregnancy.Id);
        await _repository.SaveChangesAsync(cancellationToken);

        return await WithNamesAsync(Map(connection, pregnancy.PetId), pregnancy.Pet?.Name, cancellationToken);
    }

    private async Task<VetConnectionDto> WithNamesAsync(VetConnectionDto dto, string? petName, CancellationToken cancellationToken)
    {
        dto.PetName = petName;
        if (_userDirectory is null)
            return dto;

        var ids = new[] { dto.VetUserId, dto.TutorUserId }.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToArray();
        var names = await _userDirectory.GetDisplayNamesAsync(ids, cancellationToken);
        dto.VetName = names.GetValueOrDefault(dto.VetUserId);
        dto.TutorName = names.GetValueOrDefault(dto.TutorUserId);
        return dto;
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
        PregnancyId = connection.PregnancyId ?? Guid.Empty,
        VetUserId = connection.VetUserId,
        TutorUserId = connection.TutorUserId ?? string.Empty,
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

        if (connection.PregnancyId is null)
            return Guid.Empty;

        var petId = await _repository.GetPetIdByPregnancyIdAsync(connection.PregnancyId.Value, cancellationToken);
        return petId ?? Guid.Empty;
    }
}
