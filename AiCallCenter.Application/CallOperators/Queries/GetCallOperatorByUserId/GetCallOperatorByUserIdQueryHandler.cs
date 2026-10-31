using AiCallCenter.Application.CallOperators.Dtos;
using AiCallCenter.Application.CallOperators.Interfaces;
using MediatR;

namespace AiCallCenter.Application.CallOperators.Queries
    .GetCallOperatorByUserId;

public sealed class GetCallOperatorByUserIdQueryHandler
    : IRequestHandler<
        GetCallOperatorByUserIdQuery,
        CallOperatorDto>
{
    private readonly ICallOperatorRepository _repository;

    public GetCallOperatorByUserIdQueryHandler(
        ICallOperatorRepository repository)
    {
        _repository = repository;
    }

    public async Task<CallOperatorDto> Handle(
        GetCallOperatorByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CompanyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.");
        }

        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Kullanıcı kimliği geçersiz.");
        }

        var callOperator =
            await _repository.GetByUserIdAsync(
                request.CompanyId,
                request.UserId,
                cancellationToken);

        if (callOperator is null)
        {
            throw new KeyNotFoundException(
                "Bu kullanıcıya bağlı operatör kaydı bulunamadı.");
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
