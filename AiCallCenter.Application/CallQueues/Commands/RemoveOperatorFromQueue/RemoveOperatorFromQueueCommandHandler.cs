using AiCallCenter.Application.CallQueues.Interfaces;
using MediatR;

namespace AiCallCenter.Application.CallQueues.Commands
    .RemoveOperatorFromQueue;

public sealed class RemoveOperatorFromQueueCommandHandler
    : IRequestHandler<RemoveOperatorFromQueueCommand>
{
    private readonly ICallQueueOperatorRepository
        _repository;

    public RemoveOperatorFromQueueCommandHandler(
        ICallQueueOperatorRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        RemoveOperatorFromQueueCommand request,
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

        if (request.AssignmentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Atama kimliği geçersiz.");
        }

        var assignment =
            await _repository.GetByIdAsync(
                request.CompanyId,
                request.CallQueueId,
                request.AssignmentId,
                cancellationToken);

        if (assignment is null)
        {
            throw new KeyNotFoundException(
                "Kuyruk operatör ataması bulunamadı.");
        }

        _repository.Remove(assignment);

        await _repository.SaveChangesAsync(
            cancellationToken);
    }
}