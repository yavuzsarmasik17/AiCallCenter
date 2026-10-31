using AiCallCenter.Application.AiConversations.DTOs;
using MediatR;

namespace AiCallCenter.Application.AiConversations.Queries.GetConversations;

public sealed record GetConversationsQuery(Guid CompanyId)
    : IRequest<IReadOnlyList<AiConversationDto>>;