using System.Xml.Linq;
using AiCallCenter.Domain.Common;
using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Domain.Entities;

public sealed class KnowledgeDocument : AuditableEntity
{
    public Guid KnowledgeBaseId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? OriginalFileName { get; private set; }

    public string? FilePath { get; private set; }

    public string? MimeType { get; private set; }

    public long FileSizeBytes { get; private set; }

    public DocumentType DocumentType { get; private set; }

    public DocumentStatus Status { get; private set; }

    public string? ErrorMessage { get; private set; }

    public DateTime UploadedAt { get; private set; }

    public DateTime? ProcessedAt { get; private set; }

    public KnowledgeBase KnowledgeBase { get; private set; } = null!;

    public ICollection<KnowledgeChunk> Chunks { get; private set; }
        = new List<KnowledgeChunk>();

    private KnowledgeDocument()
    {
    }

    public KnowledgeDocument(
        Guid knowledgeBaseId,
    string name,
        DocumentType documentType,
        string? originalFileName = null,
        string? filePath = null,
        string? mimeType = null,
        long fileSizeBytes = 0)
    {
        if (knowledgeBaseId == Guid.Empty)
        {
            throw new ArgumentException(
                "Bilgi bankası kimliği geçersiz.",
                nameof(knowledgeBaseId));
        }

        if (fileSizeBytes < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fileSizeBytes),
                "Dosya boyutu negatif olamaz.");
        }

        KnowledgeBaseId = knowledgeBaseId;

        SetName(name);

        DocumentType = documentType;
        OriginalFileName = NormalizeOptionalValue(originalFileName);
        FilePath = NormalizeOptionalValue(filePath);
        MimeType = NormalizeOptionalValue(mimeType);
        FileSizeBytes = fileSizeBytes;
        Status = DocumentStatus.Uploaded;
        UploadedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessing()
    {
        Status = DocumentStatus.Processing;
        ErrorMessage = null;
    }

    public void MarkAsReady()
    {
        Status = DocumentStatus.Ready;
        ProcessedAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    public void MarkAsFailed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException(
                "Hata mesajı boş olamaz.",
                nameof(errorMessage));
        }

        Status = DocumentStatus.Failed;
        ErrorMessage = errorMessage.Trim();
        ProcessedAt = DateTime.UtcNow;
    }

    public void UpdateStorageInformation(
        string? originalFileName,
        string? filePath,
        string? mimeType,
        long fileSizeBytes)
    {
        if (fileSizeBytes < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fileSizeBytes),
                "Dosya boyutu negatif olamaz.");
        }

        OriginalFileName = NormalizeOptionalValue(originalFileName);
        FilePath = NormalizeOptionalValue(filePath);
        MimeType = NormalizeOptionalValue(mimeType);
        FileSizeBytes = fileSizeBytes;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Belge adı boş olamaz.",
                nameof(name));
        }

        if (name.Trim().Length > 300)
        {
            throw new ArgumentException(
                "Belge adı en fazla 300 karakter olabilir.",
                nameof(name));
        }

        Name = name.Trim();
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}