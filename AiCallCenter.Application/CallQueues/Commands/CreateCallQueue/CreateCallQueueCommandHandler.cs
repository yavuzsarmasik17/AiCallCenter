using AiCallCenter.Application.CallQueues.Dtos;
using AiCallCenter.Application.CallQueues.Interfaces;
using AiCallCenter.Domain.Entities;
using MediatR;

namespace AiCallCenter.Application.CallQueues.Commands
    .CreateCallQueue;

public sealed class CreateCallQueueCommandHandler
    : IRequestHandler<CreateCallQueueCommand, CallQueueDto>
{
    private readonly ICallQueueRepository _repository;

    public CreateCallQueueCommandHandler(
        ICallQueueRepository repository)
    {
        _repository = repository;
    }

    public async Task<CallQueueDto> Handle(
        CreateCallQueueCommand request,
        CancellationToken cancellationToken)
    {
        if (request.CompanyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "Kuyruk adı boş olamaz.");
        }

        var nameExists =
            await _repository.NameExistsAsync(
                request.CompanyId,
                request.Name,
                cancellationToken: cancellationToken);

        if (nameExists)
        {
            throw new InvalidOperationException(
                "Bu isimde bir çağrı kuyruğu zaten mevcut.");
        }

        if (request.AiAgentId.HasValue)
        {
            var agentExists =
                await _repository.AiAgentExistsAsync(
                    request.CompanyId,
                    request.AiAgentId.Value,
                    cancellationToken);

            if (!agentExists)
            {
                throw new KeyNotFoundException(
                    "Aktif AI ajanı bulunamadı.");
            }
        }

        var callQueue = new CallQueue(
            request.CompanyId,
            request.Name,
            request.Description,
            request.Priority,
            request.MaxWaitingSeconds,
            request.MaxQueueSize,
            request.AiAgentId);

        await _repository.AddAsync(
            callQueue,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return new CallQueueDto(
            callQueue.Id,
            callQueue.CompanyId,
            callQueue.AiAgentId,
            null,
            callQueue.Name,
            callQueue.Description,
            callQueue.Priority,
            callQueue.MaxWaitingSeconds,
            callQueue.MaxQueueSize,
            callQueue.IsActive,
            0,
            0);
    }
}