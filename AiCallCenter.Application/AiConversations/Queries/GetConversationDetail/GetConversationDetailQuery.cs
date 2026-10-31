using AiCallCenter.Application.AiConversations.DTOs;
using MediatR;

namespace AiCallCenter.Application.AiConversations.Queries.GetConversationDetail;

public sealed record GetConversationDetailQuery(
    Guid CompanyId,
    Guid ConversationId)
    : IRequest<AiConversationDetailDto>;