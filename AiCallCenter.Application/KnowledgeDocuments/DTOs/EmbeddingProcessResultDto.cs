namespace AiCallCenter.Application.KnowledgeDocuments.DTOs;

public sealed class EmbeddingProcessResultDto
{
    public int ProcessedCount { get; set; }

    public int RemainingCount { get; set; }

    public string EmbeddingModel { get; set; } = string.Empty;
}