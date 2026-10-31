using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.Common.Realtime;

public sealed record CallRealtimeMessage(
    Guid Id,
    Guid CompanyId,
    Guid CallId,
    MessageSpeaker Speaker,
    string Content,
    DateTime SpokenAt,
    int SequenceNumber,
    double? ConfidenceScore,
    string? AudioFileUrl);