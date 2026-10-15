using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetGest.API.Authentication;
using VetGest.Application.Pregnancies;
using VetGest.Contracts.Common;
using VetGest.Contracts.Pregnancies;

namespace VetGest.API.Controllers;

[ApiController]
[Route("api/vet/patients")]
[Authorize(Policy = "VetOnly")]
public sealed class VetPatientsController : ControllerBase
{
    private readonly IVetPatientService _service;
    private readonly ICurrentUser _currentUser;

    public VetPatientsController(IVetPatientService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<VetPatientDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<VetPatientDto>>>> List(CancellationToken cancellationToken)
    {
        var patients = await _service.ListAsync(_currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<VetPatientDto>>.Ok(patients));
    }

    [HttpGet("{connectionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<VetPatientDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VetPatientDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<VetPatientDetailDto>>> Get(Guid connectionId, CancellationToken cancellationToken)
    {
        try
        {
            var detail = await _service.GetDetailAsync(connectionId, _currentUser.UserId, cancellationToken);
            return Ok(ApiResponse<VetPatientDetailDto>.Ok(detail));
        }
        catch (VetConnectionNotFoundException)
        {
            return NotFound(ApiResponse<VetPatientDetailDto>.Fail("Paciente não encontrado."));
        }
    }
}
