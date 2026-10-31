using AiCallCenter.Application.Calls.Dtos;
using AiCallCenter.Application.Calls.Interfaces;
using MediatR;

namespace AiCallCenter.Application.Calls.Queries.GetCallById;

public sealed class GetCallByIdQueryHandler
    : IRequestHandler<GetCallByIdQuery, CallDto>
{
    private readonly ICallRepository _repository;

    public GetCallByIdQueryHandler(
        ICallRepository repository)
    {
        _repository = repository;
    }

    public async Task<CallDto> Handle(
        GetCallByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CompanyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.");
        }

        if (request.CallId == Guid.Empty)
        {
            throw new ArgumentException(
                "Çağrı kimliği geçersiz.");
        }

        var call = await _repository.GetByIdAsync(
            request.CompanyId,
            request.CallId,
            cancellationToken);

        if (call is null)
        {
            throw new KeyNotFoundException(
                "Çağrı bulunamadı.");
        }

        return new CallDto(
            call.Id,
            call.CompanyId,
            call.AiAgentId,
            call.AiAgent?.Name,
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
