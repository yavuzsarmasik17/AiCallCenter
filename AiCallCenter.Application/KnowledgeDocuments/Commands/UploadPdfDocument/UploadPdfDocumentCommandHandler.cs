using System.Text.Json;
using AiCallCenter.Application.Common.AI;
using AiCallCenter.Application.Common.Documents;
using AiCallCenter.Application.Common.TextProcessing;
using AiCallCenter.Application.KnowledgeDocuments.DTOs;
using AiCallCenter.Application.KnowledgeDocuments.Interfaces;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.KnowledgeDocuments.Commands.UploadPdfDocument;

public sealed class UploadPdfDocumentCommandHandler
    : IRequestHandler<
        UploadPdfDocumentCommand,
        KnowledgeDocumentDto>
{
    private readonly IKnowledgeDocumentRepository
        _knowledgeDocumentRepository;

    private readonly IDocumentTextExtractor
        _documentTextExtractor;

    private readonly ITextChunker _textChunker;

    private readonly IEmbeddingService _embeddingService;

    public UploadPdfDocumentCommandHandler(
        IKnowledgeDocumentRepository knowledgeDocumentRepository,
        IDocumentTextExtractor documentTextExtractor,
        ITextChunker textChunker,
        IEmbeddingService embeddingService)
    {
        _knowledgeDocumentRepository =
            knowledgeDocumentRepository;

        _documentTextExtractor =
            documentTextExtractor;

        _textChunker =
            textChunker;

        _embeddingService =
            embeddingService;
    }

    public async Task<KnowledgeDocumentDto> Handle(
        UploadPdfDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var knowledgeBaseExists =
            await _knowledgeDocumentRepository
                .KnowledgeBaseExistsAsync(
                    request.CompanyId,
                    request.KnowledgeBaseId,
                    cancellationToken);

        if (!knowledgeBaseExists)
        {
            throw new KeyNotFoundException(
                "Bilgi bankası bulunamadı.");
        }

        var normalizedName = request.Name.Trim();

        var nameExists =
            await _knowledgeDocumentRepository.NameExistsAsync(
                request.KnowledgeBaseId,
                normalizedName,
                cancellationToken);

        if (nameExists)
        {
            throw new InvalidOperationException(
                "Bu bilgi bankasında aynı isimde bir belge bulunuyor.");
        }

        var document = new KnowledgeDocument(
            knowledgeBaseId: request.KnowledgeBaseId,
            name: normalizedName,
            documentType: DocumentType.Pdf,
            originalFileName: request.OriginalFileName,
            filePath: null,
            mimeType: request.MimeType,
            fileSizeBytes: request.FileSizeBytes);

        document.MarkAsProcessing();

        await _knowledgeDocumentRepository.AddDocumentAsync(
            document,
            cancellationToken);

        await _knowledgeDocumentRepository.SaveChangesAsync(
            cancellationToken);

        try
        {
            var extractedText =
                await _documentTextExtractor.ExtractPdfTextAsync(
                    request.FileStream,
                    cancellationToken);

            var textChunks = _textChunker.Split(
                extractedText,
                maxChunkLength: 1000,
                overlapLength: 150);

            if (textChunks.Count == 0)
            {
                throw new InvalidOperationException(
                    "PDF içeriği parçalara ayrılamadı.");
            }

            var chunks = new List<KnowledgeChunk>();

            for (var index = 0;
                 index < textChunks.Count;
                 index++)
            {
                var content = textChunks[index];

                var chunk = new KnowledgeChunk(
                    document.Id,
                    index,
                    content);

                var embedding =
                    await _embeddingService.CreateEmbeddingAsync(
                        content,
                        cancellationToken);

                var embeddingJson =
                    JsonSerializer.Serialize(embedding);

                chunk.SetEmbedding(
               embeddingJson,
              _embeddingService.ModelName);

                chunks.Add(chunk);
            }

            await _knowledgeDocumentRepository.AddChunksAsync(
                chunks,
                cancellationToken);

            document.MarkAsReady();

            await _knowledgeDocumentRepository.SaveChangesAsync(
                cancellationToken);
        }
        catch (Exception exception)
        {
            document.MarkAsFailed(exception.Message);

            await _knowledgeDocumentRepository.SaveChangesAsync(
                cancellationToken);

            throw;
        }

        return new KnowledgeDocumentDto
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
        };
    }
}