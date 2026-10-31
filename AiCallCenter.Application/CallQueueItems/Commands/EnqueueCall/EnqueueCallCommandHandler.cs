using AiCallCenter.Application.CallQueueItems.Dtos;
using AiCallCenter.Application.CallQueueItems.Interfaces;
using AiCallCenter.Domain.Entities;
using MediatR;

namespace AiCallCenter.Application.CallQueueItems.Commands.EnqueueCall;

public sealed class EnqueueCallCommandHandler
    : IRequestHandler<EnqueueCallCommand, EnqueueCallResultDto>
{
    private readonly ICallQueueItemRepository _repository;

    public EnqueueCallCommandHandler(
        ICallQueueItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<EnqueueCallResultDto> Handle(
        EnqueueCallCommand request,
        CancellationToken cancellationToken)
    {
        if (request.CompanyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.",
                nameof(request.CompanyId));
        }

        if (request.CallQueueId == Guid.Empty)
        {
            throw new ArgumentException(
                "Kuyruk kimliği geçersiz.",
                nameof(request.CallQueueId));
        }

        if (request.CallId == Guid.Empty)
        {
            throw new ArgumentException(
                "Çağrı kimliği geçersiz.",
                nameof(request.CallId));
        }

        if (request.TransferReason?.Length > 500)
        {
            throw new ArgumentException(
                "Aktarım nedeni en fazla 500 karakter olabilir.",
                nameof(request.TransferReason));
        }

        if (request.AiHandoffSummary?.Length > 4000)
        {
            throw new ArgumentException(
                "AI devir özeti en fazla 4000 karakter olabilir.",
                nameof(request.AiHandoffSummary));
        }

        var queue =
            await _repository.GetQueueAsync(
                request.CompanyId,
                request.CallQueueId,
                cancellationToken);

        if (queue is null)
        {
            throw new KeyNotFoundException(
                "Çağrı kuyruğu bulunamadı.");
        }

        if (!queue.IsActive)
        {
            throw new InvalidOperationException(
                "Pasif çağrı kuyruğuna çağrı eklenemez.");
        }

        var call =
            await _repository.GetCallAsync(
                request.CompanyId,
                request.CallId,
                cancellationToken);

        if (call is null)
        {
            throw new KeyNotFoundException(
                "Çağrı bulunamadı.");
        }

        var activeItemExists =
            await _repository.ActiveItemExistsAsync(
                request.CompanyId,
                request.CallId,
                cancellationToken);

        if (activeItemExists)
        {
            throw new InvalidOperationException(
                "Bu çağrı zaten aktif bir kuyruk kaydında bulunuyor.");
        }

        var currentQueueSize =
            await _repository.GetActiveQueueSizeAsync(
                request.CompanyId,
                request.CallQueueId,
                cancellationToken);

        if (currentQueueSize >= queue.MaxQueueSize)
        {
            throw new InvalidOperationException(
                "Çağrı kuyruğu kapasitesi dolu.");
        }

        var priority =
            request.Priority ??
            queue.Priority;

        var expiresAt =
            DateTime.UtcNow.AddSeconds(
                queue.MaxWaitingSeconds);

        var queueItem =
            new CallQueueItem(
                request.CompanyId,
                queue.Id,
                call.Id,
                queue.Name,
                priority,
                request.TransferReason,
                expiresAt,
                request.AiHandoffSummary);

        await _repository.AddAsync(
            queueItem,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return new EnqueueCallResultDto(
            queueItem.Id,
            queueItem.CallQueueId,
            queueItem.CallId,
            queueItem.QueueName,
            queueItem.Priority,
            queueItem.WaitingSince,
            queueItem.ExpiresAt);
    }
}