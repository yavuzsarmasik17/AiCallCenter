using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.CallOperators.Dtos;

public sealed record CallOperatorDto(
    Guid Id,
    Guid CompanyId,
    Guid UserId,
    string DisplayName,
    string? ExtensionNumber,
    CallOperatorStatus Status,
    Guid? CurrentCallId,
    int MaxConcurrentCalls,
    int ActiveCallCount,
    bool CanReceiveCalls,
    DateTime? LastActivityAt,
    DateTime? LastStatusChangedAt);