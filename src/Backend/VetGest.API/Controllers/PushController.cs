using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetGest.API.Authentication;
using VetGest.Application.Pregnancies;
using VetGest.Application.Push;
using VetGest.Contracts.Common;
using VetGest.Contracts.Push;

namespace VetGest.API.Controllers;

[ApiController]
[Route("api/push")]
[Authorize(Policy = "AuthenticatedUser")]
public sealed class PushController : ControllerBase
{
    private readonly IPushSubscriptionService _service;
    private readonly ICurrentUser _currentUser;

    public PushController(IPushSubscriptionService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("public-key")]
    [ProducesResponseType(typeof(ApiResponse<PushPublicKeyDto>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<PushPublicKeyDto>> PublicKey() =>
        Ok(ApiResponse<PushPublicKeyDto>.Ok(_service.GetPublicKey()));

    [HttpPost("subscribe")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> Subscribe(PushSubscribeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _service.SubscribeAsync(_currentUser.UserId, request, cancellationToken);
            return Ok(ApiResponse<bool>.Ok(true));
        }
        catch (VetConnectionValidationException exception)
        {
            return BadRequest(ApiResponse<bool>.ValidationFailed(exception.Errors.ToDictionary(pair => pair.Key, pair => pair.Value)));
        }
    }

    [HttpPost("unsubscribe")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> Unsubscribe(PushUnsubscribeRequest request, CancellationToken cancellationToken)
    {
        await _service.UnsubscribeAsync(_currentUser.UserId, request, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true));
    }
}