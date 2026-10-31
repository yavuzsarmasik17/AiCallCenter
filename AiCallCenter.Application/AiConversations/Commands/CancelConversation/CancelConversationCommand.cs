using MediatR;

namespace AiCallCenter.Application.AiConversations.Commands.CancelConversation;

public sealed record CancelConversationCommand(
    Guid CompanyId,
    Guid ConversationId)
    : IRequest;