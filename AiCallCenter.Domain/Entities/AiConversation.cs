using AiCallCenter.Domain.Common;
using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Domain.Entities;

public sealed class AiConversation : AuditableEntity
{
    public Guid CompanyId { get; private set; }

    public Guid AiAgentId { get; private set; }

    public string? CustomerName { get; private set; }

    public string? CustomerPhone { get; private set; }

    public ConversationStatus Status { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime? EndedAt { get; private set; }

    public Company Company { get; private set; } = null!;

    public AiAgent AiAgent { get; private set; } = null!;

    public ICollection<AiConversationMessage> Messages
    {
        get;
        private set;
    } = new List<AiConversationMessage>();

    private AiConversation()
    {
    }

    public AiConversation(
        Guid companyId,
        Guid aiAgentId,
        string? customerName = null,
        string? customerPhone = null)
    {
        if (companyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.",
                nameof(companyId));
        }

        if (aiAgentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Yapay zekâ ajanı kimliği geçersiz.",
                nameof(aiAgentId));
        }

        CompanyId = companyId;
        AiAgentId = aiAgentId;
        CustomerName = Normalize(customerName);
        CustomerPhone = Normalize(customerPhone);
        Status = ConversationStatus.Active;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        Status = ConversationStatus.Completed;
        EndedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = ConversationStatus.Cancelled;
        EndedAt = DateTime.UtcNow;
    }

    public AiConversationMessage AddMessage(
        MessageSpeaker speaker,
        string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException(
                "Mesaj boş olamaz.",
                nameof(content));
        }

        var message = new AiConversationMessage(
            Id,
            speaker,
            content.Trim(),
            Messages.Count + 1);

        Messages.Add(message);

        return message;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}