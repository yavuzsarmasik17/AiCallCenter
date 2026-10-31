namespace AiCallCenter.Application.CallOperators.Dtos;

public sealed record UpdateCallOperatorRequest(
    string DisplayName,
    string? ExtensionNumber,
    int MaxConcurrentCalls,
    bool CanReceiveCalls);