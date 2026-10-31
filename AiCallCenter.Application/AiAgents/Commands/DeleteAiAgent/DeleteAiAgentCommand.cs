using MediatR;

namespace AiCallCenter.Application.AiAgents.Commands.DeleteAiAgent;

public sealed record DeleteAiAgentCommand(
    Guid CompanyId,
    Guid AiAgentId)
    : IRequest;