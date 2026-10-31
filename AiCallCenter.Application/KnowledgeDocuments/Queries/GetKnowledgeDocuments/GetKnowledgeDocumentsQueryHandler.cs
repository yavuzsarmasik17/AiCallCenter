using AiCallCenter.Application.KnowledgeDocuments.DTOs;
using AiCallCenter.Application.KnowledgeDocuments.Interfaces;
using MediatR;

namespace AiCallCenter.Application.KnowledgeDocuments.Queries.GetKnowledgeDocuments;

public sealed class GetKnowledgeDocumentsQueryHandler
    : IRequestHandler<
        GetKnowledgeDocumentsQuery,
        IReadOnlyList<KnowledgeDocumentDto>>
{
    private readonly IKnowledgeDocumentRepository
        _knowledgeDocumentRepository;

    public GetKnowledgeDocumentsQueryHandler(
        IKnowledgeDocumentRepository knowledgeDocumentRepository)
    {
        _knowledgeDocumentRepository = knowledgeDocumentRepository;
    }

    public async Task<IReadOnlyList<KnowledgeDocumentDto>> Handle(
        GetKnowledgeDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        var knowledgeBaseExists =
            await _knowledgeDocumentRepository.KnowledgeBaseExistsAsync(
                request.CompanyId,
                request.KnowledgeBaseId,
                cancellationToken);

        if (!knowledgeBaseExists)
        {
            throw new KeyNotFoundException(
                "Bilgi bankası bulunamadı.");
        }

        var documents =
            await _knowledgeDocumentRepository
                .GetByKnowledgeBaseIdAsync(
                    request.KnowledgeBaseId,
                    cancellationToken);

        return documents.Select(document =>
            new KnowledgeDocumentDto
            {
                Id = document.Id,
                KnowledgeBaseId = document.KnowledgeBaseId,
                Name = document.Name,
                OriginalFileName = document.OriginalFileName,
                FileSizeBytes = document.FileSizeBytes,
                DocumentType = document.DocumentType,
                Status = document.Status,
                ErrorMessage = document.ErrorMessage,
                UploadedAt = document.UploadedAt,
                ProcessedAt = document.ProcessedAt,
                CreatedAt = document.CreatedAt
            })
            .ToList();
    }
}