using AiCallCenter.Application.Calls.Dtos;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.Calls.Commands.CreateCallMessage;

public sealed record CreateCallMessageCommand(
    Guid CompanyId,
    Guid CallId,
    MessageSpeaker Speaker,
    string Content,
    double? ConfidenceScore,
    string? AudioFileUrl)
    : IRequest<CallMessageDto>;