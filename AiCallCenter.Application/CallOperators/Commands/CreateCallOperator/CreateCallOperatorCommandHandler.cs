using AiCallCenter.Application.CallOperators.Dtos;
using AiCallCenter.Application.CallOperators.Interfaces;
using AiCallCenter.Domain.Entities;
using MediatR;

namespace AiCallCenter.Application.CallOperators.Commands
    .CreateCallOperator;

public sealed class CreateCallOperatorCommandHandler
    : IRequestHandler<
        CreateCallOperatorCommand,
        CallOperatorDto>
{
    private readonly ICallOperatorRepository _repository;

    public CreateCallOperatorCommandHandler(
        ICallOperatorRepository repository)
    {
        _repository = repository;
    }

    public async Task<CallOperatorDto> Handle(
        CreateCallOperatorCommand request,
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

        var alreadyExists =
            await _repository.UserAlreadyOperatorAsync(
                request.CompanyId,
                request.UserId,
                cancellationToken);

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                "Bu kullanıcı zaten operatör olarak kayıtlı.");
        }

        if (!string.IsNullOrWhiteSpace(
            request.ExtensionNumber))
        {
            var extensionExists =
                await _repository.ExtensionNumberExistsAsync(
                    request.CompanyId,
                    request.ExtensionNumber,
                    cancellationToken:
                        cancellationToken);

            if (extensionExists)
            {
                throw new InvalidOperationException(
                    "Bu dahili numara başka bir operatör tarafından kullanılıyor.");
            }
        }

        var callOperator = new CallOperator(
            request.CompanyId,
            request.UserId,
            request.DisplayName,
            request.ExtensionNumber,
            request.MaxConcurrentCalls);

        callOperator.UpdateSettings(
            request.DisplayName,
            request.ExtensionNumber,
            request.MaxConcurrentCalls,
            request.CanReceiveCalls);

        await _repository.AddAsync(
            callOperator,
            cancellationToken);

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