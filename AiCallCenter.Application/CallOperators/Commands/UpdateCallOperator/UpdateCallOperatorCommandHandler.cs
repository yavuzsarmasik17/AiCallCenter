using AiCallCenter.Application.CallOperators.Dtos;
using AiCallCenter.Application.CallOperators.Interfaces;
using MediatR;

namespace AiCallCenter.Application.CallOperators.Commands
    .UpdateCallOperator;

public sealed class UpdateCallOperatorCommandHandler
    : IRequestHandler<
        UpdateCallOperatorCommand,
        CallOperatorDto>
{
    private readonly ICallOperatorRepository _repository;

    public UpdateCallOperatorCommandHandler(
        ICallOperatorRepository repository)
    {
        _repository = repository;
    }

    public async Task<CallOperatorDto> Handle(
        UpdateCallOperatorCommand request,
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

        if (string.IsNullOrWhiteSpace(
            request.DisplayName))
        {
            throw new ArgumentException(
                "Operatör adı boş olamaz.");
        }

        if (request.MaxConcurrentCalls < 1)
        {
            throw new ArgumentException(
                "Maksimum eş zamanlı çağrı sayısı en az 1 olmalıdır.");
        }

        if (!string.IsNullOrWhiteSpace(
            request.ExtensionNumber))
        {
            var extensionExists =
                await _repository.ExtensionNumberExistsAsync(
                    request.CompanyId,
                    request.ExtensionNumber,
                    request.OperatorId,
                    cancellationToken);

            if (extensionExists)
            {
                throw new InvalidOperationException(
                    "Bu dahili numara başka bir operatör tarafından kullanılıyor.");
            }
        }

        callOperator.UpdateSettings(
            request.DisplayName,
            request.ExtensionNumber,
            request.MaxConcurrentCalls,
            request.CanReceiveCalls);

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