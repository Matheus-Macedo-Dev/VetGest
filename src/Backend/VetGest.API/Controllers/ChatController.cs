using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetGest.API.Authentication;
using VetGest.Application.Chat;
using VetGest.Application.Pregnancies;
using VetGest.Contracts.Chat;
using VetGest.Contracts.Common;

namespace VetGest.API.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize(Policy = "AuthenticatedUser")]
public sealed class ChatController : ControllerBase
{
    private readonly IChatService _service;
    private readonly ICurrentUser _currentUser;

    public ChatController(IChatService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("{connectionId:guid}/messages")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChatMessageDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChatMessageDto>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ChatMessageDto>>>> History(Guid connectionId, CancellationToken cancellationToken)
    {
        try
        {
            var messages = await _service.GetHistoryAsync(connectionId, _currentUser.UserId, cancellationToken);
            return Ok(ApiResponse<IReadOnlyList<ChatMessageDto>>.Ok(messages));
        }
        catch (VetConnectionNotFoundException)
        {
            return NotFound(ApiResponse<IReadOnlyList<ChatMessageDto>>.Fail("Conversa não encontrada."));
        }
    }

    [HttpPost("{connectionId:guid}/read")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> MarkRead(Guid connectionId, CancellationToken cancellationToken)
    {
        try
        {
            await _service.MarkReadAsync(connectionId, _currentUser.UserId, cancellationToken);
            return Ok(ApiResponse<bool>.Ok(true));
        }
        catch (VetConnectionNotFoundException)
        {
            return NotFound(ApiResponse<bool>.Fail("Conversa não encontrada."));
        }
    }

    [HttpGet("unread")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChatUnreadDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ChatUnreadDto>>>> Unread(CancellationToken cancellationToken)
    {
        var unread = await _service.GetUnreadAsync(_currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ChatUnreadDto>>.Ok(unread));
    }
}