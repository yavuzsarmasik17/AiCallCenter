namespace AiCallCenter.Application.CallOperatorAiAgents.Dtos;

public sealed record CallOperatorAiAgentDto(
    Guid Id,
    Guid CompanyId,
    Guid CallOperatorId,
    string CallOperatorName,
    Guid AiAgentId,
    string AiAgentName,
    string Provider,
    string ModelName,
    bool IsPrimary,
    bool IsActive,
    DateTime AssignedAt);