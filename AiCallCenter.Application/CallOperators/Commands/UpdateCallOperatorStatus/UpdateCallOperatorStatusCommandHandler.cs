using AiCallCenter.Application.CallOperators.Dtos;
using AiCallCenter.Application.CallOperators.Interfaces;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.CallOperators.Commands
    .UpdateCallOperatorStatus;

public sealed class UpdateCallOperatorStatusCommandHandler
    : IRequestHandler<
        UpdateCallOperatorStatusCommand,
        CallOperatorDto>
{
    private readonly ICallOperatorRepository _repository;

    public UpdateCallOperatorStatusCommandHandler(
        ICallOperatorRepository repository)
    {
        _repository = repository;
    }

    public async Task<CallOperatorDto> Handle(
        UpdateCallOperatorStatusCommand request,
        CancellationToken cancellationToken)
    {
        var callOperator =
            await _repository.GetByIdAsync(
                request.CompanyId,
                request.OperatorId,
                cancellationToken);

        if (callOperator is null)
        {
            throw new KeyNotFoundException(
                "Operatör bulunamadı.");
        }

        switch (request.Status)
        {
            case CallOperatorStatus.Available:
                callOperator.SetAvailable();
                break;

            case CallOperatorStatus.OnBreak:
                callOperator.SetOnBreak();
                break;

            case CallOperatorStatus.AfterCallWork:
                callOperator.SetAfterCallWork();
                break;

            case CallOperatorStatus.Offline:
                callOperator.SetOffline();
                break;

            case CallOperatorStatus.Busy:
                throw new InvalidOperationException(
                    "Operatör doğrudan meşgul durumuna alınamaz.");     

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(request.Status),
                    "Geçersiz operatör durumu.");
        }

        await _repository.SaveChangesAsync(
            cancellationToken);

        return new CallOperatorDto(
            callOperator.Id,
            callOperator.CompanyId,
            callOperator.UserId,
            callOperator.DisplayName,
            callOperator.ExtensionNumber,
            callOperator.Status,
            callOperator.CurrentCallId,
            callOperator.MaxConcurrentCalls,
            callOperator.ActiveCallCount,
            callOperator.CanReceiveCalls,
            callOperator.LastActivityAt,
            callOperator.LastStatusChangedAt);
    }
}