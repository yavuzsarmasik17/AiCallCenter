using AiCallCenter.Application.AiAgents.DTOs;
using MediatR;

namespace AiCallCenter.Application.AiAgents.Queries.GetAttachedKnowledgeBases;

public sealed record GetAttachedKnowledgeBasesQuery(
    Guid CompanyId,
    Guid AiAgentId)
    : IRequest<IReadOnlyList<AiAgentKnowledgeBaseDto>>;