using MediatR;

namespace AiCallCenter.Application.AiAgents.Commands.ChatWithAiAgent;

public sealed record ChatWithAiAgentCommand(
    Guid CompanyId,
    Guid AiAgentId,
    Guid? ConversationId,
    string Message)
    : IRequest<ChatWithAiAgentResponse>;