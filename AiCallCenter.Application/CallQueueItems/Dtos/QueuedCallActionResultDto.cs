using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.CallQueueItems.Dtos;

public sealed record QueuedCallActionResultDto(
    Guid QueueItemId,
    Guid CallId,
    Guid? AssignedOperatorId,
    CallQueueStatus Status,
    DateTime? AcceptedAt,
    DateTime? CompletedAt);
