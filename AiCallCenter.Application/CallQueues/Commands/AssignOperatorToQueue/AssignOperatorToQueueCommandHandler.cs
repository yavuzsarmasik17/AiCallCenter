using AiCallCenter.Application.CallQueues.Dtos;
using AiCallCenter.Application.CallQueues.Interfaces;
using AiCallCenter.Domain.Entities;
using MediatR;

namespace AiCallCenter.Application.CallQueues.Commands
    .AssignOperatorToQueue;

public sealed class AssignOperatorToQueueCommandHandler
    : IRequestHandler<
        AssignOperatorToQueueCommand,
        CallQueueOperatorDto>
{
    private readonly ICallQueueRepository _queueRepository;

    private readonly ICallQueueOperatorRepository
        _operatorRepository;

    public AssignOperatorToQueueCommandHandler(
        ICallQueueRepository queueRepository,
        ICallQueueOperatorRepository operatorRepository)
    {
        _queueRepository = queueRepository;
        _operatorRepository = operatorRepository;
    }

    public async Task<CallQueueOperatorDto> Handle(
        AssignOperatorToQueueCommand request,
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

        if (request.CallOperatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "Operatör kimliği geçersiz.");
        }

        if (request.Priority < 1 ||
            request.Priority > 100)
        {
            throw new ArgumentException(
                "Operatör önceliği 1 ile 100 arasında olmalıdır.");
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

        var operatorExists =
            await _operatorRepository.OperatorExistsAsync(
                request.CompanyId,
                request.CallOperatorId,
                cancellationToken);

        if (!operatorExists)
        {
            throw new KeyNotFoundException(
                "Çağrı operatörü bulunamadı.");
        }

        var assignmentExists =
            await _operatorRepository.AssignmentExistsAsync(
                request.CompanyId,
                request.CallQueueId,
                request.CallOperatorId,
                cancellationToken);

        if (assignmentExists)
        {
            throw new InvalidOperationException(
                "Bu operatör kuyruğa zaten bağlı.");
        }

        var assignment = new CallQueueOperator(
            request.CompanyId,
            request.CallQueueId,
            request.CallOperatorId,
            request.Priority);

        await _operatorRepository.AddAsync(
            assignment,
            cancellationToken);

        await _operatorRepository.SaveChangesAsync(
            cancellationToken);

        var savedAssignment =
            await _operatorRepository.GetByIdAsync(
                request.CompanyId,
                request.CallQueueId,
                assignment.Id,
                cancellationToken);

        if (savedAssignment is null)
        {
            throw new InvalidOperationException(
                "Operatör ataması kaydedildi ancak okunamadı.");
        }

        return new CallQueueOperatorDto(
            savedAssignment.Id,
            savedAssignment.CallOperatorId,
            savedAssignment.CallOperator.DisplayName,
            savedAssignment.CallOperator.ExtensionNumber,
            (int)savedAssignment.CallOperator.Status,
            savedAssignment.Priority,
            savedAssignment.IsActive,
            savedAssignment.AssignedAt);
    }
}