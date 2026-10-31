namespace AiCallCenter.Application.CallOperators.Dtos;

public sealed record CreateCallOperatorRequest(
    Guid UserId,
    string DisplayName,
    string? ExtensionNumber,
    int MaxConcurrentCalls,
    bool CanReceiveCalls);