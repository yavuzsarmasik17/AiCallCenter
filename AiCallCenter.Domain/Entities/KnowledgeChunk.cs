using AiCallCenter.Domain.Common;

namespace AiCallCenter.Domain.Entities;

public sealed class KnowledgeChunk : AuditableEntity
{
    public Guid KnowledgeDocumentId { get; private set; }

    public int ChunkIndex { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public int CharacterCount { get; private set; }

    public string? EmbeddingJson { get; private set; }

    public string? EmbeddingModel { get; private set; }

    public DateTime? EmbeddedAt { get; private set; }

    public KnowledgeDocument KnowledgeDocument
    { get; private set; } = null!;

    private KnowledgeChunk()
    {
    }

    public KnowledgeChunk(
        Guid knowledgeDocumentId,
        int chunkIndex,
        string content)
    {
        if (knowledgeDocumentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Belge kimliği geçersiz.",
                nameof(knowledgeDocumentId));
        }

        if (chunkIndex < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(chunkIndex),
                "Parça sırası negatif olamaz.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException(
                "Parça içeriği boş olamaz.",
                nameof(content));
        }

        KnowledgeDocumentId = knowledgeDocumentId;
        ChunkIndex = chunkIndex;
        Content = content.Trim();
        CharacterCount = Content.Length;
    }

    public void SetEmbedding(
        string embeddingJson,
        string embeddingModel)
    {
        if (string.IsNullOrWhiteSpace(embeddingJson))
        {
            throw new ArgumentException(
                "Embedding verisi boş olamaz.",
                nameof(embeddingJson));
        }

        if (string.IsNullOrWhiteSpace(embeddingModel))
        {
            throw new ArgumentException(
                "Embedding modeli boş olamaz.",
                nameof(embeddingModel));
        }

        EmbeddingJson = embeddingJson.Trim();
        EmbeddingModel = embeddingModel.Trim();
        EmbeddedAt = DateTime.UtcNow;
    }

    public void ClearEmbedding()
    {
        EmbeddingJson = null;
        EmbeddingModel = null;
        EmbeddedAt = null;
    }
}