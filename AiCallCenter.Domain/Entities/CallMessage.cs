using AiCallCenter.Domain.Common;
using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Domain.Entities;

public sealed class CallMessage : AuditableEntity
{
    public Guid CallId { get; private set; }

    public MessageSpeaker Speaker { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public DateTime SpokenAt { get; private set; }

    public int SequenceNumber { get; private set; }

    public double? ConfidenceScore { get; private set; }

    public string? AudioFileUrl { get; private set; }

    public Call Call { get; private set; } = null!;

    private CallMessage()
    {
    }

    public CallMessage(
        Guid callId,
        MessageSpeaker speaker,
        string content,
        int sequenceNumber)
    {
        if (callId == Guid.Empty)
        {
            throw new ArgumentException(
                "Çağrı kimliği geçersiz.",
                nameof(callId));
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

        CallId = callId;
        Speaker = speaker;
        Content = content.Trim();
        SequenceNumber = sequenceNumber;
        SpokenAt = DateTime.UtcNow;
    }

    public void SetConfidenceScore(double confidenceScore)
    {
        if (confidenceScore < 0 || confidenceScore > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(confidenceScore),
                "Güven skoru 0 ile 1 arasında olmalıdır.");
        }

        ConfidenceScore = confidenceScore;
    }

    public void SetAudioFileUrl(string? audioFileUrl)
    {
        AudioFileUrl = string.IsNullOrWhiteSpace(audioFileUrl)
            ? null
            : audioFileUrl.Trim();
    }

    public void UpdateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException(
                "Mesaj içeriği boş olamaz.",
                nameof(content));
        }

        Content = content.Trim();
    }
}