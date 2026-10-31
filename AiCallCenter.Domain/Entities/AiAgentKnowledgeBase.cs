using AiCallCenter.Domain.Common;

namespace AiCallCenter.Domain.Entities;

public sealed class AiAgentKnowledgeBase : AuditableEntity
{
    public Guid AiAgentId { get; private set; }

    public Guid KnowledgeBaseId { get; private set; }

    public AiAgent AiAgent { get; private set; } = null!;

    public KnowledgeBase KnowledgeBase { get; private set; } = null!;

    private AiAgentKnowledgeBase()
    {
    }

    public AiAgentKnowledgeBase(
        Guid aiAgentId,
        Guid knowledgeBaseId)
    {
        if (aiAgentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Yapay zekâ ajanı kimliği geçersiz.",
                nameof(aiAgentId));
        }

        if (knowledgeBaseId == Guid.Empty)
        {
            throw new ArgumentException(
                "Bilgi bankası kimliği geçersiz.",
                nameof(knowledgeBaseId));
        }

        AiAgentId = aiAgentId;
        KnowledgeBaseId = knowledgeBaseId;
    }
}