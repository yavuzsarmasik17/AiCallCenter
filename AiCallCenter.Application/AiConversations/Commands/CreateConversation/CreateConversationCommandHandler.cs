using AiCallCenter.Application.AiConversations.Interfaces;
using AiCallCenter.Domain.Entities;
using MediatR;

namespace AiCallCenter.Application.AiConversations.Commands.CreateConversation;

public sealed class CreateConversationCommandHandler
    : IRequestHandler<CreateConversationCommand, Guid>
{
    private readonly IAiConversationRepository
        _conversationRepository;

    public CreateConversationCommandHandler(
        IAiConversationRepository conversationRepository)
    {
        _conversationRepository =
            conversationRepository;
    }

    public async Task<Guid> Handle(
        CreateConversationCommand request,
        CancellationToken cancellationToken)
    {
        var conversation = new AiConversation(
            request.CompanyId,
            request.AiAgentId,
            request.CustomerName,
            request.CustomerPhone);

        await _conversationRepository
            .AddConversationAsync(
                conversation,
                cancellationToken);

        await _conversationRepository
            .SaveChangesAsync(
                cancellationToken);

        return conversation.Id;
    }
}