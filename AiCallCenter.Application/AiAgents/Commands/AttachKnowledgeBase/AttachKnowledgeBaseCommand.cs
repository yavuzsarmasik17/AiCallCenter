using MediatR;

namespace AiCallCenter.Application.AiAgents.Commands.AttachKnowledgeBase;

public sealed record AttachKnowledgeBaseCommand(
    Guid CompanyId,
    Guid AiAgentId,
    Guid KnowledgeBaseId)
    : IRequest;