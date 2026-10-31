namespace AiCallCenter.Application.Common.Realtime;

public interface ICallRealtimeNotifier
{
    Task MessageCreatedAsync(
        CallRealtimeMessage message,
        CancellationToken cancellationToken = default);

    Task CallStatusChangedAsync(
        CallStatusChangedNotification notification,
        CancellationToken cancellationToken = default);
}