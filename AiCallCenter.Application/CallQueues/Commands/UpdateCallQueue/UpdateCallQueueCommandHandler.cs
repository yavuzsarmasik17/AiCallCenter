using AiCallCenter.Application.CallQueues.Dtos;
using AiCallCenter.Application.CallQueues.Interfaces;
using MediatR;

namespace AiCallCenter.Application.CallQueues.Commands
    .UpdateCallQueue;

public sealed class UpdateCallQueueCommandHandler
    : IRequestHandler<
        UpdateCallQueueCommand,
        CallQueueDto>
{
    private readonly ICallQueueRepository _repository;

    public UpdateCallQueueCommandHandler(
        ICallQueueRepository repository)
    {
        _repository = repository;
    }

    public async Task<CallQueueDto> Handle(
        UpdateCallQueueCommand request,
        CancellationToken cancellationToken)
    {
        if (request.CompanyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.");
        }

        if (request.CallQueueId == Guid.Empty)
        {
            throw new ArgumentException(
                "Kuyruk kimliği geçersiz.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "Kuyruk adı boş olamaz.");
        }

        var queue = await _repository.GetByIdAsync(
            request.CompanyId,
            request.CallQueueId,
            cancellationToken);

        if (queue is null)
        {
            throw new KeyNotFoundException(
                "Çağrı kuyruğu bulunamadı.");
        }

        var nameExists =
            await _repository.NameExistsAsync(
                request.CompanyId,
                request.Name,
                request.CallQueueId,
                cancellationToken);

        if (nameExists)
        {
            throw new InvalidOperationException(
                "Bu isimde başka bir çağrı kuyruğu zaten mevcut.");
        }

        if (request.AiAgentId.HasValue)
        {
            var aiAgentExists =
                await _repository.AiAgentExistsAsync(
                    request.CompanyId,
                    request.AiAgentId.Value,
                    cancellationToken);

            if (!aiAgentExists)
            {
                throw new KeyNotFoundException(
                    "Aktif AI ajanı bulunamadı.");
            }
        }

        queue.Update(
            request.Name,
            request.Description,
            request.Priority,
            request.MaxWaitingSeconds,
            request.MaxQueueSize,
            request.AiAgentId,
            request.IsActive);

        await _repository.SaveChangesAsync(
            cancellationToken);

        var operatorCount =
            await _repository.GetOperatorCountAsync(
                request.CompanyId,
                request.CallQueueId,
                cancellationToken);

        var waitingCallCount =
            await _repository.GetWaitingCallCountAsync(
                request.CompanyId,
                request.CallQueueId,
                cancellationToken);

        return new CallQueueDto(
            queue.Id,
            queue.CompanyId,
            queue.AiAgentId,
            queue.AiAgent?.Name,
            queue.Name,
            queue.Description,
            queue.Priority,
            queue.MaxWaitingSeconds,
            queue.MaxQueueSize,
            queue.IsActive,
            operatorCount,
            waitingCallCount);
    }
}