using AiCallCenter.Domain.Common;
using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Domain.Entities;

public sealed class AiConversationMessage : AuditableEntity
{
    public Guid ConversationId { get; private set; }

    public MessageSpeaker Speaker { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public int SequenceNumber { get; private set; }

    public DateTime SentAt { get; private set; }

    public AiConversation Conversation { get; private set; } = null!;

    private AiConversationMessage()
    {
    }

    public AiConversationMessage(
        Guid conversationId,
        MessageSpeaker speaker,
        string content,
        int sequenceNumber)
    {
        if (conversationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Sohbet kimliği geçersiz.",
                nameof(conversationId));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException(
                "Mesaj içeriği boş olamaz.",
                nameof(content));
        }

        if (sequenceNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sequenceNumber),
                "Mesaj sıra numarası sıfırdan büyük olmalıdır.");
        }

        ConversationId = conversationId;
        Speaker = speaker;
        Content = content.Trim();
        SequenceNumber = sequenceNumber;
        SentAt = DateTime.UtcNow;
    }
}