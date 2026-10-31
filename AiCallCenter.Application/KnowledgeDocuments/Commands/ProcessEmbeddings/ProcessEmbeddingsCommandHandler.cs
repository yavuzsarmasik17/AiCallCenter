using System.Text.Json;
using AiCallCenter.Application.Common.AI;
using AiCallCenter.Application.KnowledgeDocuments.DTOs;
using AiCallCenter.Application.KnowledgeDocuments.Interfaces;
using MediatR;

namespace AiCallCenter.Application.KnowledgeDocuments.Commands.ProcessEmbeddings;

public sealed class ProcessEmbeddingsCommandHandler
    : IRequestHandler<
        ProcessEmbeddingsCommand,
        EmbeddingProcessResultDto>
{
    private readonly IKnowledgeDocumentRepository _repository;
    private readonly IEmbeddingService _embeddingService;

    public ProcessEmbeddingsCommandHandler(
        IKnowledgeDocumentRepository repository,
        IEmbeddingService embeddingService)
    {
        _repository = repository;
        _embeddingService = embeddingService;
    }

    public async Task<EmbeddingProcessResultDto> Handle(
        ProcessEmbeddingsCommand request,
        CancellationToken cancellationToken)
    {
        var knowledgeBaseExists =
            await _repository.KnowledgeBaseExistsAsync(
                request.CompanyId,
                request.KnowledgeBaseId,
                cancellationToken);

        if (!knowledgeBaseExists)
        {
            throw new KeyNotFoundException(
                "Bilgi bankası bulunamadı.");
        }

        var chunks =
            await _repository.GetChunksWithoutEmbeddingAsync(
                request.CompanyId,
                request.KnowledgeBaseId,
                request.MaximumCount,
                cancellationToken);

        foreach (var chunk in chunks)
        {
            var embedding =
                await _embeddingService.CreateEmbeddingAsync(
                    chunk.Content,
                    cancellationToken);

            var embeddingJson =
                JsonSerializer.Serialize(embedding);

            chunk.SetEmbedding(
                embeddingJson,
                _embeddingService.ModelName);
        }

        if (chunks.Count > 0)
        {
            await _repository.SaveChangesAsync(
                cancellationToken);
        }

        var remainingCount =
            await _repository.CountChunksWithoutEmbeddingAsync(
                request.CompanyId,
                request.KnowledgeBaseId,
                cancellationToken);

        return new EmbeddingProcessResultDto
        {
            ProcessedCount = chunks.Count,
            RemainingCount = remainingCount,
            EmbeddingModel = _embeddingService.ModelName
        };
    }
}