using MediatR;

namespace AiCallCenter.Application.KnowledgeDocuments.Commands.DeleteKnowledgeDocument;

public sealed record DeleteKnowledgeDocumentCommand(
    Guid CompanyId,
    Guid KnowledgeBaseId,
    Guid DocumentId)
    : IRequest;