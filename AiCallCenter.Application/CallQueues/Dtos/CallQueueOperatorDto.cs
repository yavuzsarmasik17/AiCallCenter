namespace AiCallCenter.Application.CallQueues.Dtos;

public sealed record CallQueueOperatorDto(
    Guid AssignmentId,
    Guid CallOperatorId,
    string DisplayName,
    string? ExtensionNumber,
    int OperatorStatus,
    int Priority,
    bool IsActive,
    DateTime AssignedAt);