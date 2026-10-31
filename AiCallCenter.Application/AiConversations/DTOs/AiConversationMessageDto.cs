using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.AiConversations.DTOs;

public sealed class AiConversationMessageDto
{
    public Guid Id { get; set; }

    public MessageSpeaker Speaker { get; set; }

    public string Content { get; set; } = string.Empty;

    public int SequenceNumber { get; set; }

    public DateTime SentAt { get; set; }
}