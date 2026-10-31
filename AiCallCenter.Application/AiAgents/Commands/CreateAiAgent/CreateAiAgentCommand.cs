using AiCallCenter.Application.AiAgents.DTOs;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.AiAgents.Commands.CreateAiAgent;

public sealed record CreateAiAgentCommand(
    Guid CompanyId,
    string Name,
    string Instructions,
    string WelcomeMessage,
    AgentLanguage Language,
    AiProvider Provider,
    string ModelName)
    : IRequest<AiAgentDto>;