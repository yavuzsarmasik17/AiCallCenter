using AiCallCenter.Application.Calls.Dtos;
using AiCallCenter.Application.Calls.Interfaces;
using AiCallCenter.Application.Common.Realtime;
using AiCallCenter.Domain.Entities;
using MediatR;

namespace AiCallCenter.Application.Calls.Commands.CreateCallMessage;

public sealed class CreateCallMessageCommandHandler
    : IRequestHandler<
        CreateCallMessageCommand,
        CallMessageDto>
{
    private readonly ICallRepository _callRepository;

    private readonly ICallMessageRepository
        _callMessageRepository;

    private readonly ICallRealtimeNotifier
        _realtimeNotifier;

    public CreateCallMessageCommandHandler(
        ICallRepository callRepository,
        ICallMessageRepository callMessageRepository,
        ICallRealtimeNotifier realtimeNotifier)
    {
        _callRepository = callRepository;

        _callMessageRepository =
            callMessageRepository;

        _realtimeNotifier =
            realtimeNotifier;
    }

    public async Task<CallMessageDto> Handle(
        CreateCallMessageCommand request,
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

        if (!Enum.IsDefined(request.Speaker))
        {
            throw new ArgumentException(
                "Mesaj konuşmacısı geçersiz.");
        }

        if (string.IsNullOrWhiteSpace(
                request.Content))
        {
            throw new ArgumentException(
                "Mesaj içeriği boş olamaz.");
        }

        var call =
            await _callRepository.GetByIdAsync(
                request.CompanyId,
                request.CallId,
                cancellationToken);

        if (call is null)
        {
            throw new KeyNotFoundException(
                "Çağrı bulunamadı.");
        }

        var nextSequenceNumber =
            await _callMessageRepository
                .GetNextSequenceNumberAsync(
                    request.CompanyId,
                    request.CallId,
                    cancellationToken);

        var callMessage = new CallMessage(
            request.CallId,
            request.Speaker,
            request.Content,
            nextSequenceNumber);

        if (request.ConfidenceScore.HasValue)
        {
            callMessage.SetConfidenceScore(
                request.ConfidenceScore.Value);
        }

        callMessage.SetAudioFileUrl(
            request.AudioFileUrl);

        await _callMessageRepository.AddAsync(
            callMessage,
            cancellationToken);

        await _callMessageRepository
            .SaveChangesAsync(
                cancellationToken);

        var dto = new CallMessageDto(
            callMessage.Id,
            callMessage.CallId,
            callMessage.Speaker,
            callMessage.Content,
            callMessage.SpokenAt,
            callMessage.SequenceNumber,
            callMessage.ConfidenceScore,
            callMessage.AudioFileUrl);

        await _realtimeNotifier.MessageCreatedAsync(
            new CallRealtimeMessage(
                callMessage.Id,
                request.CompanyId,
                callMessage.CallId,
                callMessage.Speaker,
                callMessage.Content,
                callMessage.SpokenAt,
                callMessage.SequenceNumber,
                callMessage.ConfidenceScore,
                callMessage.AudioFileUrl),
            cancellationToken);

        return dto;
    }
}