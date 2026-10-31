using AiCallCenter.Application.AiAgents.DTOs;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.AiAgents.Commands.UpdateAiAgent;

public sealed record UpdateAiAgentCommand(
    Guid CompanyId,
    Guid AiAgentId,
    string Name,
    string Instructions,
    string WelcomeMessage,
    AgentLanguage Language,
    AiProvider Provider,
    string ModelName)
    : IRequest<AiAgentDto>;