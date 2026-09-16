using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetGest.Application.Pets;
using VetGest.API.Authentication;
using VetGest.Contracts.Common;
using VetGest.Contracts.Pets;

namespace VetGest.API.Controllers;

[ApiController]
[Route("api/pets")]
[Authorize(Policy = "TutorOnly")]
public sealed class PetsController : ControllerBase
{
    private readonly IPetService _petService;
    private readonly ICurrentUser _currentUser;

    public PetsController(IPetService petService, ICurrentUser currentUser)
    {
        _petService = petService;
        _currentUser = currentUser;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PetDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<PetDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PetDto>>> Create(CreatePetRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var pet = await _petService.CreateAsync(request, _currentUser.UserId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = pet.Id }, ApiResponse<PetDto>.Ok(pet));
        }
        catch (PetValidationException exception)
        {
            return BadRequest(ApiResponse<PetDto>.ValidationFailed(exception.Errors.ToDictionary(
                pair => pair.Key, pair => pair.Value)));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PetDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PetDto>>>> List(CancellationToken cancellationToken)
    {
        var pets = await _petService.ListAsync(_currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PetDto>>.Ok(pets));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PetDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PetDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var pet = await _petService.GetAsync(id, _currentUser.UserId, cancellationToken);
        // A missing or unrelated ID intentionally has the same response to avoid resource enumeration.
        return pet is null
            ? NotFound(ApiResponse<PetDto>.Fail("Pet não encontrado."))
            : Ok(ApiResponse<PetDto>.Ok(pet));
    }

}