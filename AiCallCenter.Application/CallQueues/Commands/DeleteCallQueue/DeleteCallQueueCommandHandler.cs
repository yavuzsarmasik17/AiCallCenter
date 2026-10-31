using AiCallCenter.Application.CallQueues.Interfaces;
using MediatR;

namespace AiCallCenter.Application.CallQueues.Commands
    .DeleteCallQueue;

public sealed class DeleteCallQueueCommandHandler
    : IRequestHandler<DeleteCallQueueCommand>
{
    private readonly ICallQueueRepository _repository;

    public DeleteCallQueueCommandHandler(
        ICallQueueRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        DeleteCallQueueCommand request,
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

        var queue = await _repository.GetByIdAsync(
            request.CompanyId,
            request.CallQueueId,
            cancellationToken);

        if (queue is null)
        {
            throw new KeyNotFoundException(
                "Çağrı kuyruğu bulunamadı.");
        }

        var waitingCallCount =
            await _repository.GetWaitingCallCountAsync(
                request.CompanyId,
                request.CallQueueId,
                cancellationToken);

        if (waitingCallCount > 0)
        {
            throw new InvalidOperationException(
                "Bekleyen çağrısı bulunan kuyruk silinemez.");
        }

        _repository.Remove(queue);

        await _repository.SaveChangesAsync(
            cancellationToken);
    }
}