using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AiCallCenter.Api.Hubs;

[Authorize]
public sealed class CallHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public async Task JoinCompany(string companyId)
    {
        if (!Guid.TryParse(companyId, out var parsedCompanyId))
        {
            throw new HubException(
                "Firma kimliği geçersiz.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetCompanyGroupName(parsedCompanyId));
    }

    public async Task LeaveCompany(string companyId)
    {
        if (!Guid.TryParse(companyId, out var parsedCompanyId))
        {
            throw new HubException(
                "Firma kimliği geçersiz.");
        }

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GetCompanyGroupName(parsedCompanyId));
    }

    public async Task JoinCall(string callId)
    {
        if (!Guid.TryParse(callId, out var parsedCallId))
        {
            throw new HubException(
                "Çağrı kimliği geçersiz.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetCallGroupName(parsedCallId));
    }

    public async Task LeaveCall(string callId)
    {
        if (!Guid.TryParse(callId, out var parsedCallId))
        {
            throw new HubException(
                "Çağrı kimliği geçersiz.");
        }

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GetCallGroupName(parsedCallId));
    }

    public static string GetCompanyGroupName(
        Guid companyId)
    {
        return $"company:{companyId:D}";
    }

    public static string GetCallGroupName(
        Guid callId)
    {
        return $"call:{callId:D}";
    }
}