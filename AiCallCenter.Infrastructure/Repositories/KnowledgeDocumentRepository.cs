using AiCallCenter.Application.KnowledgeDocuments.Interfaces;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCallCenter.Infrastructure.Repositories;

public sealed class KnowledgeDocumentRepository
    : IKnowledgeDocumentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public KnowledgeDocumentRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> KnowledgeBaseExistsAsync(
        Guid companyId,
        Guid knowledgeBaseId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.KnowledgeBases.AnyAsync(
            knowledgeBase =>
                knowledgeBase.Id == knowledgeBaseId &&
                knowledgeBase.CompanyId == companyId &&
                knowledgeBase.IsActive,
            cancellationToken);
    }

    public Task<bool> NameExistsAsync(
        Guid knowledgeBaseId,
        string name,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.KnowledgeDocuments.AnyAsync(
            document =>
                document.KnowledgeBaseId == knowledgeBaseId &&
                document.Name == name,
            cancellationToken);
    }

    public async Task AddDocumentAsync(
        KnowledgeDocument document,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.KnowledgeDocuments.AddAsync(
            document,
            cancellationToken);
    }

    public async Task AddChunksAsync(
        IEnumerable<KnowledgeChunk> chunks,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.KnowledgeChunks.AddRangeAsync(
            chunks,
            cancellationToken);
    }

    public async Task<IReadOnlyList<KnowledgeDocument>>
        GetByKnowledgeBaseIdAsync(
            Guid knowledgeBaseId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.KnowledgeDocuments
            .AsNoTracking()
            .Where(document =>
                document.KnowledgeBaseId == knowledgeBaseId)
            .OrderByDescending(document => document.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<KnowledgeDocument?> GetByIdAsync(
        Guid companyId,
        Guid knowledgeBaseId,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.KnowledgeDocuments
            .FirstOrDefaultAsync(
                document =>
                    document.Id == documentId &&
                    document.KnowledgeBaseId ==
                        knowledgeBaseId &&
                    document.KnowledgeBase.CompanyId ==
                        companyId,
                cancellationToken);
    }

    public void DeleteDocument(
        KnowledgeDocument document)
    {
        _dbContext.KnowledgeDocuments.Remove(
            document);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<IReadOnlyList<KnowledgeChunk>>
        GetChunksWithoutEmbeddingAsync(
            Guid companyId,
            Guid knowledgeBaseId,
            int maximumCount,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.KnowledgeChunks
            .Where(chunk =>
                chunk.KnowledgeDocument.KnowledgeBaseId ==
                    knowledgeBaseId &&

                chunk.KnowledgeDocument
                    .KnowledgeBase.CompanyId ==
                    companyId &&

                chunk.KnowledgeDocument.Status ==
                    Domain.Enums.DocumentStatus.Ready &&

                chunk.EmbeddingJson == null)
            .OrderBy(chunk => chunk.CreatedAt)
            .Take(maximumCount)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountChunksWithoutEmbeddingAsync(
        Guid companyId,
        Guid knowledgeBaseId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.KnowledgeChunks.CountAsync(
            chunk =>
                chunk.KnowledgeDocument.KnowledgeBaseId ==
                    knowledgeBaseId &&

                chunk.KnowledgeDocument
                    .KnowledgeBase.CompanyId ==
                    companyId &&

                chunk.KnowledgeDocument.Status ==
                    Domain.Enums.DocumentStatus.Ready &&

                chunk.EmbeddingJson == null,
            cancellationToken);
    }
}