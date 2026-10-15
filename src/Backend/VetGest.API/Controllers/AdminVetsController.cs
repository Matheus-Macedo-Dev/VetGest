using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetGest.API.Admin;
using VetGest.API.Authentication;
using VetGest.Contracts.Admin;
using VetGest.Contracts.Common;

namespace VetGest.API.Controllers;

[ApiController]
[Route("api/admin/vets")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminVetsController : ControllerBase
{
    private readonly IAdminVetService _service;
    private readonly ICurrentUser _currentUser;

    public AdminVetsController(IAdminVetService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreatedVetDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CreatedVetDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CreatedVetDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<CreatedVetDto>>> Create(CreateVetRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateVetAsync(request, _currentUser.UserId, cancellationToken);
        return StatusCode(result.StatusCode, result.Response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminVetDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminVetDto>>>> List(CancellationToken cancellationToken)
    {
        var result = await _service.ListVetsAsync(cancellationToken);
        return StatusCode(result.StatusCode, result.Response);
    }

    [HttpPost("{vetUserId}/setup-link")]
    [ProducesResponseType(typeof(ApiResponse<VetSetupLinkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VetSetupLinkDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<VetSetupLinkDto>>> IssueSetupLink(string vetUserId, CancellationToken cancellationToken)
    {
        var result = await _service.IssueSetupTokenAsync(vetUserId, cancellationToken);
        return StatusCode(result.StatusCode, result.Response);
    }

    [HttpPost("{vetUserId}/activate")]
    [ProducesResponseType(typeof(ApiResponse<AdminVetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AdminVetDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminVetDto>>> Activate(string vetUserId, CancellationToken cancellationToken)
    {
        var result = await _service.SetActiveAsync(vetUserId, true, cancellationToken);
        return StatusCode(result.StatusCode, result.Response);
    }

    [HttpPost("{vetUserId}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<AdminVetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AdminVetDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminVetDto>>> Deactivate(string vetUserId, CancellationToken cancellationToken)
    {
        var result = await _service.SetActiveAsync(vetUserId, false, cancellationToken);
        return StatusCode(result.StatusCode, result.Response);
    }
}
