using AiCallCenter.Application.CallQueues.Dtos;
using AiCallCenter.Application.CallQueues.Interfaces;
using MediatR;

namespace AiCallCenter.Application.CallQueues.Queries
    .GetQueueOperators;

public sealed class GetQueueOperatorsQueryHandler
    : IRequestHandler<
        GetQueueOperatorsQuery,
        IReadOnlyList<CallQueueOperatorDto>>
{
    private readonly ICallQueueRepository _queueRepository;

    private readonly ICallQueueOperatorRepository
        _operatorRepository;

    public GetQueueOperatorsQueryHandler(
        ICallQueueRepository queueRepository,
        ICallQueueOperatorRepository operatorRepository)
    {
        _queueRepository = queueRepository;
        _operatorRepository = operatorRepository;
    }

    public async Task<
        IReadOnlyList<CallQueueOperatorDto>> Handle(
        GetQueueOperatorsQuery request,
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

        var queue = await _queueRepository.GetByIdAsync(
            request.CompanyId,
            request.CallQueueId,
            cancellationToken);

        if (queue is null)
        {
            throw new KeyNotFoundException(
                "Çağrı kuyruğu bulunamadı.");
        }

        var assignments =
            await _operatorRepository.GetAllAsync(
                request.CompanyId,
                request.CallQueueId,
                cancellationToken);

        return assignments
            .Select(
                x => new CallQueueOperatorDto(
                    x.Id,
                    x.CallOperatorId,
                    x.CallOperator.DisplayName,
                    x.CallOperator.ExtensionNumber,
                    (int)x.CallOperator.Status,
                    x.Priority,
                    x.IsActive,
                    x.AssignedAt))
            .ToList();
    }
}