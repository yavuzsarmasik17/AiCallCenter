using AiCallCenter.Application.Calls.Dtos;
using AiCallCenter.Application.Calls.Interfaces;
using AiCallCenter.Application.Common.Realtime;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.Calls.Commands.SendOperatorMessage;

public sealed class SendOperatorMessageCommandHandler
    : IRequestHandler<
        SendOperatorMessageCommand,
        CallMessageDto>
{
    private readonly ICallRepository _callRepository;
    private readonly ICallMessageRepository _callMessageRepository;
    private readonly ICallRealtimeNotifier _realtimeNotifier;

    public SendOperatorMessageCommandHandler(
        ICallRepository callRepository,
        ICallMessageRepository callMessageRepository,
        ICallRealtimeNotifier realtimeNotifier)
    {
        _callRepository = callRepository;
        _callMessageRepository = callMessageRepository;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<CallMessageDto> Handle(
        SendOperatorMessageCommand request,
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

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ArgumentException(
                "Operatör mesajı boş olamaz.");
        }

        var content = request.Message.Trim();

        if (content.Length > 4000)
        {
            throw new ArgumentException(
                "Operatör mesajı en fazla 4000 karakter olabilir.");
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

        /*
         * Operatör mesajı yalnızca insan temsilciye
         * aktarılmış çağrılarda gönderilebilir.
         *
         * Böylece normal AI çağrılarına yanlışlıkla
         * operatör mesajı eklenemez.
         */
        if (!call.TransferredToHuman)
        {
            throw new InvalidOperationException(
                "Bu çağrı insan temsilciye aktarılmamış.");
        }

        if (
            call.Status == CallStatus.Completed ||
            call.Status == CallStatus.Cancelled ||
            call.Status == CallStatus.Failed)
        {
            throw new InvalidOperationException(
                "Tamamlanmış, iptal edilmiş veya başarısız çağrıya mesaj gönderilemez.");
        }

        var sequenceNumber =
            await _callMessageRepository
                .GetNextSequenceNumberAsync(
                    request.CompanyId,
                    request.CallId,
                    cancellationToken);

        var message = new CallMessage(
            request.CallId,
            MessageSpeaker.HumanAgent,
            content,
            sequenceNumber);

        await _callMessageRepository.AddAsync(
            message,
            cancellationToken);

        await _callMessageRepository.SaveChangesAsync(
            cancellationToken);

        var dto = ToDto(message);

        await _realtimeNotifier.MessageCreatedAsync(
            new CallRealtimeMessage(
                message.Id,
                request.CompanyId,
                message.CallId,
                message.Speaker,
                message.Content,
                message.SpokenAt,
                message.SequenceNumber,
                message.ConfidenceScore,
                message.AudioFileUrl),
            cancellationToken);

        return dto;
    }

    private static CallMessageDto ToDto(
        CallMessage message)
    {
        return new CallMessageDto(
            message.Id,
            message.CallId,
            message.Speaker,
            message.Content,
            message.SpokenAt,
            message.SequenceNumber,
            message.ConfidenceScore,
            message.AudioFileUrl);
    }
}