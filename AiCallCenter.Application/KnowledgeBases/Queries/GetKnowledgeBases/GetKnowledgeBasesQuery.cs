using AiCallCenter.Application.KnowledgeBases.DTOs;
using MediatR;

namespace AiCallCenter.Application.KnowledgeBases.Queries.GetKnowledgeBases;

public sealed record GetKnowledgeBasesQuery(Guid CompanyId)
    : IRequest<IReadOnlyList<KnowledgeBaseDto>>;