using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetGest.API.Authentication;
using VetGest.Application.Alerts;
using VetGest.Application.Pregnancies;
using VetGest.Contracts.Common;
using VetGest.Contracts.Pregnancies;

namespace VetGest.API.Controllers;

[ApiController]
[Route("api/pets/{petId:guid}/pregnancies")]
[Authorize(Policy = "TutorOnly")]
public sealed class PregnanciesController : ControllerBase
{
    private readonly IPregnancyService _service;
    private readonly IPregnancyCareService _careService;
    private readonly IExaminationReminderService _examinationReminderService;
    private readonly IAlertEvaluationService _alertService;
    private readonly ICurrentUser _currentUser;

    public PregnanciesController(
        IPregnancyService service,
        IPregnancyCareService careService,
        IExaminationReminderService examinationReminderService,
        IAlertEvaluationService alertService,
        ICurrentUser currentUser)
    {
        _service = service;
        _careService = careService;
        _examinationReminderService = examinationReminderService;
        _alertService = alertService;
        _currentUser = currentUser;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PregnancyDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<PregnancyDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PregnancyDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PregnancyDto>>> Create(Guid petId, CreatePregnancyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var pregnancy = await _service.CreateAsync(petId, request, _currentUser.UserId, cancellationToken);
            return CreatedAtAction(nameof(GetCurrent), new { petId }, ApiResponse<PregnancyDto>.Ok(pregnancy));
        }
        catch (PregnancyNotFoundException)
        {
            return NotFound(ApiResponse<PregnancyDto>.Fail("Pet não encontrado."));
        }
        catch (PregnancyCalculationException exception)
        {
            return BadRequest(ApiResponse<PregnancyDto>.ValidationFailed(exception.Errors.ToDictionary(
                pair => pair.Key, pair => pair.Value)));
        }
    }

    [HttpGet("current")]
    [ProducesResponseType(typeof(ApiResponse<PregnancyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PregnancyDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PregnancyDto>>> GetCurrent(Guid petId, CancellationToken cancellationToken)
    {
        var pregnancy = await _service.GetCurrentAsync(petId, _currentUser.UserId, cancellationToken);
        return pregnancy is null
            ? NotFound(ApiResponse<PregnancyDto>.Fail("Gestação não encontrada."))
            : Ok(ApiResponse<PregnancyDto>.Ok(pregnancy));
    }

    [HttpGet("{pregnancyId:guid}/care")]
    [ProducesResponseType(typeof(ApiResponse<CareContentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CareContentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CareContentDto>>> GetCare(
        Guid petId,
        Guid pregnancyId,
        CancellationToken cancellationToken)
    {
        var content = await _careService.GetAsync(petId, pregnancyId, _currentUser.UserId, cancellationToken);
        return content is null
            ? NotFound(ApiResponse<CareContentDto>.Fail("Gestação não encontrada."))
            : Ok(ApiResponse<CareContentDto>.Ok(content));
    }

    [HttpGet("{pregnancyId:guid}/exams")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ExaminationReminderDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ExaminationReminderDto>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ExaminationReminderDto>>>> GetExaminations(
        Guid petId,
        Guid pregnancyId,
        CancellationToken cancellationToken)
    {
        var examinations = await _examinationReminderService.GetAsync(
            petId, pregnancyId, _currentUser.UserId, cancellationToken);
        return examinations is null
            ? NotFound(ApiResponse<IReadOnlyList<ExaminationReminderDto>>.Fail("Gestação não encontrada."))
            : Ok(ApiResponse<IReadOnlyList<ExaminationReminderDto>>.Ok(examinations));
    }

    [HttpGet("{pregnancyId:guid}/alerts")]
    [ProducesResponseType(typeof(ApiResponse<TodayAlertsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TodayAlertsDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TodayAlertsDto>>> GetAlerts(
        Guid petId,
        Guid pregnancyId,
        CancellationToken cancellationToken)
    {
        var alerts = await _alertService.GetCurrentAlertsAsync(petId, pregnancyId, _currentUser.UserId, cancellationToken);
        return alerts is null
            ? NotFound(ApiResponse<TodayAlertsDto>.Fail("Gestação não encontrada."))
            : Ok(ApiResponse<TodayAlertsDto>.Ok(alerts));
    }

}