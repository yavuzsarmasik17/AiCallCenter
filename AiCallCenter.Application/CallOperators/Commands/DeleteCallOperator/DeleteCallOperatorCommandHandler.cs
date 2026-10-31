using AiCallCenter.Application.CallOperators.Interfaces;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.CallOperators.Commands
    .DeleteCallOperator;

public sealed class DeleteCallOperatorCommandHandler
    : IRequestHandler<DeleteCallOperatorCommand>
{
    private readonly ICallOperatorRepository _repository;

    public DeleteCallOperatorCommandHandler(
        ICallOperatorRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        DeleteCallOperatorCommand request,
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

        if (callOperator.Status ==
                CallOperatorStatus.Busy ||
            callOperator.ActiveCallCount > 0)
        {
            throw new InvalidOperationException(
                "Aktif çağrısı bulunan operatör silinemez.");
        }

        _repository.Remove(callOperator);

        await _repository.SaveChangesAsync(
            cancellationToken);
    }
}