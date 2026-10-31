using AiCallCenter.Application.CallQueueItems.Dtos;
using AiCallCenter.Application.CallQueueItems.Interfaces;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.CallQueueItems.Commands.AcceptQueuedCall;

public sealed class AcceptQueuedCallCommandHandler
    : IRequestHandler<
        AcceptQueuedCallCommand,
        QueuedCallActionResultDto>
{
    private readonly ICallQueueItemRepository _repository;

    public AcceptQueuedCallCommandHandler(
        ICallQueueItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<QueuedCallActionResultDto> Handle(
        AcceptQueuedCallCommand request,
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

        if (queueItem.Status != CallQueueStatus.Assigned)
        {
            throw new InvalidOperationException(
                "Yalnızca atanmış çağrılar kabul edilebilir.");
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

        queueItem.Accept();
        queueItem.Call.Start();

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
