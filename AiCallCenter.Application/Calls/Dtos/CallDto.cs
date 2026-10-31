using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.Calls.Dtos;

public sealed record CallDto(
    Guid Id,
    Guid CompanyId,
    Guid AiAgentId,
    string? AiAgentName,
    string? ProviderCallId,
    string CallerNumber,
    string CalledNumber,
    CallDirection Direction,
    CallStatus Status,
    DateTime StartedAt,
    DateTime? AnsweredAt,
    DateTime? EndedAt,
    int DurationSeconds,
    string? Summary,
    string? Result,
    bool TransferredToHuman,
    string? TransferNumber);
