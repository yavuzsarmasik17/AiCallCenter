using MediatR;

namespace AiCallCenter.Application.AiAgents.Commands.DetachKnowledgeBase;

public sealed record DetachKnowledgeBaseCommand(
    Guid CompanyId,
    Guid AiAgentId,
    Guid KnowledgeBaseId)
    : IRequest;