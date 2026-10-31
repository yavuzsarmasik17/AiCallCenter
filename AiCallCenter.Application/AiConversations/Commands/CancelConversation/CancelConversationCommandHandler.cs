using AiCallCenter.Application.AiConversations.Interfaces;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.AiConversations.Commands.CancelConversation;

public sealed class CancelConversationCommandHandler
    : IRequestHandler<CancelConversationCommand>
{
    private readonly IAiConversationRepository _repository;

    public CancelConversationCommandHandler(
        IAiConversationRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        CancelConversationCommand request,
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

        if (conversation.Status == ConversationStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Sohbet zaten iptal edilmiş.");
        }

        if (conversation.Status == ConversationStatus.Completed)
        {
            throw new InvalidOperationException(
                "Tamamlanmış sohbet iptal edilemez.");
        }

        conversation.Cancel();

        await _repository.SaveChangesAsync(
            cancellationToken);
    }
}