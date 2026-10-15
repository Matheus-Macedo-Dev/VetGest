using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetGest.API.Admin;
using VetGest.Contracts.Admin;
using VetGest.Contracts.Common;

namespace VetGest.API.Controllers;

[ApiController]
[Route("api/admin/tutors")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminTutorsController : ControllerBase
{
    private readonly IAdminTutorService _service;

    public AdminTutorsController(IAdminTutorService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminTutorDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminTutorDto>>>> List(CancellationToken cancellationToken)
    {
        var result = await _service.ListTutorsAsync(cancellationToken);
        return StatusCode(result.StatusCode, result.Response);
    }

    [HttpPost("{tutorUserId}/activate")]
    [ProducesResponseType(typeof(ApiResponse<AdminTutorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AdminTutorDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminTutorDto>>> Activate(string tutorUserId, CancellationToken cancellationToken)
    {
        var result = await _service.SetActiveAsync(tutorUserId, true, cancellationToken);
        return StatusCode(result.StatusCode, result.Response);
    }

    [HttpPost("{tutorUserId}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<AdminTutorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AdminTutorDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminTutorDto>>> Deactivate(string tutorUserId, CancellationToken cancellationToken)
    {
        var result = await _service.SetActiveAsync(tutorUserId, false, cancellationToken);
        return StatusCode(result.StatusCode, result.Response);
    }
}
