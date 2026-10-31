namespace AiCallCenter.Application.AiAgents.DTOs;

public sealed class AiAgentKnowledgeBaseDto
{
    public Guid KnowledgeBaseId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime AttachedAt { get; set; }
}