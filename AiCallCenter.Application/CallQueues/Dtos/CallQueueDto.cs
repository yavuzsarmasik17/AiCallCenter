namespace AiCallCenter.Application.CallQueues.Dtos;

public sealed record CallQueueDto(
    Guid Id,
    Guid CompanyId,
    Guid? AiAgentId,
    string? AiAgentName,
    string Name,
    string? Description,
    int Priority,
    int MaxWaitingSeconds,
    int MaxQueueSize,
    bool IsActive,
    int OperatorCount,
    int WaitingCallCount);