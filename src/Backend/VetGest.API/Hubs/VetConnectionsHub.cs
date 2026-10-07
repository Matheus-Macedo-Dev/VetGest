using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using VetGest.Application.Pregnancies;
using VetGest.Contracts.Pregnancies;

namespace VetGest.API.Hubs;

[Authorize(Policy = "AuthenticatedUser")]
public sealed class VetConnectionsHub : Hub
{
    private readonly IVetConnectionService _service;

    public VetConnectionsHub(IVetConnectionService service)
    {
        _service = service;
    }

    public async Task JoinPregnancy(Guid pregnancyId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            throw new HubException("Acesso não autorizado para este acompanhamento.");

        if (!await _service.CanAccessPregnancyAsync(pregnancyId, userId, Context.ConnectionAborted))
            throw new HubException("Acesso não autorizado para este acompanhamento.");

        await Groups.AddToGroupAsync(Context.ConnectionId, VetConnectionHubGroups.Pregnancy(pregnancyId));
    }

    public async Task LeavePregnancy(Guid pregnancyId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, VetConnectionHubGroups.Pregnancy(pregnancyId));
    }
}
