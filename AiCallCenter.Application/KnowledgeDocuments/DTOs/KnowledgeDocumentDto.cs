using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.KnowledgeDocuments.DTOs;

public sealed class KnowledgeDocumentDto
{
    public Guid Id { get; set; }

    public Guid KnowledgeBaseId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? OriginalFileName { get; set; }

    public long FileSizeBytes { get; set; }

    public DocumentType DocumentType { get; set; }

    public DocumentStatus Status { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime UploadedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}