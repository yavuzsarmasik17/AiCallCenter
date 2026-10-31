namespace AiCallCenter.Application.CallRouting.Dtos;

public sealed record CallRoutingResultDto(
    bool Success,
    Guid QueueItemId,
    Guid CallId,
    Guid CallOperatorId,
    string OperatorName,
    string ExtensionNumber,
    DateTime AssignedAt);
