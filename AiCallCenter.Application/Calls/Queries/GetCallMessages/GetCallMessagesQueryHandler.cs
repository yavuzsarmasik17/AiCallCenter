using AiCallCenter.Application.Calls.Dtos;
using AiCallCenter.Application.Calls.Interfaces;
using MediatR;

namespace AiCallCenter.Application.Calls.Queries.GetCallMessages;

public sealed class GetCallMessagesQueryHandler
    : IRequestHandler<
        GetCallMessagesQuery,
        IReadOnlyList<CallMessageDto>>
{
    private readonly ICallRepository _callRepository;
    private readonly ICallMessageRepository _callMessageRepository;

    public GetCallMessagesQueryHandler(
        ICallRepository callRepository,
        ICallMessageRepository callMessageRepository)
    {
        _callRepository = callRepository;
        _callMessageRepository = callMessageRepository;
    }

    public async Task<IReadOnlyList<CallMessageDto>> Handle(
        GetCallMessagesQuery request,
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

        var call = await _callRepository.GetByIdAsync(
            request.CompanyId,
            request.CallId,
            cancellationToken);

        if (call is null)
        {
            throw new KeyNotFoundException(
                "Çağrı bulunamadı.");
        }

        var messages =
            await _callMessageRepository.GetByCallIdAsync(
                request.CompanyId,
                request.CallId,
                cancellationToken);

        return messages
            .Select(x => new CallMessageDto(
                x.Id,
                x.CallId,
                x.Speaker,
                x.Content,
                x.SpokenAt,
                x.SequenceNumber,
                x.ConfidenceScore,
                x.AudioFileUrl))
            .ToList();
    }
}