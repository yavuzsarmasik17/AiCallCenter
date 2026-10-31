using MediatR;

namespace AiCallCenter.Application.AiConversations.Commands.CompleteConversation;

public sealed record CompleteConversationCommand(
    Guid CompanyId,
    Guid ConversationId)
    : IRequest;