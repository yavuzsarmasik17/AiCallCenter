using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.CallQueueItems.Dtos;

public sealed record CallQueueItemListDto(
    Guid Id,
    Guid CompanyId,
    Guid CallQueueId,
    string QueueName,
    Guid CallId,
    string CallerNumber,
    string CalledNumber,
    Guid? AssignedOperatorId,
    string? AssignedOperatorName,
    string? AssignedOperatorExtension,
    CallQueueStatus Status,
    int Priority,
    string? TransferReason,
    string? AiHandoffSummary,
    DateTime WaitingSince,
    DateTime? AssignedAt,
    DateTime? AcceptedAt,
    DateTime? CompletedAt,
    DateTime? CancelledAt,
    DateTime? ExpiresAt,
    bool IsExpired,
    int WaitingSeconds);