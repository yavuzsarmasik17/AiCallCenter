using AiCallCenter.Application.AiConversations.DTOs;
using AiCallCenter.Application.AiConversations.Interfaces;
using MediatR;

namespace AiCallCenter.Application.AiConversations.Queries.GetConversationDetail;

public sealed class GetConversationDetailQueryHandler
    : IRequestHandler<
        GetConversationDetailQuery,
        AiConversationDetailDto>
{
    private readonly IAiConversationRepository _repository;

    public GetConversationDetailQueryHandler(
        IAiConversationRepository repository)
    {
        _repository = repository;
    }

    public async Task<AiConversationDetailDto> Handle(
        GetConversationDetailQuery request,
        CancellationToken cancellationToken)
    {
        var conversation =
            await _repository.GetByIdWithMessagesAsync(
                request.CompanyId,
                request.ConversationId,
                cancellationToken);

        if (conversation is null)
        {
            throw new KeyNotFoundException(
                "Sohbet kaydı bulunamadı.");
        }

        return new AiConversationDetailDto
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

            Messages = conversation.Messages
                .OrderBy(message => message.SequenceNumber)
                .Select(message => new AiConversationMessageDto
                {
                    Id = message.Id,
                    Speaker = message.Speaker,
                    Content = message.Content,
                    SequenceNumber = message.SequenceNumber,
                    SentAt = message.SentAt
                })
                .ToList()
        };
    }
}