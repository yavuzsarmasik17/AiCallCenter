using AiCallCenter.Api.Hubs;
using AiCallCenter.Application.Common.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace AiCallCenter.Api.Realtime;

public sealed class SignalRCallRealtimeNotifier
    : ICallRealtimeNotifier
{
    private readonly IHubContext<CallHub> _hubContext;

    public SignalRCallRealtimeNotifier(
        IHubContext<CallHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task MessageCreatedAsync(
        CallRealtimeMessage message,
        CancellationToken cancellationToken = default)
    {
        var companyGroup =
            CallHub.GetCompanyGroupName(
                message.CompanyId);

        var callGroup =
            CallHub.GetCallGroupName(
                message.CallId);

        await _hubContext.Clients
            .Group(companyGroup)
            .SendAsync(
                "CallMessageCreated",
                message,
                cancellationToken);

        await _hubContext.Clients
            .Group(callGroup)
            .SendAsync(
                "CallMessageCreated",
                message,
                cancellationToken);
    }

    public async Task CallStatusChangedAsync(
        CallStatusChangedNotification notification,
        CancellationToken cancellationToken = default)
    {
        var companyGroup =
            CallHub.GetCompanyGroupName(
                notification.CompanyId);

        var callGroup =
            CallHub.GetCallGroupName(
                notification.CallId);

        await _hubContext.Clients
            .Group(companyGroup)
            .SendAsync(
                "CallStatusChanged",
                notification,
                cancellationToken);

        await _hubContext.Clients
            .Group(callGroup)
            .SendAsync(
                "CallStatusChanged",
                notification,
                cancellationToken);
    }
}