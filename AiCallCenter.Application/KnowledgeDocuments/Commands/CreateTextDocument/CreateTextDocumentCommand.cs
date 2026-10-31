using AiCallCenter.Application.KnowledgeDocuments.DTOs;
using MediatR;

namespace AiCallCenter.Application.KnowledgeDocuments.Commands.CreateTextDocument;

public sealed record CreateTextDocumentCommand(
    Guid CompanyId,
    Guid KnowledgeBaseId,
    string Name,
    string Content)
    : IRequest<KnowledgeDocumentDto>;