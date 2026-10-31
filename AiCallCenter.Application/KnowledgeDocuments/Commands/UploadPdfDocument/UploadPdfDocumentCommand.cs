using AiCallCenter.Application.KnowledgeDocuments.DTOs;
using MediatR;

namespace AiCallCenter.Application.KnowledgeDocuments.Commands.UploadPdfDocument;

public sealed record UploadPdfDocumentCommand(
    Guid CompanyId,
    Guid KnowledgeBaseId,
    string Name,
    string OriginalFileName,
    string MimeType,
    long FileSizeBytes,
    Stream FileStream)
    : IRequest<KnowledgeDocumentDto>;