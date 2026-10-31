using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.Calls.Dtos;

public sealed record CallMessageDto(
    Guid Id,
    Guid CallId,
    MessageSpeaker Speaker,
    string Content,
    DateTime SpokenAt,
    int SequenceNumber,
    double? ConfidenceScore,
    string? AudioFileUrl);