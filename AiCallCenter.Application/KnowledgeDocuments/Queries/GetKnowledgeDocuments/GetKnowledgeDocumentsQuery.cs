using AiCallCenter.Application.KnowledgeDocuments.DTOs;
using MediatR;

namespace AiCallCenter.Application.KnowledgeDocuments.Queries.GetKnowledgeDocuments;

public sealed record GetKnowledgeDocumentsQuery(
    Guid CompanyId,
    Guid KnowledgeBaseId)
    : IRequest<IReadOnlyList<KnowledgeDocumentDto>>;