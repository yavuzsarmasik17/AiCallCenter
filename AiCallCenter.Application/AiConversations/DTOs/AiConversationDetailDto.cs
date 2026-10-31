using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.AiConversations.DTOs;

public sealed class AiConversationDetailDto
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public Guid AiAgentId { get; set; }

    public string AiAgentName { get; set; } = string.Empty;

    public string? CustomerName { get; set; }

    public string? CustomerPhone { get; set; }

    public ConversationStatus Status { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public IReadOnlyList<AiConversationMessageDto> Messages { get; set; }
        = Array.Empty<AiConversationMessageDto>();
}