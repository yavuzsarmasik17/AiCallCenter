namespace AiCallCenter.Application.CallQueueItems.Dtos;

public sealed record EnqueueCallResultDto(
    Guid QueueItemId,
    Guid CallQueueId,
    Guid CallId,
    string QueueName,
    int Priority,
    DateTime WaitingSince,
    DateTime? ExpiresAt);
