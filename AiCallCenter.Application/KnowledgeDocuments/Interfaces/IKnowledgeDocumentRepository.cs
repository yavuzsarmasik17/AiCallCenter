using AiCallCenter.Domain.Entities;

namespace AiCallCenter.Application.KnowledgeDocuments.Interfaces;

public interface IKnowledgeDocumentRepository
{
    Task<bool> KnowledgeBaseExistsAsync(
        Guid companyId,
        Guid knowledgeBaseId,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
        Guid knowledgeBaseId,
        string name,
        CancellationToken cancellationToken = default);

    Task AddDocumentAsync(
        KnowledgeDocument document,
        CancellationToken cancellationToken = default);

    Task AddChunksAsync(
        IEnumerable<KnowledgeChunk> chunks,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KnowledgeDocument>>
        GetByKnowledgeBaseIdAsync(
            Guid knowledgeBaseId,
            CancellationToken cancellationToken = default);

    Task<KnowledgeDocument?> GetByIdAsync(
        Guid companyId,
        Guid knowledgeBaseId,
        Guid documentId,
        CancellationToken cancellationToken = default);

    void DeleteDocument(
        KnowledgeDocument document);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KnowledgeChunk>>
        GetChunksWithoutEmbeddingAsync(
            Guid companyId,
            Guid knowledgeBaseId,
            int maximumCount,
            CancellationToken cancellationToken = default);

    Task<int> CountChunksWithoutEmbeddingAsync(
        Guid companyId,
        Guid knowledgeBaseId,
        CancellationToken cancellationToken = default);
}