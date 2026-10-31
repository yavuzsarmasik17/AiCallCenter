namespace AiCallCenter.Application.KnowledgeBases.DTOs;

public sealed class KnowledgeBaseDto
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}