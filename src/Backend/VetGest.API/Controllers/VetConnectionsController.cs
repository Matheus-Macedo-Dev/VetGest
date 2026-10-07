using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetGest.API.Authentication;
using VetGest.API.Hubs;
using VetGest.Application.Pregnancies;
using VetGest.Contracts.Common;
using VetGest.Contracts.Pregnancies;

namespace VetGest.API.Controllers;

[ApiController]
[Route("api/vet-connections")]
[Authorize(Policy = "AuthenticatedUser")]
public sealed class VetConnectionsController : ControllerBase
{
    private readonly IVetConnectionService _service;
    private readonly ICurrentUser _currentUser;
    private readonly IVetConnectionRealtimePublisher _realtimePublisher;

    public VetConnectionsController(
        IVetConnectionService service,
        ICurrentUser currentUser,
        IVetConnectionRealtimePublisher realtimePublisher)
    {
        _service = service;
        _currentUser = currentUser;
        _realtimePublisher = realtimePublisher;
    }

    [HttpPost("invitations")]
    [Authorize(Policy = "VetOnly")]
    [ProducesResponseType(typeof(ApiResponse<VetConnectionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<VetConnectionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<VetConnectionDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<VetConnectionDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<VetConnectionDto>>> CreateInvitation(
        CreateVetInvitationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var connection = await _service.CreateInvitationAsync(request, _currentUser.UserId, cancellationToken);
            await _realtimePublisher.PublishConnectionUpdatedAsync(connection, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, ApiResponse<VetConnectionDto>.Ok(connection));
        }
        catch (VetConnectionValidationException exception)
        {
            return BadRequest(ApiResponse<VetConnectionDto>.ValidationFailed(exception.Errors.ToDictionary(pair => pair.Key, pair => pair.Value)));
        }
        catch (VetConnectionNotFoundException)
        {
            return NotFound(ApiResponse<VetConnectionDto>.Fail("Gestação não encontrada."));
        }
        catch (VetInvitationConflictException exception)
        {
            return Conflict(ApiResponse<VetConnectionDto>.Fail(exception.Message));
        }
    }

    [HttpPost("accept")]
    [Authorize(Policy = "TutorOnly")]
    [ProducesResponseType(typeof(ApiResponse<VetConnectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VetConnectionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<VetConnectionDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<VetConnectionDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<VetConnectionDto>>> AcceptInvitation(
        AcceptVetInvitationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var connection = await _service.AcceptInvitationAsync(request, _currentUser.UserId, cancellationToken);
            await _realtimePublisher.PublishConnectionUpdatedAsync(connection, cancellationToken);
            return Ok(ApiResponse<VetConnectionDto>.Ok(connection));
        }
        catch (VetConnectionValidationException exception)
        {
            return BadRequest(ApiResponse<VetConnectionDto>.ValidationFailed(exception.Errors.ToDictionary(pair => pair.Key, pair => pair.Value)));
        }
        catch (VetConnectionNotFoundException)
        {
            return NotFound(ApiResponse<VetConnectionDto>.Fail("Convite não encontrado."));
        }
        catch (VetInvitationConflictException exception)
        {
            return Conflict(ApiResponse<VetConnectionDto>.Fail(exception.Message));
        }
    }

    [HttpPost("{connectionId:guid}/revoke")]
    [ProducesResponseType(typeof(ApiResponse<VetConnectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VetConnectionDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<VetConnectionDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<VetConnectionDto>>> Revoke(
        Guid connectionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var connection = await _service.RevokeAsync(connectionId, _currentUser.UserId, cancellationToken);
            await _realtimePublisher.PublishConnectionUpdatedAsync(connection, cancellationToken);
            return Ok(ApiResponse<VetConnectionDto>.Ok(connection));
        }
        catch (VetConnectionForbiddenException)
        {
            return Forbid();
        }
        catch (VetConnectionNotFoundException)
        {
            return NotFound(ApiResponse<VetConnectionDto>.Fail("Conexão não encontrada."));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<VetConnectionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<VetConnectionDto>>>> List(CancellationToken cancellationToken)
    {
        var connections = await _service.ListForUserAsync(_currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<VetConnectionDto>>.Ok(connections));
    }
}
