using AiCallCenter.Application.CallOperators.Dtos;
using AiCallCenter.Application.CallOperators.Interfaces;
using MediatR;

namespace AiCallCenter.Application.CallOperators.Queries.GetCallOperators;

public sealed class GetCallOperatorsQueryHandler
    : IRequestHandler<
        GetCallOperatorsQuery,
        IReadOnlyList<CallOperatorDto>>
{
    private readonly ICallOperatorRepository _repository;

    public GetCallOperatorsQueryHandler(
        ICallOperatorRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CallOperatorDto>> Handle(
        GetCallOperatorsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CompanyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.",
                nameof(request.CompanyId));
        }

        var operators =
            await _repository.GetAllByCompanyAsync(
                request.CompanyId,
                cancellationToken);

        return operators
            .Select(x => new CallOperatorDto(
                x.Id,
                x.CompanyId,
                x.UserId,
                x.DisplayName,
                x.ExtensionNumber,
                x.Status,
                x.CurrentCallId,
                x.MaxConcurrentCalls,
                x.ActiveCallCount,
                x.CanReceiveCalls,
                x.LastActivityAt,
                x.LastStatusChangedAt))
            .ToList();
    }
}