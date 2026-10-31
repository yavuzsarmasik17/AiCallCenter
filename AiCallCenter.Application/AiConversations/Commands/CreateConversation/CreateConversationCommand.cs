using MediatR;

namespace AiCallCenter.Application.AiConversations.Commands.CreateConversation;

public sealed record CreateConversationCommand(
    Guid CompanyId,
    Guid AiAgentId,
    string? CustomerName,
    string? CustomerPhone) : IRequest<Guid>;