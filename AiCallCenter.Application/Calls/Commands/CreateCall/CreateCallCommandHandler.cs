using AiCallCenter.Application.Calls.Dtos;
using AiCallCenter.Application.Calls.Interfaces;
using AiCallCenter.Domain.Entities;
using MediatR;

namespace AiCallCenter.Application.Calls.Commands.CreateCall;

public sealed class CreateCallCommandHandler
    : IRequestHandler<CreateCallCommand, CallDto>
{
    private readonly ICallRepository _repository;

    public CreateCallCommandHandler(
        ICallRepository repository)
    {
        _repository = repository;
    }

    public async Task<CallDto> Handle(
        CreateCallCommand request,
        CancellationToken cancellationToken)
    {
        if (request.CompanyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.");
        }

        if (request.AiAgentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Yapay zekâ ajanı kimliği geçersiz.");
        }

        if (string.IsNullOrWhiteSpace(
            request.CallerNumber))
        {
            throw new ArgumentException(
                "Arayan numara boş olamaz.");
        }

        if (string.IsNullOrWhiteSpace(
            request.CalledNumber))
        {
            throw new ArgumentException(
                "Aranan numara boş olamaz.");
        }

        var companyExists =
            await _repository.CompanyExistsAsync(
                request.CompanyId,
                cancellationToken);

        if (!companyExists)
        {
            throw new KeyNotFoundException(
                "Firma bulunamadı.");
        }

        var aiAgentExists =
            await _repository.AiAgentExistsAsync(
                request.CompanyId,
                request.AiAgentId,
                cancellationToken);

        if (!aiAgentExists)
        {
            throw new KeyNotFoundException(
                "Yapay zekâ ajanı bulunamadı.");
        }

        var call = new Call(
            request.CompanyId,
            request.AiAgentId,
            request.CallerNumber,
            request.CalledNumber,
            request.Direction);

        await _repository.AddAsync(
            call,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return new CallDto(
            call.Id,
            call.CompanyId,
            call.AiAgentId,
            null,
            call.ProviderCallId,
            call.CallerNumber,
            call.CalledNumber,
            call.Direction,
            call.Status,
            call.StartedAt,
            call.AnsweredAt,
            call.EndedAt,
            call.DurationSeconds,
            call.Summary,
            call.Result,
            call.TransferredToHuman,
            call.TransferNumber);
    }
}
