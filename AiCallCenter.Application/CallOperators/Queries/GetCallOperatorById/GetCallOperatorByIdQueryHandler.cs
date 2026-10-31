using AiCallCenter.Application.CallOperators.Dtos;
using AiCallCenter.Application.CallOperators.Interfaces;
using MediatR;

namespace AiCallCenter.Application.CallOperators.Queries
    .GetCallOperatorById;

public sealed class GetCallOperatorByIdQueryHandler
    : IRequestHandler<
        GetCallOperatorByIdQuery,
        CallOperatorDto>
{
    private readonly ICallOperatorRepository _repository;

    public GetCallOperatorByIdQueryHandler(
        ICallOperatorRepository repository)
    {
        _repository = repository;
    }

    public async Task<CallOperatorDto> Handle(
        GetCallOperatorByIdQuery request,
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