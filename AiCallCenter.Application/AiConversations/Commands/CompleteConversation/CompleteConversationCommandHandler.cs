using AiCallCenter.Application.AiConversations.Interfaces;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.AiConversations.Commands.CompleteConversation;

public sealed class CompleteConversationCommandHandler
    : IRequestHandler<CompleteConversationCommand>
{
    private readonly IAiConversationRepository _repository;

    public CompleteConversationCommandHandler(
        IAiConversationRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        CompleteConversationCommand request,
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

        if (conversation.Status == ConversationStatus.Completed)
        {
            throw new InvalidOperationException(
                "Sohbet zaten tamamlanmış.");
        }

        if (conversation.Status == ConversationStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "İptal edilmiş sohbet tamamlanamaz.");
        }

        conversation.Complete();

        await _repository.SaveChangesAsync(
            cancellationToken);
    }
}