using AiCallCenter.Application.CallQueueItems.Dtos;
using AiCallCenter.Application.CallQueueItems.Interfaces;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.CallQueueItems.Commands.CompleteQueuedCall;

public sealed class CompleteQueuedCallCommandHandler
    : IRequestHandler<
        CompleteQueuedCallCommand,
        QueuedCallActionResultDto>
{
    private readonly ICallQueueItemRepository _repository;

    public CompleteQueuedCallCommandHandler(
        ICallQueueItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<QueuedCallActionResultDto> Handle(
        CompleteQueuedCallCommand request,
        CancellationToken cancellationToken)
    {
        var queueItem = await _repository.GetByIdAsync(
            request.CompanyId,
            request.QueueItemId,
            cancellationToken);

        if (queueItem is null)
        {
            throw new KeyNotFoundException(
                "Kuyruk kaydı bulunamadı.");
        }

        if (queueItem.Status != CallQueueStatus.Accepted &&
            queueItem.Status != CallQueueStatus.Assigned)
        {
            throw new InvalidOperationException(
                "Bu çağrı tamamlanabilecek durumda değil.");
        }

        if (queueItem.AssignedOperatorId !=
            request.CallOperatorId)
        {
            throw new InvalidOperationException(
                "Bu çağrı farklı bir operatöre atanmış.");
        }

        if (queueItem.AssignedOperator is null)
        {
            throw new InvalidOperationException(
                "Atanmış operatör kaydı bulunamadı.");
        }

        queueItem.Complete();

        queueItem.Call.Complete(
            request.Summary,
            request.Result);

        queueItem.AssignedOperator.FinishCall();

        await _repository.SaveChangesAsync(
            cancellationToken);

        return new QueuedCallActionResultDto(
            queueItem.Id,
            queueItem.CallId,
            queueItem.AssignedOperatorId,
            queueItem.Status,
            queueItem.AcceptedAt,
            queueItem.CompletedAt);
    }
}
