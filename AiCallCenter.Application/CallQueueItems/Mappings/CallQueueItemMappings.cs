using AiCallCenter.Application.CallQueueItems.Dtos;
using AiCallCenter.Domain.Entities;

namespace AiCallCenter.Application.CallQueueItems.Mappings;

public static class CallQueueItemMappings
{
    public static CallQueueItemListDto ToListDto(this CallQueueItem item)
    {
        var now = DateTime.UtcNow;

        var waitingEnd =
            item.AssignedAt ??
            item.CancelledAt ??
            item.CompletedAt ??
            now;

        var waitingSeconds = Math.Max(
            0,
            (int)(waitingEnd - item.WaitingSince).TotalSeconds);

        return new CallQueueItemListDto(
            item.Id,
            item.CompanyId,
            item.CallQueueId,
            item.QueueName,
            item.CallId,
            item.Call.CallerNumber,
            item.Call.CalledNumber,
            item.AssignedOperatorId,
            item.AssignedOperator?.DisplayName,
            item.AssignedOperator?.ExtensionNumber,
            item.Status,
            item.Priority,
            item.TransferReason,
            item.AiHandoffSummary,
            item.WaitingSince,
            item.AssignedAt,
            item.AcceptedAt,
            item.CompletedAt,
            item.CancelledAt,
            item.ExpiresAt,
            item.IsExpired(),
            waitingSeconds);
    }
}