using AiCallCenter.Application.Calls.Dtos;
using AiCallCenter.Application.Calls.Interfaces;
using MediatR;

namespace AiCallCenter.Application.Calls.Queries.GetCalls;

public sealed class GetCallsQueryHandler
    : IRequestHandler<
        GetCallsQuery,
        IReadOnlyList<CallDto>>
{
    private readonly ICallRepository _repository;

    public GetCallsQueryHandler(
        ICallRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CallDto>> Handle(
        GetCallsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CompanyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.");
        }

        var calls = await _repository.GetAllAsync(
            request.CompanyId,
            cancellationToken);

        return calls
            .Select(call => new CallDto(
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
                call.TransferNumber))
            .ToList();
    }
}
