namespace AiCallCenter.Application.CallOperatorAiAgents.Dtos;

public sealed record AvailableAiAgentDto(
    Guid Id,
    string Name,
    string Provider,
    string ModelName,
    bool IsActive,
    bool IsAssigned,
    bool IsPrimary);