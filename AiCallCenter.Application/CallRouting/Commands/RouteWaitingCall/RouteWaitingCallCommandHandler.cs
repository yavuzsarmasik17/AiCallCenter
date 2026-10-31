using AiCallCenter.Application.CallRouting.Dtos;
using AiCallCenter.Application.CallRouting.Interfaces;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.CallRouting.Commands
    .RouteWaitingCall;

public sealed class RouteWaitingCallCommandHandler
    : IRequestHandler<
        RouteWaitingCallCommand,
        CallRoutingResultDto>
{
    private readonly ICallRoutingRepository _repository;

    public RouteWaitingCallCommandHandler(
        ICallRoutingRepository repository)
    {
        _repository = repository;
    }

    public async Task<CallRoutingResultDto> Handle(
        RouteWaitingCallCommand request,
        CancellationToken cancellationToken)
    {
        if (request.CompanyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.",
                nameof(request.CompanyId));
        }

        if (request.QueueItemId == Guid.Empty)
        {
            throw new ArgumentException(
                "Kuyruk kaydı kimliği geçersiz.",
                nameof(request.QueueItemId));
        }

        var queueItem = await _repository.GetQueueItemAsync(
            request.CompanyId,
            request.QueueItemId,
            cancellationToken);

        if (queueItem is null)
        {
            throw new KeyNotFoundException(
                "Bekleyen çağrı kaydı bulunamadı.");
        }

        if (queueItem.Status != CallQueueStatus.Waiting)
        {
            throw new InvalidOperationException(
                "Yalnızca bekleyen çağrılar yönlendirilebilir.");
        }

        if (queueItem.IsExpired())
        {
            queueItem.Expire();

            await _repository.SaveChangesAsync(
                cancellationToken);

            throw new InvalidOperationException(
                "Kuyruk kaydının bekleme süresi dolmuş.");
        }

        if (!queueItem.CallQueue.IsActive)
        {
            throw new InvalidOperationException(
                "Çağrı kuyruğu aktif değil.");
        }

        var candidates =
    await _repository.GetAvailableOperatorsAsync(
        request.CompanyId,
        queueItem.CallQueueId,
        cancellationToken);

        var candidateCount = candidates.Count;

        var selectedCandidate = candidates
            .OrderByDescending(x => x.QueuePriority)
            .ThenBy(x => x.Operator.ActiveCallCount)
            .ThenBy(x =>
                x.Operator.LastStatusChangedAt ??
                DateTime.MinValue)
            .FirstOrDefault();

        if (selectedCandidate is null)
        {
            throw new InvalidOperationException(
                $"Uygun operatör bulunamadı. " +
                $"Aday sayısı: {candidateCount}, " +
                $"CompanyId: {request.CompanyId}, " +
                $"CallQueueId: {queueItem.CallQueueId}");
        }

        var selectedOperator =
            selectedCandidate.Operator;

        var extensionNumber =
            selectedOperator.ExtensionNumber!;

        queueItem.AssignToOperator(
            selectedOperator.Id);

        selectedOperator.StartCall(
            queueItem.CallId);

        queueItem.Call.TransferToHumanAgent(
            extensionNumber);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return new CallRoutingResultDto(
            true,
            queueItem.Id,
            queueItem.CallId,
            selectedOperator.Id,
            selectedOperator.DisplayName,
            extensionNumber,
            queueItem.AssignedAt ?? 
            DateTime.UtcNow);
    }
}
