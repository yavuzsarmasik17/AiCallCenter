using AiCallCenter.Application.AiConversations.DTOs;
using AiCallCenter.Application.AiConversations.Interfaces;
using MediatR;

namespace AiCallCenter.Application.AiConversations.Queries.GetConversations;

public sealed class GetConversationsQueryHandler
    : IRequestHandler<
        GetConversationsQuery,
        IReadOnlyList<AiConversationDto>>
{
    private readonly IAiConversationRepository _repository;

    public GetConversationsQueryHandler(
        IAiConversationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AiConversationDto>> Handle(
        GetConversationsQuery request,
        CancellationToken cancellationToken)
    {
        var conversations = await _repository.GetByCompanyIdAsync(
            request.CompanyId,
            cancellationToken);

        return conversations
            .Select(conversation => new AiConversationDto
            {
                Id = conversation.Id,
                CompanyId = conversation.CompanyId,
                AiAgentId = conversation.AiAgentId,
                AiAgentName = conversation.AiAgent.Name,
                CustomerName = conversation.CustomerName,
                CustomerPhone = conversation.CustomerPhone,
                Status = conversation.Status,
                StartedAt = conversation.StartedAt,
                EndedAt = conversation.EndedAt,
                MessageCount = conversation.Messages.Count
            })
            .ToList();
    }
}