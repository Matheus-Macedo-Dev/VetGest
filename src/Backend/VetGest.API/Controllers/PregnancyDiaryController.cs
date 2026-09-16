using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetGest.API.Authentication;
using VetGest.Application.Pregnancies;
using VetGest.Contracts.Common;
using VetGest.Contracts.Pregnancies;

namespace VetGest.API.Controllers;

[ApiController]
[Route("api/pets/{petId:guid}/pregnancies/{pregnancyId:guid}/diary")]
[Authorize(Policy = "TutorOnly")]
public sealed class PregnancyDiaryController : ControllerBase
{
    private readonly IPregnancyDiaryService _service;
    private readonly ICurrentUser _currentUser;

    public PregnancyDiaryController(IPregnancyDiaryService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DiaryEntryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<DiaryEntryDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<DiaryEntryDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<DiaryEntryDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<DiaryEntryDto>>> Create(Guid petId, Guid pregnancyId, CreateDiaryEntryRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var entry = await _service.CreateAsync(petId, pregnancyId, request, _currentUser.UserId, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, ApiResponse<DiaryEntryDto>.Ok(entry));
        }
        catch (DiaryValidationException exception)
        {
            return BadRequest(ApiResponse<DiaryEntryDto>.ValidationFailed(exception.Errors.ToDictionary(pair => pair.Key, pair => pair.Value)));
        }
        catch (DiaryParentNotFoundException)
        {
            return NotFound(ApiResponse<DiaryEntryDto>.Fail("Gestação não encontrada."));
        }
        catch (DiaryDuplicateDateException)
        {
            return Conflict(ApiResponse<DiaryEntryDto>.Fail("Já existe uma observação registrada para esta gestação nessa data."));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DiaryEntryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DiaryEntryDto>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DiaryEntryDto>>>> List(Guid petId, Guid pregnancyId, CancellationToken cancellationToken)
    {
        var entries = await _service.ListAsync(petId, pregnancyId, _currentUser.UserId, cancellationToken);
        return entries is null
            ? NotFound(ApiResponse<IReadOnlyList<DiaryEntryDto>>.Fail("Gestação não encontrada."))
            : Ok(ApiResponse<IReadOnlyList<DiaryEntryDto>>.Ok(entries));
    }

}